using Moonstorm;
using RoR2;
using System;
using RoR2.Items;
using UnityEngine;

namespace LostInTransit.Items
{
    public class SmartShopper : ItemBase
    {
        private const string token = "LIT_ITEM_SMARTSHOPPER_DESC";
        public override ItemDef ItemDef { get; } = LITAssets.LoadAsset<ItemDef>("SmartShopper");

        [ConfigurableField(ConfigDesc = "Percentage of money refunded when purchasing something, Percentage (0.5 = 50)")]
        [TokenModifier(token, StatTypes.Percentage, 0)]
        public static float refundAmount = 0.5f;

        public class SmartShopperBehavior : BaseItemBodyBehavior
        {
            [ItemDefAssociation(useOnClient = true, useOnServer = true)]
            public static ItemDef GetItemDef() => LITContent.Items.SmartShopper;
            private float refundAmount;
            private int maxRefunds;
            private int currentRefunds;
            public bool CanRefund { get => currentRefunds < maxRefunds; }

            public void Start()
            {
                refundAmount = Mathf.Clamp01(SmartShopper.refundAmount);
                currentRefunds = 0;
                maxRefunds = stack;

                GlobalEventManager.OnInteractionsGlobal += TryToRefund;
                body.onInventoryChanged += UpdateStacks;
            }
            public void OnDestroy()
            {
                GlobalEventManager.OnInteractionsGlobal -= TryToRefund;
                body.onInventoryChanged -= UpdateStacks;
            }

            private void TryToRefund(Interactor interactor, IInteractable interactable, UnityEngine.GameObject interactableObject)
            {
                var pInteraction = interactableObject.GetComponent<PurchaseInteraction>();
                if(pInteraction)
                {
                    if(pInteraction.costType == CostTypeIndex.Money && CanRefund)
                    {
                        DoRefund((uint)pInteraction.cost);
                    }
                }
            }

            private void DoRefund(uint moneyCost)
            {
                currentRefunds++;
                body.master?.GiveMoney((uint)(moneyCost * refundAmount));
            }


            private void UpdateStacks()
            {
                maxRefunds = body.inventory.GetItemCount(LITAssets.LoadAsset<ItemDef>("SmartShopper"));
            }
        }

        /*public class SmartShopperBehavior : CharacterBody.ItemBehavior, IOnKilledOtherServerReceiver
        {
            public void OnKilledOtherServer(DamageReport damageReport)
            {
                var deathRewards = damageReport.victimBody.GetComponent<DeathRewards>();
                float smartShopperGold;
                if (deathRewards)
                {
                    //Debug.WriteLine("Gold before Smart Shopper: " + deathRewards.goldReward);
                    smartShopperGold = usesExpScaling ? (uint)(deathRewards.goldReward * Math.Pow(goldAmount, 1 / stack)) : (uint)(deathRewards.goldReward * goldAmount * stack);
                    //Debug.WriteLine("And that, times " + 0.25f * stack + "...");
                    //Debug.WriteLine("Comes out to " + smartShopperGold + " extra gold!");

                    body.master.GiveMoney((uint)smartShopperGold);
                }
            }
        }*/
    }
}
