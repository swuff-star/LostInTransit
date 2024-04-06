using MSU;
using RoR2;
using R2API;
using RoR2.Items;
using System;
using UnityEngine;
using MSU.Config;
using RoR2.ContentManagement;
using System.Collections;

namespace LostInTransit.Items
{
    public sealed class WickedRing : LITItem
    {
        private const string TOKEN = "LIT_ITEM_WICKEDRING_DESC";

        [RiskOfOptionsConfigureField(LITConfig.ITEMS, ConfigDescOverride = "Seconds removed from skill cooldowns on kill.")]
        [FormatToken(TOKEN, 0)]
        public static float secondsRemovedPerKill = 1f;

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
             * ItemDef - "WickedRingNew" - Items
             */
            yield break;
        }
        public class WickedRingBehavior : BaseItemBodyBehavior, IOnKilledOtherServerReceiver
        {
            [ItemDefAssociation(useOnClient = true, useOnServer = true)]
            public static ItemDef GetItemDef() => LITContent.Items.WickedRingNew;

            public void OnKilledOtherServer(DamageReport damageReport)
            {
                if (body.skillLocator)
                    body.skillLocator.DeductCooldownFromAllSkillsServer(secondsRemovedPerKill * stack);
            }
        }
    }
}
