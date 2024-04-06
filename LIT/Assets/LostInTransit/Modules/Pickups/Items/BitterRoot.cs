using MSU;
using RoR2;
using RoR2.Items;
using UnityEngine;
using R2API;
using System;
using RoR2.ContentManagement;
using System.Collections;
using MSU.Config;

namespace LostInTransit.Items
{
    public sealed class BitterRoot : LITItem
    {
        public const string TOKEN = "LIT_ITEM_BITTERROOT_DESC";

        [RiskOfOptionsConfigureField(LITConfig.ITEMS, ConfigDescOverride = "Amount of regen on kill per Root.")]
        [FormatToken(TOKEN, 0)]
        public static float regenAmount = 3f;

        [RiskOfOptionsConfigureField(LITConfig.ITEMS, ConfigDescOverride = "Duration of regen on kill per Root.")]
        [FormatToken(TOKEN, 1)]
        public static float regenDuration = 3f;

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
             * ItemDef - "BitterRoot" - Items
             */
            yield break;
        }

        public class BitterRootBehavior : BaseItemBodyBehavior, IOnKilledOtherServerReceiver
        {
            [ItemDefAssociation(useOnClient = true, useOnServer = true, behaviorTypeOverride = typeof(BitterRootBehavior))]
            public static ItemDef GetItemDef() => LITContent.Items.BitterRoot;

            public void OnKilledOtherServer(DamageReport damageReport)
            {
                body.AddTimedBuffAuthority(LITContent.Buffs.bdRootRegen.buffIndex, regenDuration * stack);
            }
        }
    }
}
