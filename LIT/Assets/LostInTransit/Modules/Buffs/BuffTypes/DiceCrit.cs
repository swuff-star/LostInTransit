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
        public override BuffDef BuffDef { get; } = LITAssets.LoadAsset<BuffDef>("bdDiceCrit", LITBundle.Items);

        public class DiceCritBehavior : BaseBuffBodyBehavior, IBodyStatArgModifier
        {
            public static BuffDef GetBuffDef() => LITContent.Buffs.bdDiceCrit;

            public void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args)
            {
                args.critAdd += Items.BlessedDice.critAmount;
            }
        }
    }
}