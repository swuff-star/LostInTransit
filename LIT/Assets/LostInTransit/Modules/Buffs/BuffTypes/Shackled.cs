using Moonstorm;
using Moonstorm.Components;
using RoR2;

namespace LostInTransit.Buffs
{
    public class Shackled : BuffBase
    {
        public override BuffDef BuffDef { get; } = LITAssets.LoadAsset<BuffDef>("Shackled");

        //G - this still uses IStatItemBehaviour because the slow needs to be applied after all other modifiers have been added, which isn't supported by RecalcStatsAPI yet
        public class ShackledDebuffBehavior : BaseBuffBodyBehavior, IStatItemBehavior
        {
            [BuffDefAssociation(useOnClient = true, useOnServer = true)]
            public static BuffDef GetBuffDef() => LITContent.Buffs.Shackled;

            public void RecalculateStatsEnd()
            {
                body.attackSpeed *= (1 - Items.PrisonShackles.slowMultiplier);
            }

            public void RecalculateStatsStart()
            {
            }
        }
    }
}