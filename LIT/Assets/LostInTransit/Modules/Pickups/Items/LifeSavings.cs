using Moonstorm;
using RoR2;
using UnityEngine;
using RoR2.Items;

namespace LostInTransit.Items
{
    public class LifeSavings : ItemBase
    {
        private const string token = "LIT_ITEM_LIFESAVINGS_DESC";
        public override ItemDef ItemDef { get; } = LITAssets.LoadAsset<ItemDef>("LifeSavings");
        public static ItemDef itemDef = LITAssets.LoadAsset<ItemDef>("LifeSavings");

        [ConfigurableField(ConfigName = "Money per Life Savings", ConfigDesc = "Money granted per Life Savings.")]
        [TokenModifier(token, StatTypes.Default, 0)]
        public static int moneyPerSavings = 75;

        public override void Initialize()
        {
            CharacterBody.onBodyStartGlobal += GiveMoney;
        }

        private void GiveMoney(CharacterBody obj)
        {
            if (!obj.inventory)
                return;
            var inv = obj.inventory;
            int count = inv.GetItemCount(ItemDef);
            if (count >= 1)
            {
                obj.master.GiveMoney((uint)Run.instance.GetDifficultyScaledCost(moneyPerSavings));
                //EffectManager.SimpleImpactEffect(HealthComponent.AssetReferences.gainCoinsImpactEffectPrefab, obj.corePosition, Vector3.up, true);
                //EffectManager.SimpleImpactEffect(HealthComponent.AssetReferences.gainCoinsImpactEffectPrefab, obj.corePosition, Vector3.up, true);
                inv.RemoveItem(ItemDef);
                inv.GiveItem(LifeSavingsUsed.itemDef);
                CharacterMasterNotificationQueue.SendTransformNotification(obj.master, itemDef.itemIndex, LifeSavingsUsed.itemDef.itemIndex, CharacterMasterNotificationQueue.TransformationType.Default);
                //Debug.Log("Giving " + Run.instance.GetDifficultyScaledCost(25 * count) + " money");
            }
        }
    }
}
