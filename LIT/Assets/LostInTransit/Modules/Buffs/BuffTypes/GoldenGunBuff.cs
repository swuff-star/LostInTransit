using Moonstorm;
using RoR2;
using R2API;
using Moonstorm.Components;

namespace LostInTransit.Buffs
{
    public class GoldenGunBuff : BuffBase
    {
        public override BuffDef BuffDef { get; } = LITAssets.LoadAsset<BuffDef>("GoldenGun");

        public class GoldenGunBuffBehavior : BaseBuffBodyBehavior, IBodyStatArgModifier
        {
            [BuffDefAssociation(useOnClient = true, useOnServer = true)]
            public static BuffDef GetBuffDef() => LITContent.Buffs.GoldenGun;

            public void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args)
            {
                //args.damageMultAdd += 0.01f * buffStacks;
            }
        }
    }
}