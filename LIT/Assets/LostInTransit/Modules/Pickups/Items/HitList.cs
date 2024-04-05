using MSU;
using RoR2;
using R2API;
using RoR2.Items;

namespace LostInTransit.Items
{
    public class HitList : LITItem
    {
        private const string token = "LIT_ITEM_HITLIST_DESC";
        public override ItemDef ItemDef { get; } = LITAssets.LoadAsset<ItemDef>("HitList", LITBundle.Items);

        [RiskOfOptionsConfigureField(ConfigNameOverride = "Chance to Mark Enemies", ConfigDescOverride = "Base chance for enemies to spawn Marked.")]
        [TokenModifier(token, StatTypes.Default, 0)]
        public static float markChance = 5f;

        [RiskOfOptionsConfigureField(ConfigNameOverride = "Damage Buff Power", ConfigDescOverride = "% increase to base damage provided by buffs from this item.")]
        [TokenModifier(token, StatTypes.Default, 1)]
        public static float buffDmg = 0.5f;

        [RiskOfOptionsConfigureField(ConfigNameOverride = "Damage Buff Duration", ConfigDescOverride = "Duration of the damage buff provided by Hit List.")]
        [TokenModifier(token, StatTypes.Default, 2)]
        public static float buffDur = 20f;


        public class MysteriousVialBehavior : BaseItemBodyBehavior
        {
            [ItemDefAssociation(useOnClient = true, useOnServer = true)]
            public static ItemDef GetItemDef() => LITContent.Items.HitList;
        }
    }
}