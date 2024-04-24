
using MSU;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using RoR2.Items;
using R2API;
using RoR2.ContentManagement;
using System.Collections;
using MSU.Config;
using System.Collections.Generic;

namespace LostInTransit.Items
{
    public class GoldenGun : LITItem, IContentPackModifier
    {
        private const string TOKEN = "LIT_ITEM_GOLDENGUN_DESC";

        [RiskOfOptionsConfigureField(LITConfig.ITEMS, ConfigDescOverride = "The maximum amount of bonus damage Golden Gun grants.")]
        [FormatToken(TOKEN)]
        [FormatToken(TOKEN, FormatTokenAttribute.OperationTypeEnum.DivideByN, 1, 2)]
        public static uint maxDamageBonus = 40;

        [RiskOfOptionsConfigureField(LITConfig.ITEMS, ConfigDescOverride = "The maximum amount of gold that Golden Gun will account for.")]
        [FormatToken(TOKEN, 2)]
        [FormatToken(TOKEN, FormatTokenAttribute.OperationTypeEnum.DivideByN, 3, 2)]
        public static uint maxGoldThreshold = 300;


        public override NullableRef<List<GameObject>> ItemDisplayPrefabs => null;
        public override ItemDef ItemDef => _itemDef;
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
            var request = LITAssets.LoadAssetAsync<AssetCollection>("acGoldenGun", LITBundle.Items);

            request.StartLoad();
            while (!request.IsComplete)
                yield return null;

            _assetCollection = request.Asset;

            _itemDef = _assetCollection.FindAsset<ItemDef>("GoldenGun");
        }

        public void ModifyContentPack(ContentPack contentPack)
        {
            contentPack.AddContentFromAssetCollection(_assetCollection);
        }

        public class GoldenGunBehavior : BaseItemBodyBehavior, IBodyStatArgModifier
        {
            [ItemDefAssociation(useOnClient = true, useOnServer = true)]
            public static ItemDef GetItemDef() => LITContent.Items.GoldenGun;

            public void OnDestroy()
            {
                body.SetBuffCount(LITContent.Buffs.bdGoldenGun.buffIndex, 0);
            }

            public void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args)
            {
                args.damageMultAdd += 0.01f * body.GetBuffCount(LITContent.Buffs.bdGoldenGun.buffIndex);
            }

            private void FixedUpdate()
            {
                if (NetworkServer.active)
                {
                    int singleStackCost = Stage.instance ? Run.instance.GetDifficultyScaledCost((int)maxGoldThreshold, Stage.instance.entryDifficultyCoefficient) : Run.instance.GetDifficultyScaledCost((int)maxGoldThreshold);

                    int maxCost = singleStackCost + ((int)(0.5f * maxGoldThreshold) * stack - 1);
                    int maxBuffs = (int)maxDamageBonus + ((int)(0.5f * maxDamageBonus) * stack - 1);

                    float moneyPercent = (float)body.master.money / maxCost;
                    int targetBuffCount = Mathf.Min(maxBuffs, Mathf.FloorToInt(maxBuffs * moneyPercent));

                    int currentBuffCount = body.GetBuffCount(LITContent.Buffs.bdGoldenGun.buffIndex);
                    if (targetBuffCount != currentBuffCount)
                    {
                        body.SetBuffCount(LITContent.Buffs.bdGoldenGun.buffIndex, targetBuffCount);
                    }
                }
            }
        }
    }
}
