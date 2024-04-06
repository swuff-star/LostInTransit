using MSU;
using RoR2;
using R2API;
using RoR2.Items;
using UnityEngine.Networking;
using UnityEngine;
using RoR2.ContentManagement;
using System.Collections;
using MSU.Config;

namespace LostInTransit.Items
{
    public sealed class TheToxin : LITItem
    {
        private const string TOKEN = "LIT_ITEM_THETOXIN_DESC";

        [RiskOfOptionsConfigureField(LITConfig.ITEMS, ConfigDescOverride = "Time in seconds until The Toxin can re-infect.")]
        [FormatToken(TOKEN, 0)]
        public static float toxinCooldown = 6f;

        [RiskOfOptionsConfigureField(LITConfig.ITEMS, ConfigDescOverride = "Time in seconds that The Toxin infects enemies.")]
        [FormatToken(TOKEN, 1)]
        public static float toxinDuration = 8f;

        [RiskOfOptionsConfigureField(LITConfig.ITEMS, ConfigDescOverride = "Range of which enemies will become infected by The Toxin.")]
        [FormatToken(TOKEN, 2)]
        public static float toxinRadius = 8f;

        [RiskOfOptionsConfigureField(LITConfig.ITEMS, ConfigDescOverride = "Armor removed by the debuff inflicted by The Toxin.")]
        [FormatToken(TOKEN, 3)]
        public static float toxinArmorReduction = 40f;

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
             * ItemDef - "TheToxin" - Items
             */
            yield break;
        }

        public class TheToxinBehavior : BaseItemBodyBehavior
        {
            [ItemDefAssociation(useOnClient = true, useOnServer = true)]
            public static ItemDef GetItemDef() => LITContent.Items.TheToxin;

            public void FixedUpdate()
            {
                if (!body.HasBuff(LITContent.Buffs.bdToxinCooldown) && !body.HasBuff(LITContent.Buffs.bdToxinReady))
                {
                    body.SetBuffCount(LITContent.Buffs.bdToxinReady.buffIndex, 1);
                }
            }

            public void OnDestroy()
            {
                if (body.GetItemCount(LITContent.Items.TheToxin) == 0)
                {
                    body.SetBuffCount(LITContent.Buffs.bdToxinReady.buffIndex, 0);
                    body.SetBuffCount(LITContent.Buffs.bdToxinCooldown.buffIndex, 0);
                }
            }
        }
    }
}
