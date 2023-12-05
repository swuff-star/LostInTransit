using LostInTransit.Equipments;
using System.Collections.Generic;
using RoR2;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections.ObjectModel;
using LostInTransit.Elites;

namespace LostInTransit.Components
{
    public class BlightDirector : MonoBehaviour
    {
        public const float MAX_SPAWN_RATE = 1f;
        public const float MIN_TIME_BEFORE_KILLS_COUNT = 1200f;
        public const float SPAWN_RATE_PER_MONSTER_KILLED = 0.001f;

        public static BlightDirector Instance { get; private set; }

        ReadOnlyCollection<PlayerCharacterMasterController> PlayerCharacterMasters => PlayerCharacterMasterController.instances;

        public Run Run { get; private set; }

        public DifficultyDef RunDifficulty => DifficultyCatalog.GetDifficultyDef(Run.selectedDifficulty);

        public float MaxSpawnRate => (MAX_SPAWN_RATE * RunDifficulty.scalingValue) + GetBeadCount();

        public float MinTimeBeforeKillsCount => (MIN_TIME_BEFORE_KILLS_COUNT / RunDifficulty.scalingValue);

        public float CurrentSpawnRate { get; private set; }
        public ulong MonstersKilled
        {
            get
            {
                return _monstersKilled;
            }
            set
            {
                if(_monstersKilled != value)
                {
                    _monstersKilled = (ulong)Mathf.Max(0, value);
                    RecalculateSpawnChance();
                }
            }
        }
        private ulong _monstersKilled;
        public bool IsPrestigeActive => RunArtifactManager.instance.IsArtifactEnabled(LITContent.Artifacts.Prestige);
        public bool IsHonorActive => RunArtifactManager.instance.IsArtifactEnabled(RoR2Content.Artifacts.eliteOnlyArtifactDef);
        public bool IsSwarmsActive => RunArtifactManager.instance.IsArtifactEnabled(RoR2Content.Artifacts.swarmsArtifactDef);

        private SceneDef _moonScene;
        private SceneDef _moon2Scene;

        private void Awake()
        {
            if (!BlightedElites.Initialized)
            {
                Destroy(this);
                return;
            }

            _moonScene = SceneCatalog.GetSceneDefFromSceneName("moon");
            _moon2Scene = SceneCatalog.GetSceneDefFromSceneName("moon2");
            Run = GetComponentInParent<Run>();
        }

        private void Start()
        {
            Instance = this;
            RecalculateSpawnChance();

            GlobalEventManager.onCharacterDeathGlobal += OnEnemyKilled;
            CharacterBody.onBodyStartGlobal += TrySpawn;
        }

        //Who knew fucking guard clauses where good? -N
        private void TrySpawn(CharacterBody obj)
        {
            if (!Stage.instance)
                return;

            var checkRoll = Util.CheckRoll(CurrentSpawnRate);

            if (!checkRoll)
                return;

            var currentSceneDef = Stage.instance.sceneDef;

            if (currentSceneDef == _moon2Scene || currentSceneDef == _moonScene)
                return;

            var teamIndex = obj.teamComponent.teamIndex;
            if (!IsEnemyTeam(teamIndex))
                return;

            bool canChampionBeBlighted = IsHonorActive || true; //change true constant to config check
            if (!canChampionBeBlighted && obj.isChampion)
                return;

            MakeBlighted(obj);
        }

        private void OnEnemyKilled(DamageReport obj)
        {
            if(!(Run.GetRunStopwatch() > MinTimeBeforeKillsCount))
            {
                return;
            }

            if (!(CurrentSpawnRate < MaxSpawnRate))
                return;

            var victimBody = obj.victimBody;
            var attackerBody = obj.attackerBody;

            if (!victimBody || !attackerBody)
                return;

            var victimTeam = obj.victimTeamIndex;
            if (!IsEnemyTeam(victimTeam))
                return;

            var attackerTeam = obj.attackerTeamIndex;
            if (attackerTeam != TeamIndex.Player)
                return;

            MonstersKilled += 1 * ((ulong)Run.loopClearCount + 1);
        }

        private void MakeBlighted(CharacterBody body)
        {
            MonstersKilled -= CalculateCostToTurnBlighted(body);

            var inventory = body.inventory;

            if (inventory && NetworkServer.active)
            {
                inventory.SetEquipmentIndex(LITContent.Equipments.AffixBlighted.equipmentIndex);
                inventory.RemoveItem(RoR2Content.Items.BoostHp, inventory.GetItemCount(RoR2Content.Items.BoostHp));
                inventory.RemoveItem(RoR2Content.Items.BoostDamage, inventory.GetItemCount(RoR2Content.Items.BoostDamage));
            }

            DeathRewards deathRewards = body.GetComponent<DeathRewards>();
            if(deathRewards)
            {
                deathRewards.expReward *= 7;
                deathRewards.goldReward *= 7;
            }
        }

        private ulong CalculateCostToTurnBlighted(CharacterBody body)
        {
            ulong cost = 5;
            switch(body.hullClassification)
            {
                case HullClassification.Human:
                    break;
                case HullClassification.Golem:
                    cost += 5;
                    break;
                case HullClassification.BeetleQueen:
                    cost += 10;
                    break;
            }

            if(body.isChampion)
            {
                cost += 10;
            }
            return cost;
        }

        private int GetBeadCount()
        {
            int sharedBeadCount = 0;
            for (int i = 0; i < PlayerCharacterMasters.Count; i++)
            {
                var playableMaster = PlayerCharacterMasters[i];
                if (!playableMaster)
                    continue;

                var master = playableMaster.master;
                if (!master)
                    continue;

                var inventory = master.inventory;
                if (!inventory)
                    continue;

                sharedBeadCount += inventory.GetItemCount(RoR2Content.Items.LunarTrinket);
            }
            return sharedBeadCount;
        }
        private bool IsEnemyTeam(TeamIndex index)
        {
            return index == TeamIndex.Monster || index == TeamIndex.Lunar || index == TeamIndex.Void;
        }

        private void RecalculateSpawnChance()
        {
            if (IsPrestigeActive)
            {
                CurrentSpawnRate = 10f;
                return;
            }

            float baseSpawnChance = 0;
            var runScalingValue = RunDifficulty.scalingValue;
            if(RunDifficulty.scalingValue > 3)
            {
                baseSpawnChance = 0.1f * (runScalingValue - 2);
            }

            float monstersKilledModifier = 0;
            int divisor = IsSwarmsActive ? 2 : 1;
            monstersKilledModifier = (MonstersKilled * SPAWN_RATE_PER_MONSTER_KILLED * runScalingValue) / divisor;

            float finalSpawnChance = baseSpawnChance + monstersKilledModifier;
            CurrentSpawnRate = Mathf.Min(finalSpawnChance, MaxSpawnRate);
        }

        private void OnDestroy()
        {
            GlobalEventManager.onCharacterDeathGlobal -= OnEnemyKilled;
            CharacterBody.onBodyStartGlobal -= TrySpawn;
        }
    }
}