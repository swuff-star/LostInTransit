using Moonstorm;
using RoR2;
using UnityEngine;
using RoR2.Items;

namespace LostInTransit.Items
{
    public class LifeSavingsUsed : ItemBase
    {
        private const string token = "LIT_ITEM_LIFESAVINGSUSED_DESC";
        public override ItemDef ItemDef { get; } = LITAssets.LoadAsset<ItemDef>("LifeSavingsUsed");
        public static ItemDef itemDef = LITAssets.LoadAsset<ItemDef>("LifeSavingsUsed");
    }
}
