using MSU;
using RoR2;
using System;
using RoR2.Items;
using UnityEngine;
using RoR2.ContentManagement;
using System.Collections;
using MSU.Config;

namespace LostInTransit.Items
{
#if DEBUG
    public sealed class SmartShopper : LITItem
    {
        private const string TOKEN = "LIT_ITEM_SMARTSHOPPER_DESC";

        [RiskOfOptionsConfigureField(LITConfig.ITEMS, ConfigDescOverride = "Percentage of money refunded when purchasing something, Percentage (0.5 = 50)")]
        [FormatToken(TOKEN, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100)]
        public static float refundAmount = 0.5f;

        public override NullableRef<GameObject> ItemDisplayPrefab => null;
        public override ItemDef ItemDef => _itemDef;
        private static ItemDef _itemDef;
        public override void Initialize()
        {
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return false;
        }

        public override IEnumerator LoadContentAsync()
        {
            /*
             * ItemDef - "SmartShopper" - Items
             */
            yield break;
        }

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
                if (pInteraction)
                {
                    if (pInteraction.costType == CostTypeIndex.Money && CanRefund)
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
                maxRefunds = body.inventory.GetItemCount(LITContent.Items.SmartShopper);
            }
        }
    }
#endif
}
