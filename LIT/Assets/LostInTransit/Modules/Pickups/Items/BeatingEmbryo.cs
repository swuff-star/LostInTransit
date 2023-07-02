using Moonstorm;
using RoR2;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Random = UnityEngine.Random;

namespace LostInTransit.Items
{
    /*TODO:
     * > FireBallDash (Volcanic Egg) and Gateway (Eccentric Vase) are on blacklist, find unique upgrades (On blacklist because they use vehicles.)
     * > BFG (Preon) just lowers how fast the bfg projectile appears, must figure out how to fire a XIConstruct death laser.}
     * > Primordial cube may do nothing if doubled, need more testing
     * > Executive card may do nothing if doubled, needs more testing.
     * > Recycler may do nothing if doubled, needs more testing.
     * > Gnarled Woodsprite needs testing.
     * > Radar Scanner still useless
     * > Remote Caffeinator seems to spawn 2 vending machines, only one seems interactable? needs more testing
     * > Make the upgraded royal capacitor spawn a AOE effect alongside the lightning strike
     * > The Crowdfunder does nothing, needs more testing
     * > Add more funny messages to the used trophy hunter's tricorn
     */

    [DisabledContent]
    public class BeatingEmbryo : ItemBase
    {
        public override ItemDef ItemDef => LITAssets.LoadAsset<ItemDef>("BeatingEmbryo", LITBundle.Equips);

        public string[] bossHunterOptions = new string[] { "EQUIPMENT_BOSSHUNTERCONSUMED_CHAT", "LIT_EQUIPMENT_BOSSHUNTERCONSUMED_CHAT" };

        public override void Initialize()
        {
            SetBlacklist();
            SetUpgradedFunctions();
        }

        private void SetBlacklist()
        {
            //Volcanic egg uses a vehicle and i'm scared of the interaction, to the black list for now. same applies to the gateway.
            Addressables.LoadAssetAsync<EquipmentDef>("RoR2/Base/FireBallDash/FireBallDash.asset").Completed += AddToBlacklist;
            Addressables.LoadAssetAsync<EquipmentDef>("RoR2/Base/Gateway/Gateway.asset").Completed += AddToBlacklist;
        }

        private void SetUpgradedFunctions()
        {
            Addressables.LoadAssetAsync<EquipmentDef>("RoR2/Base/BFG/BFG.asset").Completed += (op) =>
            {
                BeatingEmbryoManager.AddEmbryoEffect(op.Result, FireBFG);
            };
            Addressables.LoadAssetAsync<EquipmentDef>("RoR2/DLC1/BossHunter/BossHunterConsumed.asset").Completed += (op) =>
            {
                BeatingEmbryoManager.AddEmbryoEffect(op.Result, FireBossHunterConsumed);
            };
            Addressables.LoadAssetAsync<EquipmentDef>("RoR2/Base/BurnNearby/BurnNearby.asset").Completed += (op) =>
            {
                BeatingEmbryoManager.AddEmbryoEffect(op.Result, FireBurnNearby);
            };
            Addressables.LoadAssetAsync<EquipmentDef>("RoR2/Base/Cleanse/Cleanse.asset").Completed += (op) =>
            {
                BeatingEmbryoManager.AddEmbryoEffect(op.Result, FireCleanse);
            };
            Addressables.LoadAssetAsync<EquipmentDef>("RoR2/Base/CritOnUse/CritOnUse.asset").Completed += (op) =>
            {
                BeatingEmbryoManager.AddEmbryoEffect(op.Result, FireCritOnUse);
            };
            Addressables.LoadAssetAsync<EquipmentDef>("RoR2/Base/GainArmor/GainArmor.asset").Completed += (op) =>
            {
                BeatingEmbryoManager.AddEmbryoEffect(op.Result, FireGainArmor);
            };
            Addressables.LoadAssetAsync<EquipmentDef>("RoR2/Base/LifestealOnHit/LifestealOnHit.asset").Completed += (op) =>
            {
                BeatingEmbryoManager.AddEmbryoEffect(op.Result, FireLifeStealOnHit);
            };
        }

        private void AddToBlacklist(UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<EquipmentDef> obj)
        {
            BeatingEmbryoManager.AddToBlackList(obj.Result);
        }

        #region Upgraded Effects
        //ToDo: XIConstruct Mega Laser
        private bool FireBFG(EquipmentSlot slot)
        {
            slot.bfgChargeTimer = 1;
            slot.subcooldownTimer = 1.1f;
            return true;
        }

        //Best we can do is a variety of messages.
        private bool FireBossHunterConsumed(EquipmentSlot slot)
        {
            if(slot.characterBody)
            {
                Chat.SendBroadcastChat(new Chat.BodyChatMessage
                {
                    bodyObject = slot.characterBody.gameObject,
                    token = bossHunterOptions[Random.Range(0, bossHunterOptions.Length)]
                });
                slot.subcooldownTimer = 1;
            }
            return true;
        }

