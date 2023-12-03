using Moonstorm;
using RoR2;
using R2API;
using RoR2.Items;
using Moonstorm.Components;

namespace LostInTransit.Buffs
{
    //[DisabledContent]
    public class TimeStopDebuff : BuffBase
    {
        public override BuffDef BuffDef { get; } = LITAssets.LoadAsset<BuffDef>("bdTimeStopDebuff", LITBundle.Equips);

        //slow needs to be applied after all other mods hence istatitembehavior
        public sealed class TimeStopDebuffBehavior : BaseBuffBodyBehavior, IStatItemBehavior
        {
            [BuffDefAssociation(useOnServer = true, useOnClient = true)]
            public static BuffDef GetBuffDef() => LITContent.Buffs.bdTimeStopDebuff;

            public void RecalculateStatsEnd()
            {
                body.moveSpeed *= 0f;
                body.attackSpeed *= 0f;
            }

            public void RecalculateStatsStart()
            {
            }
        }
    }
}