using MSU;
using RoR2;
using RoR2.Items;
using UnityEngine;
using R2API;
using System;
using RoR2.ContentManagement;
using System.Collections;
using MSU.Config;
using System.Collections.Generic;

namespace LostInTransit.Items
{
    public sealed class BitterRoot : LITItem, IContentPackModifier
    {
        public const string TOKEN = "LIT_ITEM_BITTERROOT_DESC";

        [RiskOfOptionsConfigureField(LITConfig.ITEMS, configDescOverride = "Amount of regen on kill per Root.")]
        [FormatToken(TOKEN, 0)]
        public static float regenAmount = 3f;

        [RiskOfOptionsConfigureField(LITConfig.ITEMS, configDescOverride = "Duration of regen on kill per Root.")]
        [FormatToken(TOKEN, 1)]
        public static float regenDuration = 3f;

        public override NullableRef<List<GameObject>> itemDisplayPrefabs => null;

        public override ItemDef itemDef => _itemDef;
        private ItemDef _itemDef;
        private AssetCollection _assetCollection;

        public override void Initialize()
        {
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public override IEnumerator LoadContentAsync()
        {
            /*
             * ItemDef - "BitterRoot" - Items
             */

            var assetRequest = LITAssets.LoadAssetAsync<AssetCollection>("acBitterRoot", LITBundle.Items);

            assetRequest.StartLoad();
            while (!assetRequest.IsComplete)
                yield return null;

            _assetCollection = assetRequest.Asset;
            _itemDef = _assetCollection.FindAsset<ItemDef>("BitterRoot");
        }

        public void ModifyContentPack(ContentPack contentPack)
        {
            contentPack.AddContentFromAssetCollection(_assetCollection);
        }

        public class BitterRootBehavior : BaseItemBodyBehavior, IOnKilledOtherServerReceiver, IBodyStatArgModifier
        {
            [ItemDefAssociation(useOnClient = true, useOnServer = true, behaviorTypeOverride = typeof(BitterRootBehavior))]
            public static ItemDef GetItemDef() => LITContent.Items.BitterRoot;

            public void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args)
            {
                args.baseRegenAdd += (Items.BitterRoot.regenAmount + ((Items.BitterRoot.regenAmount / 5) * body.level)) * body.GetBuffCount(LITContent.Buffs.bdRootRegen);
            }

            public void OnKilledOtherServer(DamageReport damageReport)
            {
                body.AddTimedBuffAuthority(LITContent.Buffs.bdRootRegen.buffIndex, regenDuration * stack);
            }
        }
    }
}