        //Vanilla duration is 12.
        private bool FireBurnNearby(EquipmentSlot slot)
        {
            if(slot.characterBody)
            {
                slot.characterBody.AddHelfireDuration(24f);
            }
            return true;
        }

        //Idea from gaforb
        private bool FireCleanse(EquipmentSlot slot)
        {
            BlastAttack attack = new BlastAttack
            {
                attacker = slot.characterBody.gameObject,
                attackerFiltering = AttackerFiltering.NeverHitSelf,
                baseForce = 10000,
                bonusForce = Vector3.up,
                canRejectForce = false,
                damageColorIndex = DamageColorIndex.Default,
                damageType = DamageType.Stun1s,
                falloffModel = BlastAttack.FalloffModel.None,
                inflictor = slot.gameObject,
                losType = BlastAttack.LoSType.None,
                position = slot.characterBody.footPosition,
                procCoefficient = 1,
                radius = slot.characterBody.bestFitRadius,
                teamIndex = slot.characterBody.teamComponent.teamIndex
            };
            attack.Fire();
            return slot.FireCleanse();
        }

        //Vanilla duration is 8
        private bool FireCritOnUse(EquipmentSlot slot)
        {
            slot.characterBody.AddTimedBuff(RoR2Content.Buffs.FullCrit, 16f);
            return true;
        }

        //Vanilla duration is 5
        private bool FireGainArmor(EquipmentSlot slot)
        {
            slot.characterBody.AddTimedBuff(RoR2Content.Buffs.ElephantArmorBoost, 10f);
            return true;
        }

        //Vanilla duration is 8
        private bool FireLifeStealOnHit(EquipmentSlot slot)
        {
            EffectData effectData = new EffectData
            {
                origin = slot.characterBody.corePosition
            };
            effectData.SetHurtBoxReference(slot.characterBody.gameObject);
            EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/LifeStealOnHitActivation"), effectData, transmit: false);
            slot.characterBody.AddTimedBuff(RoR2Content.Buffs.LifeSteal, 16f);
            return true;
        }
        #endregion
    }

    public static class BeatingEmbryoManager
    {
        private static readonly List<EquipmentDef> blacklist = new List<EquipmentDef>();
        private static readonly Dictionary<EquipmentDef, Func<EquipmentSlot, bool>> equipToFunction = new Dictionary<EquipmentDef, Func<EquipmentSlot, bool>>();
        public static bool ManagerInitialized = false;

        public static Func<EquipmentSlot, bool> GetFunc(EquipmentDef def)
        {
            if(equipToFunction.TryGetValue(def, out var func))
            {
                return func;
            }
            return null;
        }

        public static void AddToBlackList(EquipmentDef equipmentDef)
        {
            if (ManagerInitialized)
                throw new InvalidOperationException($"Cannot add new equipment to the blacklist because the embryo manager has initialized.");
            blacklist.Add(equipmentDef);
        }

        public static void AddEmbryoEffect(EquipmentDef equipmentDef, Func<EquipmentSlot, bool> equipmentEffect)
        {
            if (ManagerInitialized)
                throw new InvalidOperationException($"Cannot add new embryo effect because the embryo manager has initialized.");
            equipToFunction.Add(equipmentDef, equipmentEffect);
        }

        [SystemInitializer(typeof(EquipmentCatalog), typeof(ItemCatalog))]
        private static void Init()
        {
            ManagerInitialized = true;
            On.RoR2.EquipmentSlot.PerformEquipmentAction += PerformUpgradedAction;
        }

        private static bool PerformUpgradedAction(On.RoR2.EquipmentSlot.orig_PerformEquipmentAction orig, EquipmentSlot self, EquipmentDef equipmentDef)
        {
            //No inventory? dont do anything funky
            if (!self.inventory)
                return orig(self, equipmentDef);
            
            //Body doesnt have a beating embryo? dont do anything funky
            if (self.inventory.GetItemCount(LITContent.Items.BeatingEmbryo) == 0)
                return orig(self, equipmentDef);

            //Is in the blacklist? dont do anything funky
            if (blacklist.Contains(equipmentDef))
                return orig(self, equipmentDef);

            //If the index has a funky function, use that instead of orig(self);
            if(equipToFunction.TryGetValue(equipmentDef, out var function))
            {
                return function(self);
            }
            //If the index does not have a funky function, use this for loop for triggering the equipment twice.
            bool result = false;
            for(int i = 0; i < 2; i++)
            {
                result = orig(self, equipmentDef);
            }
            return result;
        }
    }
}