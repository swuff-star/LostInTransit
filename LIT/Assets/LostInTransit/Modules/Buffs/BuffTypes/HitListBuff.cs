using Moonstorm;
using RoR2;
using R2API;
using RoR2.Items;
using Moonstorm.Components;

namespace LostInTransit.Buffs
{
    public class HitListBuff : BuffBase
    {
        public override BuffDef BuffDef { get; } = LITAssets.LoadAsset<BuffDef>("bdHitListBuff", LITBundle.Items);

        public class HitListBuffBehavior : BaseBuffBodyBehavior, IBodyStatArgModifier
        {
            [BuffDefAssociation(useOnServer = true, useOnClient = true)]
            public static BuffDef GetBuffDef() => LITContent.Buffs.bdHitListBuff;

            public void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args)
            {
                args.baseDamageAdd += args.baseDamageAdd += body.baseDamage * Items.HitList.buffDmg * body.GetBuffCount(LITContent.Buffs.bdHitListBuff);
            }

            public void OnDestroy()
            {
                body.RecalculateStats();
            }
        }
    }
}
