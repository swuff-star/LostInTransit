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
    //N- Items no longer have an "AddBehavior(ref CharacterBody body, int stacks)" method

    public sealed class BeckoningCat : LITItem
    {
        private const string TOKEN = "LIT_ITEM_BECKONINGCAT_DESC";

        [RiskOfOptionsConfigureField(LITConfig.ITEMS, configDescOverride = "Base chance for Elites to drop an item.")]
        [FormatToken(TOKEN)]
        public static float baseDropChance = 4.5f;

        [RiskOfOptionsConfigureField(LITConfig.ITEMS, configDescOverride = "Added chance for Elites to drop an item per stack.")]
        [FormatToken(TOKEN,1)]
        public static float stackingDropChance = 1.5f;

        [RiskOfOptionsConfigureField(LITConfig.ITEMS, configDescOverride = "Maximum possible chance for Elites to drop an item, regardless of stacks.")]
        public static float maximumDropChance = 100f;

        [RiskOfOptionsConfigureField(LITConfig.ITEMS, configNameOverride = "Uncommon Item Chance", configDescOverride = "Chance for Elites to drop an Uncommon (Green) item.")]
        [FormatToken(TOKEN, 2)]
        public static float greenItemChance = 6f;

        [RiskOfOptionsConfigureField(LITConfig.ITEMS, configNameOverride = "Uncommon Item Stacking Chance", configDescOverride = "Extra chance for Elites to drop an Uncommon (Green) item per stack.")]
        [FormatToken(TOKEN, 3)]
        public static float greenItemStack = 1f;

        [RiskOfOptionsConfigureField(LITConfig.ITEMS, configNameOverride = "Rare Item Chance", configDescOverride = "Chance for Elites to drop a Rare (Red) item.")]
        [FormatToken(TOKEN, 4)]
        public static float redItemChance = 0.5f;

        [RiskOfOptionsConfigureField(LITConfig.ITEMS, configNameOverride = "Rare Item Stacking Chance", configDescOverride = "Extra chance for Elites to drop a Rare (Red) item per stack.")]
        [FormatToken(TOKEN, 5)]
        public static float redItemStack = 0.25f;

        [RiskOfOptionsConfigureField(LITConfig.ITEMS, configDescOverride = "Whether Luck should be accounted for in all Beckoning Cat-related rolls.")]
        public static bool useLuck = true;

        public override NullableRef<List<GameObject>> itemDisplayPrefabs => null;
        public override ItemDef itemDef => _itemDef;
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
            var assetRequest = LITAssets.LoadAssetAsync<ItemDef>("BeckoningCat", LITBundle.Items);

            assetRequest.StartLoad();
            while (!assetRequest.IsComplete)
                yield return null;

            _itemDef = assetRequest.Asset;
            yield break;
        }

        public class BeckoningCatBehavior : BaseItemBodyBehavior, IOnKilledOtherServerReceiver
        {
            [ItemDefAssociation(useOnClient = false, useOnServer = true)]
            public static ItemDef GetItemDef() => LITContent.Items.BeckoningCat;

            public List<PickupIndex> redItems = Run.instance.availableTier3DropList;
            public List<PickupIndex> greenItems = Run.instance.availableTier2DropList;
            public List<PickupIndex> whiteItems = Run.instance.availableTier1DropList;

            private int nextRedItem;
            private int nextGreenItem;
            private int nextWhiteItem;

            private Vector3 constant = (Vector3.up * 20f) + (5 * Vector3.right * Mathf.Cos(2f * Mathf.PI / Run.instance.participatingPlayerCount)) + (5 * Vector3.forward * Mathf.Sin(2f * Mathf.PI / Run.instance.participatingPlayerCount));

            //Swuff's original code hurts me so i'm re-using the one from varianceAPI.
            //★ at least my code dropped items more than 0.9% of the time :smirk:
            //Yeah thats fair.
            //★ ily
            //Funnily enough this didn't work like in ror1 where it used every elite modifier, now it does, chad.
            public void OnKilledOtherServer(DamageReport damageReport)
            {
                RefreshNextItems();
                var victimBody = damageReport.victimBody;
                var dropLocation = damageReport.attackerBody.transform.position;
                for (int i = 0; i < victimBody.eliteBuffCount; i++)
                {
                    if (victimBody.isElite && Roll(Mathf.Min(baseDropChance + (stackingDropChance * (stack - 1)), maximumDropChance), 0))
                    {
                        //Debug.Log("Rolling for item...");
                        var redItem = useLuck ? Roll(redItemChance + (redItemStack * (stack - 1)), body.master.luck) : Roll(redItemChance + (redItemStack * (stack - 1)), 0);
                        //Debug.Log($"red Item? {redItem}");
                        if (redItem)
                        {

                            SpawnItem(redItems, nextRedItem);
                            return;
                        }
                        var greenItem = useLuck ? Roll(greenItemChance + (greenItemStack * (stack - 1)), body.master.luck) : Roll(greenItemChance + (greenItemStack * (stack - 1)), 0);
                        //Debug.Log($"green Item? {greenItem}");
                        if (greenItem)
                        {
                            SpawnItem(greenItems, nextGreenItem);
                            return;
                        }
                        else
                        {
                            SpawnItem(whiteItems, nextWhiteItem);
                            return;
                        }
                    }

                }
                void SpawnItem(List<PickupIndex> items, int nextItem)
                {
                    PickupDropletController.CreatePickupDroplet(items[nextItem], victimBody.transform.position, constant);
                    Util.PlaySound("CatProc", body.gameObject);
                }
            }
            private void RefreshNextItems()
            {
                nextWhiteItem = Run.instance.treasureRng.RangeInt(0, whiteItems.Count);
                nextGreenItem = Run.instance.treasureRng.RangeInt(0, greenItems.Count);
                nextRedItem = Run.instance.treasureRng.RangeInt(0, redItems.Count);
            }


            private bool Roll(float chance, float usesLuck)
            {
                return Util.CheckRoll(chance, usesLuck);
            }
        }
    }
}
