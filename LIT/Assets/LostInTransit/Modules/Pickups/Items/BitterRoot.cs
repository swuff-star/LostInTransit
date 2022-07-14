using Moonstorm;
using RoR2;
using RoR2.Items;
using UnityEngine;
using R2API;
using System;

namespace LostInTransit.Items
{
    //[DisabledContent]
    public class BitterRoot : ItemBase
    {
        private const string token = "LIT_ITEM_BITTERROOT_DESC";
        public override ItemDef ItemDef { get; } = LITAssets.LoadAsset<ItemDef>("BitterRoot");

        /*[ConfigurableField(LITConfig.items, ConfigName = "Extra Maximum Health per Root", ConfigDesc = "Extra percentage of maximum health added per root")]
        [TokenModifier(token, StatTypes.Default)]
        public static float rootIncrease = 4f;*/

        [ConfigurableField(LITConfig.items, ConfigName = "Regen per Root", ConfigDesc = "Amount of regen on kill per Root.")]
        [TokenModifier(token, StatTypes.Default, 0)]
        public static float rootRegen = 3f;

        [ConfigurableField(LITConfig.items, ConfigName = "Regen Duration", ConfigDesc = "Duration of regen on kill per Root.")]
        [TokenModifier(token, StatTypes.Default, 1)]
        public static float rootRegenDur = 3f;

        public class BitterRootBehavior : BaseItemBodyBehavior, IOnKilledOtherServerReceiver
        {
            [ItemDefAssociation(useOnClient = true, useOnServer = true, behaviorTypeOverride = typeof(BitterRootBehavior))]
            public static ItemDef GetItemDef() => LITContent.Items.BitterRoot;

            public void OnKilledOtherServer(DamageReport damageReport)
            {
                body.AddTimedBuffAuthority(LITContent.Buffs.RootRegen.buffIndex, rootRegenDur * stack);
            }

            /*public void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args)
            {
                args.healthMultAdd += (rootIncrease/100) * stack;  
            }*/
        }
    }
}
