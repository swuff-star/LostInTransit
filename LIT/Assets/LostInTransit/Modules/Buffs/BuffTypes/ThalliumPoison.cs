using Moonstorm;
using Moonstorm.Components;
using R2API;
using RoR2;

namespace LostInTransit.Buffs
{
    public class ThalliumPoison : BuffBase
    {
        public override BuffDef BuffDef { get; } = LITAssets.LoadAsset<BuffDef>("ThalliumPoison");
        public static DotController.DotIndex index;

        public override void Initialize()
        {
            index = DotAPI.RegisterDotDef(1f, 1f, DamageColorIndex.DeathMark, BuffDef);
        }

        public class ThalDebuffBehavior : BaseBuffBodyBehavior, IBodyStatArgModifier
        {
            [BuffDefAssociation(useOnClient = true, useOnServer = true)]
            public static BuffDef GetBuffDef() => LITContent.Buffs.ThalliumPoison;

            public void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args)
            {
                args.moveSpeedReductionMultAdd += Items.Thallium.slowMultiplier;
            }
        }
    }
}