using LostInTransit.Buffs;
using MSU;
using RoR2;
using RoR2.Items;

namespace LostInTransit.Items
{
    public class PrisonShackles : LITItem
    {
        private const string token = "LIT_ITEM_PRISONSHACKLES_DESC";
        public override ItemDef ItemDef { get; } = LITAssets.LoadAsset<ItemDef>("PrisonShackles", LITBundle.Items);

        public static string section;
        [RiskOfOptionsConfigureField(ConfigNameOverride = "Slow Multiplier", ConfigDescOverride = "Multiplier added to the shackled body's movement speed.")]
        [TokenModifier(token, StatTypes.MultiplyByN, 0, 100)]
        public static float slowMultiplier = 0.3f;

        [RiskOfOptionsConfigureField(ConfigNameOverride = "Duration", ConfigDescOverride = "Base duration of the Shackled debuff.")]
        [TokenModifier(token, StatTypes.Default, 1)]
        public static int duration = 2;

        [RiskOfOptionsConfigureField(ConfigNameOverride = "Stacking Duration", ConfigDescOverride = "Extra duration of the Shackled debuff per stack of shackles.")]
        [TokenModifier(token, StatTypes.Default, 2)]
        public static int durationStack = 2;

        public class PrisonShacklesBehavior : BaseItemBodyBehavior, IOnDamageDealtServerReceiver
        {
            [ItemDefAssociation(useOnClient = true, useOnServer = true)]
            public static ItemDef GetItemDef() => LITContent.Items.PrisonShackles;
            public void OnDamageDealtServer(DamageReport damageReport)
            {
                if(damageReport.damageInfo.procCoefficient > 0)
                    damageReport.victimBody.AddTimedBuff(LITContent.Buffs.bdShackled, duration + durationStack * (stack - 1));
            }
        }
    }
}
