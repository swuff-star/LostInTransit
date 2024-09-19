using LostInTransit.Equipments;
using System.Collections.Generic;
using RoR2;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections.ObjectModel;
using LostInTransit.Elites;
using System.Runtime.CompilerServices;

namespace LostInTransit.Components
{
    public class BlightDirector : MonoBehaviour
    {
        public const string KILL_COUNT_KEY = "LIT_" + nameof(monstersKilled);
        public const float MAX_SPAWN_RATE = 1f;
        public const float MIN_TIME_BEFORE_KILLS_COUNT = 1200f;
        public const float SPAWN_RATE_PER_MONSTER_KILLED = 0.001f;

        public static BlightDirector instance { get; private set; }

        ReadOnlyCollection<PlayerCharacterMasterController> playerCharacterMasters => PlayerCharacterMasterController.instances;

        public Run Run { get; private set; }

        public DifficultyDef runDifficulty => DifficultyCatalog.GetDifficultyDef(Run.selectedDifficulty);

        public float maxSpawnRate => (MAX_SPAWN_RATE * runDifficulty.scalingValue) + GetBeadCount();

        public float minTimeBeforeKillsCount => (MIN_TIME_BEFORE_KILLS_COUNT / runDifficulty.scalingValue);

        public float currentSpawnRate { get; private set; }
        public ulong monstersKilled
        {
            get
            {
                return _monstersKilled;
            }
            set
            {
                if (_monstersKilled != value)
                {
                    _monstersKilled = (ulong)Mathf.Max(0, value);
                    RecalculateSpawnChance();
                }
            }
        }
        private ulong _monstersKilled;
        public bool isPrestigeActive => RunArtifactManager.instance.IsArtifactEnabled(LITContent.Artifacts.Prestige);
        public bool isHonorActive => RunArtifactManager.instance.IsArtifactEnabled(RoR2Content.Artifacts.eliteOnlyArtifactDef);
        public bool isSwarmsActive => RunArtifactManager.instance.IsArtifactEnabled(RoR2Content.Artifacts.swarmsArtifactDef);

        private SceneDef _moonScene;
        private SceneDef _moon2Scene;

        private void Awake()
        {
            if (!BlightedElites.initialized)
            {
                Destroy(this);
                return;
            }

            _moonScene = SceneCatalog.GetSceneDefFromSceneName("moon");
            _moon2Scene = SceneCatalog.GetSceneDefFromSceneName("moon2");
            Run = GetComponentInParent<Run>();
        }

        private void OnEnable()
        {
            instance = this;

            GlobalEventManager.onCharacterDeathGlobal += OnEnemyKilled;
            CharacterSpawnCard.onSpawnedServerGlobal += TrySpawn;
            CharacterBody.onBodyStartGlobal += TrySpawn;
        }

        private void TrySpawn(SpawnCard.SpawnResult obj)
        {
        }

        private void OnDisable()
        {
            if(instance == this)
            {
                instance = null;
            }

            GlobalEventManager.onCharacterDeathGlobal -= OnEnemyKilled;
            CharacterBody.onBodyStartGlobal -= TrySpawn;
        }
        private void Start()
        {
            if(LITMain.properSaveInstalled)
            {
                RetrieveKillCountFromProperSave();
            }
            RecalculateSpawnChance();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void RetrieveKillCountFromProperSave()
        {
            if (ProperSave.Loading.CurrentSave == null)
                return;

            monstersKilled = ProperSave.Loading.CurrentSave.GetModdedData<ulong>(KILL_COUNT_KEY);
            LITLog.Message($"Retrieved ProperSave's Kill Count, MonstersKilled set to {monstersKilled}");
        }

        //Who knew fucking guard clauses where good? -N
        private void TrySpawn(CharacterBody obj)
        {
            if (!Stage.instance)
                return;

            if (IsInBlacklist(obj))
                return;

            var checkRoll = Util.CheckRoll(currentSpawnRate);

            if (!checkRoll)
                return;

            var currentSceneDef = Stage.instance.sceneDef;

            if (currentSceneDef == _moon2Scene || currentSceneDef == _moonScene)
                return;

            var teamIndex = obj.teamComponent.teamIndex;
            if (!IsEnemyTeam(teamIndex))
                return;

            bool canChampionBeBlighted = isPrestigeActive;
            if (!canChampionBeBlighted && obj.isChampion)
                return;

            MakeBlighted(obj);
        }

        private void OnEnemyKilled(DamageReport obj)
        {
            if(!(Run.GetRunStopwatch() > minTimeBeforeKillsCount))
            {
                return;
            }

            if (!(currentSpawnRate < maxSpawnRate))
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

            monstersKilled += 1 * ((ulong)Run.loopClearCount + 1);
        }

        private void MakeBlighted(CharacterBody body)
        {
            monstersKilled -= CalculateCostToTurnBlighted(body);

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
            for (int i = 0; i < playerCharacterMasters.Count; i++)
            {
                var playableMaster = playerCharacterMasters[i];
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
            if (isPrestigeActive)
            {
                currentSpawnRate = 10f;
                return;
            }

            float baseSpawnChance = 0;
            var runScalingValue = runDifficulty.scalingValue;
            if(runDifficulty.scalingValue > 3)
            {
                baseSpawnChance = 0.1f * (runScalingValue - 2);
            }

            float monstersKilledModifier = 0;
            int divisor = isSwarmsActive ? 2 : 1;
            monstersKilledModifier = (monstersKilled * SPAWN_RATE_PER_MONSTER_KILLED * runScalingValue) / divisor;

            float finalSpawnChance = baseSpawnChance + monstersKilledModifier;
            currentSpawnRate = Mathf.Min(finalSpawnChance, maxSpawnRate);
        }

        private bool IsInBlacklist(CharacterBody body)
        {
            return isPrestigeActive ? BlightedElites.prestigeBodyBlacklist.Contains(body.bodyIndex) : BlightedElites.regularBodyBlacklist.Contains(body.bodyIndex);
        }

        private void OnDestroy()
        {
            GlobalEventManager.onCharacterDeathGlobal -= OnEnemyKilled;
            CharacterBody.onBodyStartGlobal -= TrySpawn;
        }
    }
}