using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using R2API;
using HG;
using LostInTransit.Components;
namespace LostInTransit
{
    public class LITTempItems
    {
        public static ItemTierDef tempTier1;
        public static ItemTierDef tempTier2;
        public static ItemTierDef tempTier3;
        public static ItemTierDef tempLunar;

        public static ItemTag temporaryItemTag; // idk what we'll use a tag for but why not

        public static Dictionary<ItemDef, ItemDef> realToTemporary = new Dictionary<ItemDef, ItemDef>();
        public static Dictionary<ItemIndex, ItemIndex> realToTemporaryIndex = new Dictionary<ItemIndex, ItemIndex>();

        public static float fallbackTemporaryItemDuration = 80f; // ideally this isnt ever used
        public static GameObject temporaryItemPickupPrefab;
        public static ItemIndex[] blacklist;
        public void Init()
        {
            ItemTierDef t1 = Addressables.LoadAssetAsync<ItemTierDef>("RoR2/Base/Common/Tier1Def.asset").WaitForCompletion();
            ItemTierDef t2 = Addressables.LoadAssetAsync<ItemTierDef>("RoR2/Base/Common/Tier2Def.asset").WaitForCompletion();
            ItemTierDef t3 = Addressables.LoadAssetAsync<ItemTierDef>("RoR2/Base/Common/Tier3Def.asset").WaitForCompletion();
            ItemTierDef tL = Addressables.LoadAssetAsync<ItemTierDef>("RoR2/Base/Common/LunarTierDef.asset").WaitForCompletion();

            GameObject pickup = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/GenericPickup.prefab").WaitForCompletion();

            // blue glow needs to be on temporaryItemPickupPrefab
            temporaryItemPickupPrefab = PrefabAPI.InstantiateClone(pickup, "TemporaryItemPickup"); // might have to do r2api/contentpack thing i dont remember
            temporaryItemPickupPrefab.AddComponent<TemporaryItemPickupComponent>();
            ContentAddition.AddNetworkedObject(temporaryItemPickupPrefab); /////////////////////////

            TemporaryItemTracker.Init();

            On.RoR2.ItemCatalog.SetItemDefs += ItemCatalog_SetItemDefs;

            On.RoR2.Inventory.GetItemCount_ItemIndex += (orig, inv, item) =>
            {
                if (realToTemporaryIndex.TryGetValue(item, out var tempItem))
                    return orig(inv, item) + orig(inv, tempItem);
                return orig(inv, item);
            };


            // would be good to get item timers into these somehow
            // also mithrix duplicates the temporary items when he gives them back. not fixed yet
            //On.RoR2.Orbs.ItemTransferOrb.DefaultOnArrivalBehavior += (orig, orb) =>
            //{

            //}

            tempTier1 = ScriptableObject.CreateInstance<ItemTierDef>();
            tempTier1.name = "TemporaryTier1";
            tempTier1.tier = ItemTier.AssignedAtRuntime;
            tempTier1.bgIconTexture = t1.bgIconTexture;
            tempTier1.colorIndex = t1.colorIndex;
            tempTier1.darkColorIndex = t1.darkColorIndex;
            tempTier1.highlightPrefab = t1.highlightPrefab;
            tempTier1.dropletDisplayPrefab = t1.dropletDisplayPrefab;
            tempTier1.isDroppable = false;
            tempTier1.canScrap = false;
            tempTier1.canRestack = false;
            tempTier1.pickupRules = ItemTierDef.PickupRules.Default;

            ContentAddition.AddItemTierDef(tempTier1);

            tempTier2 = ScriptableObject.CreateInstance<ItemTierDef>();
            tempTier2.name = "TemporaryTier2";
            tempTier2.tier = ItemTier.AssignedAtRuntime;
            tempTier2.bgIconTexture = t2.bgIconTexture;
            tempTier2.colorIndex = t2.colorIndex;
            tempTier2.darkColorIndex = t2.darkColorIndex;
            tempTier2.highlightPrefab = t2.highlightPrefab;
            tempTier2.dropletDisplayPrefab = t2.dropletDisplayPrefab;
            tempTier2.isDroppable = false;
            tempTier2.canScrap = false;
            tempTier2.canRestack = false;
            tempTier2.pickupRules = ItemTierDef.PickupRules.Default;

            ContentAddition.AddItemTierDef(tempTier2);

            tempTier3 = ScriptableObject.CreateInstance<ItemTierDef>();
            tempTier3.name = "TemporaryTier3";
            tempTier3.tier = ItemTier.AssignedAtRuntime;
            tempTier3.bgIconTexture = t3.bgIconTexture;
            tempTier3.colorIndex = t3.colorIndex;
            tempTier3.darkColorIndex = t3.darkColorIndex;
            tempTier3.highlightPrefab = t3.highlightPrefab;
            tempTier3.dropletDisplayPrefab = t3.dropletDisplayPrefab;
            tempTier3.isDroppable = false;
            tempTier3.canScrap = false;
            tempTier3.canRestack = false;
            tempTier3.pickupRules = ItemTierDef.PickupRules.Default;

            ContentAddition.AddItemTierDef(tempTier3);

            tempLunar = ScriptableObject.CreateInstance<ItemTierDef>();
            tempLunar.name = "TemporaryLunar";
            tempLunar.tier = ItemTier.AssignedAtRuntime;
            tempLunar.bgIconTexture = tL.bgIconTexture;
            tempLunar.colorIndex = tL.colorIndex;
            tempLunar.darkColorIndex = tL.darkColorIndex;
            tempLunar.highlightPrefab = tL.highlightPrefab;
            tempLunar.dropletDisplayPrefab = tL.dropletDisplayPrefab;
            tempLunar.isDroppable = false;
            tempLunar.canScrap = false;
            tempLunar.canRestack = false;
            tempLunar.pickupRules = ItemTierDef.PickupRules.ConfirmAll;

            ContentAddition.AddItemTierDef(tempLunar);
        }

