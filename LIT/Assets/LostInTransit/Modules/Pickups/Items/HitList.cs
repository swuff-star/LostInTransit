using Moonstorm;
using RoR2;
using R2API;
using RoR2.Items;

namespace LostInTransit.Items
{
    public class HitList : ItemBase
    {
        private const string token = "LIT_ITEM_HITLIST_DESC";
        public override ItemDef ItemDef { get; } = LITAssets.LoadAsset<ItemDef>("HitList");

        [ConfigurableField(ConfigName = "Chance to Mark Enemies", ConfigDesc = "Base chance for enemies to spawn Marked.")]
        [TokenModifier(token, StatTypes.Default, 0)]
        public static float markChance = 5f;

        [ConfigurableField(ConfigName = "Damage Buff Power", ConfigDesc = "% increase to base damage provided by buffs from this item.")]
        [TokenModifier(token, StatTypes.Default, 1)]
        public static float buffDmg = 0.5f;

        [ConfigurableField(ConfigName = "Damage Buff Duration", ConfigDesc = "Duration of the damage buff provided by Hit List.")]
        [TokenModifier(token, StatTypes.Default, 2)]
        public static float buffDur = 20f;


        public class MysteriousVialBehavior : BaseItemBodyBehavior
        {
            [ItemDefAssociation(useOnClient = true, useOnServer = true)]
            public static ItemDef GetItemDef() => LITContent.Items.HitList;
        }
    }
}