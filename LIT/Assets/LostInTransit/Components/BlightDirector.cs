﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using RoR2;
using Moonstorm;
using System;
using LostInTransit.Modules;
using System.Linq;
using UnityEngine.SceneManagement;
using LostInTransit.Utils;

namespace LostInTransit.Components
{
    public class BlightDirector : NetworkBehaviour
    {
        public static BlightDirector Instance { get; private set; }

        private PlayerCharacterMasterController[] PlayerCharMasters { get => PlayerCharacterMasterController.instances.ToArray(); }

        public DifficultyDef RunDifficulty { get => DifficultyCatalog.GetDifficultyDef(Run.instance.selectedDifficulty); }

        public const float maxSpawnRate = 1f;

        public float MaxSpawnRateWithDiffCoef { get => (maxSpawnRate * RunDifficulty.scalingValue) + GetTotalBeadCount(); }

        [SyncVar]
        public float SpawnRate = 0;

        public const float spawnRatePerMonsterKilled = 0.001f;

        [SyncVar]
        private ulong monstersKilled;

        private bool IsPrestigeArtifactEnabled
        {
            get
            {
                var blightArtifact = Assets.LITAssets.LoadAsset<ArtifactDef>("Prestige");
                if(blightArtifact)
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

        private CharacterBody[] BlacklistedBodies { get => Elites.Blight.blacklistedBodies.ToArray(); }

        private SceneDef CommencementScene { get => SceneCatalog.GetSceneDefFromSceneName("moon2"); }

        private EquipmentIndex BlightedEquipIndex { get => Assets.LITAssets.LoadAsset<EquipmentDef>("AffixBlighted").equipmentIndex; }
        
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
            var victimBody = obj.victimBody;
            var attackerBody = obj.attackerBody;
            if(victimBody && attackerBody)
            {
                var victimTeamComponent = victimBody.teamComponent;
                if(victimTeamComponent)
                {
                    if(victimTeamComponent.teamIndex == TeamIndex.Monster || victimTeamComponent.teamIndex == TeamIndex.Lunar)
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

        private void TrySpawn(CharacterBody body)
        {
            var stageNotCommencement = (Stage.instance?.sceneDef?.sceneDefIndex != CommencementScene.sceneDefIndex);
            var isEnemy = (body.teamComponent?.teamIndex != TeamIndex.Player);
            var hasComponent = ((bool)body.master?.GetComponent<BlightedController>());
            var isntChampion = !body.isChampion;
            var isBlacklisted = CheckBlacklist(body);

            if (isBlacklisted)
                return;
            if(IsHonorArtifactEnabled)
            {
                if (stageNotCommencement && isEnemy && hasComponent)
                {
                    if (Util.CheckRoll(SpawnRate))
                    {
                        MakeBlighted(body);
                    }
                }
            }
            else if(stageNotCommencement && isEnemy && hasComponent && isntChampion)
            {
                if (Util.CheckRoll(SpawnRate))
                {
                    MakeBlighted(body);
                }
            }     
        }

        private bool CheckBlacklist(CharacterBody body)
        {
            return BlacklistedBodies.Contains(body);
        }

        private void MakeBlighted(CharacterBody body)
        {
            if (body.master?.GetComponent<BlightedController>())
            {
                monstersKilled -= 10;
                var inventory = body.inventory;

                inventory.SetEquipmentIndex(BlightedEquipIndex);
                body.isElite = true;

                inventory.RemoveItem(RoR2Content.Items.BoostHp, inventory.GetItemCount(RoR2Content.Items.BoostHp));
                inventory.RemoveItem(RoR2Content.Items.BoostDamage, inventory.GetItemCount(RoR2Content.Items.BoostDamage));

                DeathRewards rewards = body.GetComponent<DeathRewards>();
                if (rewards)
                {
                    rewards.expReward *= 5;
                    rewards.goldReward *= 5;
                }
            }
        }

        [Server]
        private void RecalculateSpawnChance()
        {
            if(IsPrestigeArtifactEnabled)
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