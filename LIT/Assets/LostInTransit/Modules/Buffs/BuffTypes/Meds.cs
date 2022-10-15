using Moonstorm;
using RoR2;
using R2API;
using RoR2.Items;
using Moonstorm.Components;

namespace LostInTransit.Buffs
{
    //[DisabledContent]
    public class Meds : BuffBase
    {
        public override BuffDef BuffDef { get; } = LITAssets.LoadAsset<BuffDef>("Meds");

        public class DiceAtkBehavior : BaseBuffBodyBehavior, IBodyStatArgModifier
        {
            [BuffDefAssociation(useOnServer = true, useOnClient = true)]
            public static BuffDef GetBuffDef() => LITContent.Buffs.Meds;

            public void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args)
            {
                args.attackSpeedMultAdd += 0.7f;
                args.damageMultAdd += 0.2f;
                args.armorAdd += 20f;
            }

            public void OnDestroy()
            {
                body.RecalculateStats();
            }
        }
    }
}