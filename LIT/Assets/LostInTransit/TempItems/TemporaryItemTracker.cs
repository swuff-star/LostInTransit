using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using RoR2;
using UnityEngine.Networking;
using RoR2.UI;
using UnityEngine.UI;

namespace LostInTransit.Components
{
    public class TemporaryItemTracker : MonoBehaviour
    {
        
        public static Vector3 timerLocalPos = new Vector3(20f, -50f, 0f); // XDdd
        public static Vector3 timerLocalScale = new Vector3(-0.25f, 0.25f, 0.25f); // rotating didnt work on the prefab idk
        public static GameObject timerPrefab = LITAssets.LoadAsset<GameObject>("TemporaryItemTimer", LITBundle.Base);
        public static void Init()
        {
            On.RoR2.UI.ItemIcon.SetItemIndex += AddTemporaryUI;
        }


        // turns out this is a bit tricker since item icons arent bound to specific items or inventories. they just change their item index
        // might be performance issues here? since SetItemIndex called for every item icon at once. probably not tho
        // networking is definitely fucking stupid and probably doesnt work since TemporaryItemTracker isnt a networkbehavior
        private static void AddTemporaryUI(On.RoR2.UI.ItemIcon.orig_SetItemIndex orig, ItemIcon self, ItemIndex newItemIndex, int newItemCount)
        {
            orig(self, newItemIndex, newItemCount);

            if (!NetworkServer.active) return;

            // need to check if the icon already has a timer. otherwise a new one will be instantiated every time the index changes. 
            // or the timer wont disappear if its not a temporary item
            Transform timer = self.transform.Find("TemporaryItemTimer(Clone)");// xDDDDDD

            if (LITTempItems.realToTemporaryIndex.ContainsValue(newItemIndex))
            {
                ItemInventoryDisplay display = self.GetComponentInParent<ItemInventoryDisplay>();

                if (display) ////////
                {
                    TemporaryItemHudElement itemTimer;
                    
                    if(!timer)
                    {
                        itemTimer = GameObject.Instantiate(timerPrefab, self.transform).GetComponent<TemporaryItemHudElement>();
                    }
                    else
                    {
                        itemTimer = timer.GetComponent<TemporaryItemHudElement>();
                    }

                    itemTimer.transform.localPosition = timerLocalPos; // lol
                    itemTimer.transform.localScale = timerLocalScale;

                    TemporaryItemTracker tracker = display.inventory.GetComponent<TemporaryItemTracker>();
                    TemporaryItem temporaryItem = tracker.GetTemporaryItem(newItemIndex);
                    itemTimer.duration = temporaryItem.duration;
                    itemTimer.timeLeft = temporaryItem.timeRemaining;
                    NetworkServer.Spawn(itemTimer.gameObject);
                }
            }
            else
            {
                if (timer) Destroy(timer.gameObject);
            }
        }

        public class TemporaryItem
        {
            public ItemIndex itemIndex;
            public float timeRemaining;
            public float duration;
            public int stack;
        }

        public List<TemporaryItem> temporaryItems = new List<TemporaryItem>();

        private Inventory inventory;

        private void Awake()
        {
            this.inventory = base.GetComponent<Inventory>();
        }

        public TemporaryItem GetTemporaryItem(ItemIndex itemIndex)
        {
            foreach (TemporaryItem item in temporaryItems)
            {
                if (item.itemIndex == itemIndex)
                {
                    return item;
                }
            }
            return null;
        }

        public void AddTemporaryItemTimer(ItemIndex index, float duration, int stack) // stack refreshing goes here if needed
        {
            this.temporaryItems.Add(new TemporaryItem { itemIndex = index, timeRemaining = duration, duration = duration, stack = stack });
        }

        public void OnTemporaryItemExpired(TemporaryItem item)
        {
            if (this.inventory)
            {
                this.inventory.RemoveItem(item.itemIndex, item.stack);
            }
        }
        private void FixedUpdate()
        {
            for (int i = temporaryItems.Count - 1; i >= 0; i--)
            {
                temporaryItems[i].timeRemaining -= Time.fixedDeltaTime;

                if (temporaryItems[i].timeRemaining <= 0)
                {
                    OnTemporaryItemExpired(temporaryItems[i]);
                    temporaryItems.RemoveAt(i);                
                }
                else if (this.inventory.GetItemCount(temporaryItems[i].itemIndex) == 0)
                {
                    temporaryItems.RemoveAt(i);
                }

            }
        }
    }

    
}
