using LostInTransit.Equipments;
using RoR2;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;


namespace LostInTransit.Components
{
    public class BlightDirector : NetworkBehaviour
    {
        public static BlightDirector Instance { get; private set; }

        private PlayerCharacterMasterController[] PlayerCharMasters { get => PlayerCharacterMasterController.instances.ToArray(); }

        public DifficultyDef RunDifficulty { get => DifficultyCatalog.GetDifficultyDef(Run.instance.selectedDifficulty); }

        public const float maxSpawnRate = 1f;

        public const float minTimeBeforeKillsCount = 1200f; //This roughly causes kills to only count past 20 minutes in drizzle.

        public float MaxSpawnRateWithDiffCoef { get => (1f * RunDifficulty.scalingValue) + GetTotalBeadCount(); }

        public float MinTimeBeforeKillsCountWithDiffCoef { get => (minTimeBeforeKillsCount / RunDifficulty.scalingValue); }

        [SyncVar]
        public float SpawnRate = 0;


        public const float spawnRatePerMonsterKilled = 0.001f;

        [SyncVar]
        private ulong monstersKilled;

        private bool IsPrestigeArtifactEnabled
        {
            get
            {
                var blightArtifact = LITAssets.LoadAsset<ArtifactDef>("Prestige");
                if (blightArtifact)
                {
                    return RunArtifactManager.instance.IsArtifactEnabled(blightArtifact);
                }
                return false;
            }
        }

        private bool IsHonorArtifactEnabled
        {
            get
            {
                return RunArtifactManager.instance.IsArtifactEnabled(RoR2Content.Artifacts.eliteOnlyArtifactDef);
            }
        }

        private SceneDef CommencementScene { get => SceneCatalog.GetSceneDefFromSceneName("moon2"); }

        private EquipmentIndex BlightedEquipIndex { get => LITAssets.LoadAsset<EquipmentDef>("AffixBlighted").equipmentIndex; }

        void Start()
        {
            Instance = this;
            RecalculateSpawnChance();

            GlobalEventManager.onCharacterDeathGlobal += OnEnemyKilled;
            CharacterBody.onBodyStartGlobal += TrySpawn;
        }

        [Server]
        private void OnEnemyKilled(DamageReport obj)
        {
            if (Run.instance.GetRunStopwatch() > MinTimeBeforeKillsCountWithDiffCoef)
            {
                var victimBody = obj.victimBody;
                var attackerBody = obj.attackerBody;
                if (victimBody && attackerBody)
                {
                    var victimTeamComponent = victimBody.teamComponent;
                    if (victimTeamComponent)
                    {
                        if (victimTeamComponent.teamIndex == TeamIndex.Monster || victimTeamComponent.teamIndex == TeamIndex.Lunar)
                        {
                            if (attackerBody.isPlayerControlled && SpawnRate < MaxSpawnRateWithDiffCoef)
                            {
                                monstersKilled += 1 * ((ulong)Run.instance?.loopClearCount + 1);
                                RecalculateSpawnChance();
                            }
                        }
                    }
                }
            }
        }

        private void TrySpawn(CharacterBody body)
        {
            var stageNotCommencement = (Stage.instance?.sceneDef?.sceneDefIndex != CommencementScene.sceneDefIndex);
            var isEnemy = (body.teamComponent?.teamIndex != TeamIndex.Player);
            var hasComponent = ((bool)body.master?.GetComponent<BlightedController>());
            var isntChampion = !body.isChampion;

            if (IsHonorArtifactEnabled || AffixBlighted.bossBlighted)
            {
                if (stageNotCommencement && isEnemy && hasComponent)
                {
                    if (Util.CheckRoll(SpawnRate))
                    {
                        MakeBlighted(body);
                    }   //Commenting this out as right now, EVERYTHING is becoming Blighted; hopefully this serves as a temp fix.
                }
            }

            else if (stageNotCommencement && isEnemy && hasComponent && isntChampion)
            {
                if (Util.CheckRoll(SpawnRate))
                {
                    MakeBlighted(body);
                }
            }
        }

        private void MakeBlighted(CharacterBody body)
        {
            if (body.master?.GetComponent<BlightedController>())
            {
                monstersKilled -= 10;
                var inventory = body.inventory;

                inventory.SetEquipmentIndex(BlightedEquipIndex);
                body.isElite = true;
                body.GetComponent<CharacterMaster>().isBoss = true;
                

                inventory.RemoveItem(RoR2Content.Items.BoostHp, inventory.GetItemCount(RoR2Content.Items.BoostHp));
                inventory.RemoveItem(RoR2Content.Items.BoostDamage, inventory.GetItemCount(RoR2Content.Items.BoostDamage));

                DeathRewards rewards = body.GetComponent<DeathRewards>();
                if (rewards)
                {
                    rewards.expReward *= 7;
                    rewards.goldReward *= 7;
                }
            }
        }

        [Server]
        private void RecalculateSpawnChance()
        {
            if (IsPrestigeArtifactEnabled)
            {
                SpawnRate = 10f;
                return;
            }

            float baseSpawnChance = 0;
            if (RunDifficulty.scalingValue > 3)
                baseSpawnChance = 0.1f * (RunDifficulty.scalingValue - 2);

            float monstersKilledModifier = 0;
            int divisor = 1;
            if (RunArtifactManager.instance.IsArtifactEnabled(RoR2Content.Artifacts.Swarms))
                divisor = 2;
            monstersKilledModifier = (monstersKilled * spawnRatePerMonsterKilled * RunDifficulty.scalingValue) / divisor;

            float finalSpawnChance = baseSpawnChance + monstersKilledModifier;
            SpawnRate = Mathf.Min(finalSpawnChance, MaxSpawnRateWithDiffCoef);
        }

        private float GetTotalBeadCount()
        {
            float sharedBeadCount = 0;
            for (int i = 0; i < PlayerCharMasters.Length; i++)
            {
                var inventory = PlayerCharMasters[i].master?.inventory;
                sharedBeadCount += inventory.GetItemCount(RoR2Content.Items.LunarTrinket);
            }
            return sharedBeadCount;
        }

        void OnDestroy()
        {
            GlobalEventManager.onCharacterDeathGlobal -= OnEnemyKilled;
            CharacterBody.onBodyStartGlobal -= TrySpawn;
        }
    }
}