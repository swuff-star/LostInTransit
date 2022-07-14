using Moonstorm;
using RoR2;
using R2API;
using RoR2.Items;
using Moonstorm.Components;

namespace LostInTransit.Buffs
{
    [DisabledContent]
    public class DiceCrit : BuffBase
    {
        public override BuffDef BuffDef { get; } = LITAssets.LoadAsset<BuffDef>("DiceCrit");

        public class DiceCritBehavior : BaseBuffBodyBehavior, IBodyStatArgModifier
        {
            public static BuffDef GetBuffDef() => LITContent.Buffs.DiceCrit;

            public void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args)
            {
                args.critAdd += Items.BlessedDice.critAmount;
            }
        }
    }
}