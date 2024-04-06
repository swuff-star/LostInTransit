using MSU;
using MSU.Config;
using RoR2;
using RoR2.ContentManagement;
using RoR2.Items;
using System;
using System.Collections;
using UnityEngine;

namespace LostInTransit.Items
{
    public class Looper : LITItem
    {
        private const string TOKEN = "LIT_ITEM_LOOPER_DESC";

        [RiskOfOptionsConfigureField(LITConfig.ITEMS, ConfigDescOverride = "Maximum extra damage dealt by each stack of Ol' Lopper.")]
        [FormatToken(TOKEN)]
        public static float maxBonus = 0.6f;

        public override NullableRef<GameObject> ItemDisplayPrefab => null;
        public override ItemDef ItemDef => _itemDef;
        private ItemDef _itemDef;

        public override void Initialize()
        {
            throw new NotImplementedException();
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            throw new NotImplementedException();
        }

        public override IEnumerator LoadContentAsync()
        {
            /*
             * ItemDef - "Looper" - Items
             */
            yield break;
        }

        public class LopperBehavior : BaseItemBodyBehavior, IOnIncomingDamageOtherServerReciever
        {
            [ItemDefAssociation(useOnClient = true, useOnServer = true)]
            public static ItemDef GetItemDef() => LITContent.Items.Lopper;

            public void OnIncomingDamageOther(HealthComponent healthComponent, DamageInfo damageInfo)
            {
                damageInfo.damage += stack * (damageInfo.damage * Math.Min(((1f - healthComponent.combinedHealthFraction) * 2f), maxBonus));
            }
        }
    }
}
