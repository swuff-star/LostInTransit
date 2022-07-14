using Moonstorm;
using RoR2;
using R2API;
using RoR2.Items;
using Moonstorm.Components;

namespace LostInTransit.Buffs
{
    [DisabledContent]
    public class DiceMove : BuffBase
    {
        public override BuffDef BuffDef { get; } = LITAssets.LoadAsset<BuffDef>("DiceMove");

        public class DiceMoveBehavior : BaseBuffBodyBehavior, IBodyStatArgModifier
        {
            [BuffDefAssociation(useOnClient = true, useOnServer = true)]
            public static BuffDef GetBuffDef() => LITContent.Buffs.DiceMove;

            public void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args)
            {
                args.moveSpeedMultAdd += (Items.BlessedDice.moveAmount / 100);
            }
        }
    }
}