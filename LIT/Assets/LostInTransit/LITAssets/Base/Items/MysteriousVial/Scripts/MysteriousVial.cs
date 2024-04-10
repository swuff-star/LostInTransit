using MSU;
using RoR2;
using R2API;
using RoR2.Items;
using MSU.Config;
using UnityEngine;
using RoR2.ContentManagement;
using System.Collections;

namespace LostInTransit.Items
{
    public sealed class MysteriousVial : LITItem
    {
        private const string TOKEN = "LIT_ITEM_MYSTERIOUSVIAL_DESC";

        [RiskOfOptionsConfigureField(LITConfig.ITEMS, ConfigDescOverride = "Extra Regeneration added per vial.")]
        [FormatToken(TOKEN)]
        public static float regenBonus = 0.8f;

        public override NullableRef<GameObject> ItemDisplayPrefab => null;
        public override ItemDef ItemDef => _itemDef;
        private ItemDef _itemDef;

        public override void Initialize()
        {
            RecalculateStatsAPI.GetStatCoefficients += AddRegen;
        }

        private void AddRegen(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if(sender.TryGetItemCount(_itemDef, out int count))
            {
                args.baseRegenAdd += (regenBonus + ((regenBonus / 5) * sender.level)) * count;
            }
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public override IEnumerator LoadContentAsync()
        {
            var request = LITAssets.LoadAssetAsync<ItemDef>("MysteriousVial", LITBundle.Items);

            request.StartLoad();

            while (!request.IsComplete)
                yield return null;

            _itemDef = request.Asset;
        }
    }
}
