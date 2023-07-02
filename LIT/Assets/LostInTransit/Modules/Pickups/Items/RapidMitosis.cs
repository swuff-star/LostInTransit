using Moonstorm;
using R2API;
using RoR2.Items;
using On.RoR2;
using RoR2;
using System;
using UnityEngine;

namespace LostInTransit.Items
{
    public class RapidMitosis : ItemBase
    {
        private const string token = "LIT_ITEM_RAPIDMITOSIS_DESC";
        public override RoR2.ItemDef ItemDef { get; } = LITAssets.LoadAsset<RoR2.ItemDef>("RapidMitosis", LITBundle.Items);

        [ConfigurableField(ConfigName = "Equipment CDR Amount", ConfigDesc = "Equipment Cooldown Reduction per Rapid Mitosis.")]
        [TokenModifier(token, StatTypes.Default, 0)]
        public static float mitosisEquipCD = 0.30f;

        [ConfigurableField(ConfigName = "Skill CDR Amount", ConfigDesc = "Skill Cooldown Reduction granted via Rapid Mitosis.")]
        [TokenModifier(token, StatTypes.Default, 1)]
        public static float mitosisSkillCD = 0.4f;

        [ConfigurableField(ConfigName = "Skill CDR Length", ConfigDesc = "Duration of the buff granted via Rapid Mitosis.")]
        [TokenModifier(token, StatTypes.Default, 2)]
        public static float mitosisDur = 6f;

        /*[ConfigurableField(ConfigName = "Regeneration Amount", ConfigDesc = "Extra health regen given by Rapid Mitosis.")]
        [TokenModifier(token, StatTypes.Default, 1)]
        public static float mitosisRegen = 3.6f;*/


        public class RapidMitosisBehavior : BaseItemBodyBehavior, IBodyStatArgModifier  // I don't even think this needs an itemBehavior anymore.
        {
            [ItemDefAssociation(useOnClient = true, useOnServer = true)]
            public static RoR2.ItemDef GetItemDef() => LITContent.Items.RapidMitosis;

            public void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args)
            {
                /*if (body.equipmentSlot.stock >= 1f)
                { args.baseRegenAdd += mitosisRegen + ((mitosisRegen / 2) * (stack - 1)); }*/
            }
        }
    }
}