        [SystemInitializer(typeof(ItemCatalog))]
        private static void CreateBlacklist() // config
        {
            blacklist = new ItemIndex[] {
            DLC1Content.Items.RegeneratingScrap.itemIndex, // usable on green printers, but doesnt remove the it from inventory. and the "consumed" scrap isnt temporary (aka infinite items). seems annoying to fix 
            DLC1Content.Items.LunarSun.itemIndex, // egocentrism turns items into non-temporary copies. would have to ILHook LunarSunBehavior. probably worth doing because the interaction would be funny
            };
        }
        public static ItemIndex CheckForTemporaryReplacement(ItemIndex index)
        {           
            if (Array.IndexOf<ItemIndex>(blacklist, index) != -1) return ItemIndex.None;

            return realToTemporaryIndex.TryGetValue(index, out ItemIndex temporaryItem) ? temporaryItem : ItemIndex.None;
        }


        // silly way of passing a temporary item's duration into a pickup
        public static void CreateTemporaryItemDroplet(PickupIndex pickupIndex, Vector3 position, Vector3 velocity, float duration)
        {
            GenericPickupController.CreatePickupInfo info = new GenericPickupController.CreatePickupInfo
            {
                pickupIndex = pickupIndex,
                rotation = Quaternion.identity,
                position = position,
                prefabOverride = LITTempItems.temporaryItemPickupPrefab,
            };
            TemporaryItemPickupComponent.onAwakeGlobal += (timer) => ModifyTimer(timer, duration);
            PickupDropletController.CreatePickupDroplet(info, position, velocity);          
            TemporaryItemPickupComponent.onAwakeGlobal -= (timer) => ModifyTimer(timer, duration); // THIS DOESNT UNHOOK BUT IDK HOWWWWWWWW
        }

        private static void ModifyTimer(TemporaryItemPickupComponent timer, float duration)
        {
            timer.itemDuration = duration;
            LITLog.Info("TemporaryItemPickupComponent timer modified: " + duration + " seconds");
        }

        // in order to give timers to pickups
        // give override prefab with temporary item component when doing PickupDropletController.CreatePickupDroplet
        // set the temporary item component's time to whatever we want
        // check if context.controller has our temporary item component in AttemptGrant
        // add tracker to inventory and give it the time

        //an artifact that turns all items into temporary should either also disable printers/scrappers or just not use TemporaryItemDefs (and implement their behavior instead)
        public class TemporaryItemDef : ItemDef
        {
            public static void AttemptGrant(ref PickupDef.GrantContext context)
            {
                Inventory inventory = context.body.inventory;
                PickupDef pickupDef = PickupCatalog.GetPickupDef(context.controller.pickupIndex);
                
                ItemIndex itemIndex = (pickupDef != null) ? pickupDef.itemIndex : ItemIndex.None;
                inventory.GiveItem(itemIndex, 1);

                TemporaryItemTracker tracker = inventory.GetComponent<TemporaryItemTracker>();
                if (!tracker)
                {
                    tracker = inventory.gameObject.AddComponent<TemporaryItemTracker>();
                }

                float duration = LITTempItems.fallbackTemporaryItemDuration;
                TemporaryItemPickupComponent timer = context.controller.GetComponent<TemporaryItemPickupComponent>();
                if (timer)
                {
                    duration = timer.itemDuration;
                    LITLog.Info("Pickup timer found: " + itemIndex + " for " + duration + " seconds.");
                }

                tracker.AddTemporaryItemTimer(itemIndex, duration, 1);

                context.shouldDestroy = true;
                context.shouldNotify = true;
            }

