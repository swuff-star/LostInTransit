using MSU;
using RoR2;
using R2API;
using RoR2.Items;

namespace LostInTransit.Items
{
    public class MysteriousVial : LITItem
    {
        private const string token = "LIT_ITEM_MYSTERIOUSVIAL_DESC";
        public override ItemDef ItemDef { get; } = LITAssets.LoadAsset<ItemDef>("MysteriousVial", LITBundle.Items);

        [RiskOfOptionsConfigureField(ConfigNameOverride = "Extra Regen Per Vial", ConfigDescOverride = "Extra Regeneration added per vial.")]
        [TokenModifier(token, StatTypes.Default, 0)]
        public static float vialRegen = 0.8f;


        public class MysteriousVialBehavior : BaseItemBodyBehavior, IBodyStatArgModifier
        {
            [ItemDefAssociation(useOnClient = true, useOnServer = true)]
            public static ItemDef GetItemDef() => LITContent.Items.MysteriousVial;
            public void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args)
            {
                args.baseRegenAdd += (vialRegen + ((vialRegen / 5) * body.level)) * stack;
            }
        }
    }
}
