using Moonstorm;
using RoR2;
using R2API;
using RoR2.Items;
using Moonstorm.Components;

namespace LostInTransit.Buffs
{
    public class HitListMarked : BuffBase
    {
        public override BuffDef BuffDef { get; } = LITAssets.LoadAsset<BuffDef>("bdHitListMarked", LITBundle.Items);

        public class HitListMarkedBehavior : BaseBuffBodyBehavior, IOnKilledServerReceiver
        {
            [BuffDefAssociation(useOnServer = true, useOnClient = true)]
            public static BuffDef GetBuffDef() => LITContent.Buffs.bdHitListMarked;

            public void OnKilledServer(DamageReport damageReport)
            {
                if (damageReport.attackerBody)
                {
                    damageReport.attackerBody.AddTimedBuffAuthority(LITContent.Buffs.bdHitListBuff.buffIndex, Items.HitList.buffDur);
                }
            }
        }
    }
}
