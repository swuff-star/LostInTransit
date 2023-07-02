using Moonstorm;
using RoR2;
using RoR2.Items;
using System.Collections.Generic;
using UnityEngine;

namespace LostInTransit.Items
{
    //N- Items no longer have an "AddBehavior(ref CharacterBody body, int stacks)" method
    public class BeckoningCat : ItemBase
    {
        private const string token = "LIT_ITEM_BECKONINGCAT_DESC";

        public override ItemDef ItemDef => LITAssets.LoadAsset<ItemDef>("BeckoningCat", LITBundle.Items);

        [ConfigurableField(LITConfig.items, ConfigName = "Base Drop Chance", ConfigDesc = "Base chance for Elites to drop an item.")]
        [TokenModifier(token, StatTypes.Default, 0)]
        public static float baseChance = 4.5f;

        [ConfigurableField(LITConfig.items, ConfigName = "Stacking Drop Chance", ConfigDesc = "Added chance for Elites to drop an item per stack.")]
        [TokenModifier(token, StatTypes.Default, 1)]
        public static float stackChance = 1.5f;
        
        [ConfigurableField(LITConfig.items, ConfigName = "Maximum Drop Chance Cap", ConfigDesc = "Maximum possible chance for Elites to drop an item, regardless of stacks.")]
        public static float capChance = 100f;

        [ConfigurableField(LITConfig.items, ConfigName = "Uncommon Item Chance", ConfigDesc = "Chance for Elites to drop an Uncommon (Green) item.")]
        [TokenModifier(token, StatTypes.Default, 2)]
        public static float greenItemChance = 6f;

        [ConfigurableField(LITConfig.items, ConfigName = "Uncommon Item Stacking Chance", ConfigDesc = "Extra chance for Elites to drop an Uncommon (Green) item per stack.")]
        [TokenModifier(token, StatTypes.Default, 3)]
        public static float greenItemStack = 1f;

        [ConfigurableField(LITConfig.items, ConfigName = "Rare Item Chance", ConfigDesc = "Chance for Elites to drop a Rare (Red) item.")]
        [TokenModifier(token, StatTypes.Default, 4)]
        public static float redItemChance = 0.5f;

        [ConfigurableField(LITConfig.items, ConfigName = "Rare Item Stacking Chance", ConfigDesc = "Extra chance for Elites to drop a Rare (Red) item per stack.")]
        [TokenModifier(token, StatTypes.Default, 5)]
        public static float redItemStack = 0.25f;

        [ConfigurableField(LITConfig.items, ConfigName = "Use Luck", ConfigDesc = "Whether Luck should be accounted for in all Beckoning Cat-related rolls.")]
        public static bool usesLuck = true;

        //N- Item Behaviors now use RoR2's "BaseItemBodyBehavior" class
        //The class itself is abstract and its in RoR2.Items
        public class BeckoningCatBehavior : BaseItemBodyBehavior, IOnKilledOtherServerReceiver
        {
            //N- This attribute is needed for the system to automatically take care of adding the behavior or not depending on the item's stacks.
            //it NEEDS to be static, return an itemDef, and have this attribute.
            //If you're not sure whether the behavior needs to be on client, on server, or both, just set "useOnClient" and "useOnServer" to true.

            //SIDENOTE: Use on client is false, which means that client's game objects wont have this behavior, but this is ok, as IOnKilledOtherServerReceiver runs only on server.
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
                    if (victimBody.isElite && Roll(Mathf.Min(baseChance + (stackChance * (stack - 1)), capChance), 0))
                    {
                        //Debug.Log("Rolling for item...");
                        var redItem = usesLuck ? Roll(redItemChance + (redItemStack * (stack - 1)), body.master.luck) : Roll(redItemChance + (redItemStack * (stack - 1)), 0);
                        //Debug.Log($"red Item? {redItem}");
                        if (redItem)
                        {

                            SpawnItem(redItems, nextRedItem);
                            return;
                        }
                        var greenItem = usesLuck ? Roll(greenItemChance + (greenItemStack * (stack - 1)), body.master.luck) : Roll(greenItemChance + (greenItemStack * (stack - 1)), 0);
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
