using LostInTransit.Buffs;
using MSU;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using RoR2.Items;
using R2API;
using RoR2.ContentManagement;
using System.Collections;
using MSU.Config;

namespace LostInTransit.Items
{
    public class GoldenGun : LITItem
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


        public override NullableRef<GameObject> ItemDisplayPrefab => null;
        public override ItemDef ItemDef => _itemDef;
        private ItemDef _itemDef;
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
             * ItemDef - "GoldenGun" - Items
             */
            yield break;
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
