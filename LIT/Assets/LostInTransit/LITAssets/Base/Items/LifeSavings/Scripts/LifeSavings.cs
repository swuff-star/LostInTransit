using MSU;
using RoR2;
using UnityEngine;
using RoR2.Items;
using RoR2.ContentManagement;
using System.Collections;
using MSU.Config;

namespace LostInTransit.Items
{
    public sealed class LifeSavings : LITItem, IContentPackModifier
    {

        private const string TOKEN = "LIT_ITEM_LIFESAVINGS_DESC";

        [RiskOfOptionsConfigureField(LITConfig.ITEMS, ConfigDescOverride = "Money granted per Life Savings.")]
        [FormatToken(TOKEN)]
        public static int moneyPerPig = 75;

        public override NullableRef<GameObject> ItemDisplayPrefab => null;
        public override ItemDef ItemDef => _itemDef;
        private ItemDef _itemDef;

        //rip piggo -N
        private ItemDef _brokenPiggy;
        public override void Initialize()
        {
            CharacterBody.onBodyStartGlobal += GiveMoney;
        }

        private void GiveMoney(CharacterBody obj)
        {
            if (!obj.inventory)
                return;
            var inv = obj.inventory;
            int count = inv.GetItemCount(ItemDef);
            if (count >= 1)
            {
                obj.master.GiveMoney((uint)Run.instance.GetDifficultyScaledCost(moneyPerPig));
                inv.RemoveItem(ItemDef);
                inv.GiveItem(_brokenPiggy);
                CharacterMasterNotificationQueue.SendTransformNotification(obj.master, _itemDef.itemIndex, _brokenPiggy.itemIndex, CharacterMasterNotificationQueue.TransformationType.Default);
            }
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public override IEnumerator LoadContentAsync()
        {
            var request = LITAssets.LoadAssetAsync<AssetCollection>("acLifeSavings", LITBundle.Items);

            request.StartLoad();
            while (!request.IsComplete)
                yield return null;

            var collection = request.Asset;

            _itemDef = collection.FindAsset<ItemDef>("LifeSavings");
            _brokenPiggy = collection.FindAsset<ItemDef>("LifeSavingsUsed");
        }

        public void ModifyContentPack(ContentPack contentPack)
        {
            contentPack.itemDefs.AddSingle(_brokenPiggy);
        }
    }
}
