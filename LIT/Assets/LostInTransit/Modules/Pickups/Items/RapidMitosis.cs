using MSU;
using R2API;
using RoR2.Items;
using RoR2;
using System;
using UnityEngine;
using RoR2.ContentManagement;
using System.Collections;
using MSU.Config;

namespace LostInTransit.Items
{
    public sealed class RapidMitosis : LITItem
    {
        private const string TOKEN = "LIT_ITEM_RAPIDMITOSIS_DESC";

        [RiskOfOptionsConfigureField(LITConfig.ITEMS, ConfigNameOverride = "Equipment CDR Amount", ConfigDescOverride = "Equipment Cooldown Reduction per Rapid Mitosis.")]
        [FormatToken(TOKEN)]
        public static float mitosisEquipCD = 0.30f;

        [RiskOfOptionsConfigureField(LITConfig.ITEMS, ConfigNameOverride = "Skill CDR Amount", ConfigDescOverride = "Skill Cooldown Reduction granted via Rapid Mitosis.")]
        [FormatToken(TOKEN)]
        public static float mitosisSkillCD = 0.4f;

        [RiskOfOptionsConfigureField(LITConfig.ITEMS, ConfigNameOverride = "Skill CDR Length", ConfigDescOverride = "Duration of the buff granted via Rapid Mitosis.")]
        [FormatToken(TOKEN)]
        public static float mitosisDur = 6f;

        public override NullableRef<GameObject> ItemDisplayPrefab => null;
        public override ItemDef ItemDef => _itemDef;
        private ItemDef _itemDef;

        public override void Initialize()
        {
            On.RoR2.Inventory.CalculateEquipmentCooldownScale += Inventory_CalculateEquipmentCooldownScale;
            On.RoR2.EquipmentSlot.RpcOnClientEquipmentActivationRecieved += ProcMitosis;
        }

        private float Inventory_CalculateEquipmentCooldownScale(On.RoR2.Inventory.orig_CalculateEquipmentCooldownScale orig, Inventory self)
        {
            float num = orig(self);
            num *= (1 - MSUtil.InverseHyperbolicScaling(mitosisEquipCD, mitosisEquipCD, 0.7f, self.GetItemCount(LITContent.Items.RapidMitosis)));
            return num;
        }

        private static void ProcMitosis(On.RoR2.EquipmentSlot.orig_RpcOnClientEquipmentActivationRecieved orig, EquipmentSlot self)
        {
            orig(self);

            if (self.hasAuthority && self.inventory)
            {
                int mitosisCount = self.inventory.GetItemCount(LITContent.Items.RapidMitosis);
                if (mitosisCount > 0)
                {
                    self.characterBody.AddTimedBuffAuthority(LITContent.Buffs.bdMitosisBuff.buffIndex, Items.RapidMitosis.mitosisDur);
                }
            }
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public override IEnumerator LoadContentAsync()
        {
            /*
             * ItemDef - "RapidMitosis" - Items
             */
            yield break;
        }
    }
}
