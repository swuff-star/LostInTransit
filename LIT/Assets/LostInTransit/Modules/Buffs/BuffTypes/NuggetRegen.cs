using Moonstorm;
using RoR2;
using UnityEngine;
using R2API;
using Moonstorm.Components;

namespace LostInTransit.Buffs
{
    //[DisabledContent]
    public class NuggetRegen : BuffBase
    {
        public override BuffDef BuffDef { get;} = LITAssets.LoadAsset<BuffDef>("NuggetRegen");

        public override void Initialize()
        {
            var schmeat = Resources.Load<BuffDef>("buffdefs/MeatRegenBoost");
            BuffDef.iconSprite = schmeat.iconSprite;
            BuffDef.startSfx = schmeat.startSfx;
        }

        public class NuggetRegenBehavior : BaseBuffBodyBehavior, IBodyStatArgModifier
        {
            [BuffDefAssociation(useOnServer = true, useOnClient = true)]
            public static BuffDef GetBuffDef() => LITContent.Buffs.NuggetRegen;

            public void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args)
            {
                args.baseRegenAdd += Items.MeatNugget.regenAdded * buffStacks;
            }
        }
    }
}
