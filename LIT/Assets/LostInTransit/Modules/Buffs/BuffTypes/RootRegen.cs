using Moonstorm;
using RoR2;
using UnityEngine;
using R2API;
using Moonstorm.Components;

namespace LostInTransit.Buffs
{
    public class RootRegen : BuffBase
    {
        public override BuffDef BuffDef { get; } = LITAssets.LoadAsset<BuffDef>("RootRegen");
        public class NuggetRegenBehavior : BaseBuffBodyBehavior, IBodyStatArgModifier
        {
            [BuffDefAssociation(useOnServer = true, useOnClient = true)]
            public static BuffDef GetBuffDef() => LITContent.Buffs.RootRegen;

            public void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args)
            {
                args.baseRegenAdd += Items.BitterRoot.rootRegen * buffStacks;
            }
        }
    }
}
