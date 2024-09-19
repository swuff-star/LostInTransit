
using MSU;
using MSU.Config;
using RoR2;
using RoR2.ContentManagement;
using RoR2.Items;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LostInTransit.Items
{
    public sealed class PrisonShackles : LITItem, IContentPackModifier
    {
        private const string TOKEN = "LIT_ITEM_PRISONSHACKLES_DESC";

        [RiskOfOptionsConfigureField(LITConfig.ITEMS, configDescOverride = "Multiplier added to the shackled body's movement speed.")]
        [FormatToken(TOKEN, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100)]
        public static float slowMultiplier = 0.3f;

        [RiskOfOptionsConfigureField(LITConfig.ITEMS, configDescOverride = "Base duration of the Shackled debuff.")]
        [FormatToken(TOKEN, 1)]
        public static int duration = 2;

        [RiskOfOptionsConfigureField(LITConfig.ITEMS, configDescOverride = "Extra duration of the Shackled debuff per stack of shackles.")]
        [FormatToken(TOKEN, 2)]
        public static int durationStack = 2;

        public override NullableRef<List<GameObject>> itemDisplayPrefabs => null;
        public override ItemDef itemDef => _itemDef;
        private ItemDef _itemDef;

        private AssetCollection _assetCollection;

        public override void Initialize()
        {
            R2API.RecalculateStatsAPI.GetStatCoefficients += HandleSlow;
        }

        private void HandleSlow(CharacterBody sender, R2API.RecalculateStatsAPI.StatHookEventArgs args)
        {
            if(sender.HasBuff(LITContent.Buffs.bdShackled))
                sender.attackSpeed *= (1 - slowMultiplier);
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public override IEnumerator LoadContentAsync()
        {
            var request = LITAssets.LoadAssetAsync<AssetCollection>("acPrisonShackles", LITBundle.Items);

            request.StartLoad();
            while (!request.IsComplete)
                yield return null;

            _assetCollection = request.Asset;

            _itemDef = _assetCollection.FindAsset<ItemDef>("PrisonShackles");
        }

        public void ModifyContentPack(ContentPack contentPack)
        {
            contentPack.AddContentFromAssetCollection(_assetCollection);
        }

        public class PrisonShacklesBehavior : BaseItemBodyBehavior, IOnDamageDealtServerReceiver
        {
            [ItemDefAssociation(useOnClient = true, useOnServer = true)]
            public static ItemDef GetItemDef() => LITContent.Items.PrisonShackles;
            public void OnDamageDealtServer(DamageReport damageReport)
            {
                if (damageReport.damageInfo.procCoefficient > 0)
                    damageReport.victimBody.AddTimedBuff(LITContent.Buffs.bdShackled, duration + durationStack * (stack - 1));
            }
        }
    }
}