            public override PickupDef CreatePickupDef()
            {
                ItemTierDef itemTierDef = ItemTierCatalog.GetItemTierDef(this.tier);
                return new PickupDef
                {
                    internalName = "ItemIndex." + base.name,
                    itemIndex = this.itemIndex,
                    itemTier = this.tier,
                    displayPrefab = this.pickupModelPrefab,
                    dropletDisplayPrefab = ((itemTierDef != null) ? itemTierDef.dropletDisplayPrefab : null),
                    nameToken = this.nameToken,
                    baseColor = ColorCatalog.GetColor(this.colorIndex),
                    darkColor = ColorCatalog.GetColor(this.darkColorIndex),
                    unlockableDef = this.unlockableDef,
                    interactContextToken = "ITEM_PICKUP_CONTEXT",
                    isLunar = (this.tier == ItemTier.Lunar),
                    isBoss = (this.tier == ItemTier.Boss),
                    iconTexture = this.pickupIconTexture,
                    iconSprite = this.pickupIconSprite,
                    attemptGrant = new PickupDef.AttemptGrantDelegate(TemporaryItemDef.AttemptGrant)
                };
            }
        }
        private void ItemCatalog_SetItemDefs(On.RoR2.ItemCatalog.orig_SetItemDefs orig, ItemDef[] newItemDefs)
        {
            List<ItemDef> items = newItemDefs.ToList(); ////////////////////////
            LITLog.Info("hook ItemCatalog_SetItemDefs()");
            foreach (ItemDef item in newItemDefs)
            {

                CustomItem tempItem = CreateTemporaryItemDef(item);
                if (tempItem != null && ItemAPI.Add(tempItem))
                {
                    LITLog.Info(tempItem.ItemDef.name + " ADDED");
                    items.Add(tempItem.ItemDef);
                }
            }
            orig(items.ToArray());

            foreach (KeyValuePair<ItemDef, ItemDef> pair in realToTemporary)
            {
                realToTemporaryIndex.Add(pair.Key.itemIndex, pair.Value.itemIndex);
            }

        }

        public CustomItem CreateTemporaryItemDef(ItemDef original)
        {
            if (original.hidden) return null;

            if (original.tier != ItemTier.Tier1 && original.tier != ItemTier.Tier2 && original.tier != ItemTier.Tier3 && original.tier != ItemTier.Lunar) return null;

            LITLog.Info(original.name + " ATTEMPTING TO CLONE...");

            TemporaryItemDef newItem = ScriptableObject.CreateInstance<TemporaryItemDef>();
            newItem.canRemove = false;
            newItem.descriptionToken = original.descriptionToken; // add "temporary" ?
            newItem.name = "Temporary" + original.name;
            newItem.nameToken = original.nameToken; // use token
            newItem.loreToken = original.loreToken; //
            newItem.pickupIconSprite = original.pickupIconSprite;
            newItem.pickupModelPrefab = original.pickupModelPrefab;
            newItem.pickupToken = original.pickupToken; // use token
            ItemTag[] tags = ArrayUtils.Clone(original.tags);
            ArrayUtils.ArrayAppend(ref tags, temporaryItemTag);
            newItem.tags = tags;
            newItem.unlockableDef = original.unlockableDef; // idk if this might fuck with anything
            newItem.requiredExpansion = original.requiredExpansion;
            ItemTier tier = ItemTier.NoTier;
            switch (original.tier)
            {
                case ItemTier.Tier1:
                    tier = tempTier1.tier;
                    break;
                case ItemTier.Tier2:
                    tier = tempTier2.tier;
                    break;
                case ItemTier.Tier3:
                    tier = tempTier3.tier;
                    break;
                case ItemTier.Lunar:
                    tier = tempLunar.tier;
                    break;
            }
            newItem.tier = tier;


            realToTemporary.Add(original, newItem);
            ItemDisplayRule[] rules = null;
            return new CustomItem(newItem, rules);
        }
    }
}
