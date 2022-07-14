using LostInTransit.Items;
using Moonstorm;
using RoR2;
using R2API;
using Moonstorm.Components;

namespace LostInTransit.Buffs
{
    //[DisabledContent]
    public class GuardiansHeartBuff : BuffBase
    {
        public override BuffDef BuffDef { get; } = LITAssets.LoadAsset<BuffDef>("GuardiansHeartBuff");

        public class GuardiansHeartBehavior : BaseBuffBodyBehavior, IBodyStatArgModifier
        {
            [BuffDefAssociation(useOnClient = true, useOnServer = true)]
            public static BuffDef GetBuffDef() => LITContent.Buffs.GuardiansHeartBuff;

            public void Start()
            {
                if (body.healthComponent.shield >= 1f)
                {
                    body.RemoveBuff(LITContent.Buffs.GuardiansHeartBuff.buffIndex);
                }
            }

            public void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args)
            {
                args.armorAdd += GuardiansHeart.heartArmor;
                if (body.healthComponent.shield >= 1f)
                {
                    body.RemoveBuff(LITContent.Buffs.GuardiansHeartBuff.buffIndex);
                }
            }
        }
    }
}