using LostInTransit.Buffs;
using Moonstorm;
using RoR2;
using RoR2.Items;

namespace LostInTransit.Items
{
    public class PrisonShackles : ItemBase
    {
        private const string token = "LIT_ITEM_PRISONSHACKLES_DESC";
        public override ItemDef ItemDef { get; } = LITAssets.LoadAsset<ItemDef>("PrisonShackles");

        public static string section;
        [ConfigurableField(ConfigName = "Slow Multiplier", ConfigDesc = "Multiplier added to the shackled body's movement speed.")]
        [TokenModifier(token, StatTypes.Percentage, 0)]
        public static float slowMultiplier = 0.3f;

        [ConfigurableField(ConfigName = "Duration", ConfigDesc = "Base duration of the Shackled debuff.")]
        [TokenModifier(token, StatTypes.Default, 1)]
        public static int duration = 2;

        [ConfigurableField(ConfigName = "Stacking Duration", ConfigDesc = "Extra duration of the Shackled debuff per stack of shackles.")]
        [TokenModifier(token, StatTypes.Default, 2)]
        public static int durationStack = 2;

        public class PrisonShacklesBehavior : BaseItemBodyBehavior, IOnDamageDealtServerReceiver
        {
            [ItemDefAssociation(useOnClient = true, useOnServer = true)]
            public static ItemDef GetItemDef() => LITContent.Items.PrisonShackles;
            public void OnDamageDealtServer(DamageReport damageReport)
            {
                if(damageReport.damageInfo.procCoefficient > 0)
                    damageReport.victimBody.AddTimedBuff(LITContent.Buffs.Shackled, duration + durationStack * (stack - 1));
            }
        }
    }
}
