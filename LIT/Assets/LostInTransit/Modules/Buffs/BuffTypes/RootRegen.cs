using Moonstorm;
using RoR2;
using UnityEngine;
using R2API;
using Moonstorm.Components;

namespace LostInTransit.Buffs
{
    public class RootRegen : BuffBase
    {
        public override BuffDef BuffDef { get; } = LITAssets.LoadAsset<BuffDef>("bdRootRegen", LITBundle.Items);
        public class RootRegenBehaviour : BaseBuffBodyBehavior, IBodyStatArgModifier
        {
            [BuffDefAssociation(useOnServer = true, useOnClient = true)]
            public static BuffDef GetBuffDef() => LITContent.Buffs.bdRootRegen;

            public void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args)
            {
                args.baseRegenAdd += (Items.BitterRoot.rootRegen + ((Items.BitterRoot.rootRegen / 5) * body.level)) * buffStacks;
            }
        }
    }
}
