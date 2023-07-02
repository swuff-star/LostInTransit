using LostInTransit.Buffs;
using Moonstorm;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using RoR2.Items;
using R2API;

namespace LostInTransit.Items
{
    public class GoldenGun : ItemBase
    {
        private const string token = "LIT_ITEM_GOLDENGUN_DESC";
        public override ItemDef ItemDef { get; } = LITAssets.LoadAsset<ItemDef>("GoldenGun", LITBundle.Items);

        [ConfigurableField(ConfigName = "Maximum Gold Threshold", ConfigDesc = "The maximum amount of gold that Golden Gun will account for.")]
        [TokenModifier(token, StatTypes.Default, 2)]
        [TokenModifier(token, StatTypes.DivideBy2, 3)]
        public static uint goldCap = 300;

        [ConfigurableField(ConfigName = "Maximum Damage Bonus", ConfigDesc = "The maximum amount of bonus damage Golden Gun grants.")]
        [TokenModifier(token, StatTypes.Default, 0)]
        [TokenModifier(token, StatTypes.DivideBy2, 1)]
        public static uint goldNeeded = 40;


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
                    int singleStackCost = Stage.instance ? Run.instance.GetDifficultyScaledCost((int)goldCap, Stage.instance.entryDifficultyCoefficient) : Run.instance.GetDifficultyScaledCost((int)goldCap);

                    int maxCost = singleStackCost + ((int)(0.5f * goldCap) * stack - 1);
                    int maxBuffs = (int)goldNeeded + ((int)(0.5f * goldNeeded) * stack - 1);

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
