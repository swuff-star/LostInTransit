using LostInTransit.Buffs;
using MSU;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using RoR2.Items;
using RoR2.ContentManagement;
using System.Collections;
using MSU.Config;

namespace LostInTransit.Items
{
    public sealed class RepulsionArmor : LITItem
    {
        private const string TOKEN = "LIT_ITEM_REPULCHEST_DESC";

        [RiskOfOptionsConfigureField(LITConfig.ITEMS, ConfigDescOverride = "Amount of times required to take damage before activating Repulsion Armor.")]
        [FormatToken(TOKEN, 0)]
        public static int hitsNeeded = 6;

        [RiskOfOptionsConfigureField(LITConfig.ITEMS, ConfigDescOverride = "Amount of extra hits needed per stack to activate Repulsion Armor.")] //This kinda sucks but is easy to include if anyone wanted it for some god-forsaken reason.
        public static float hitsNeededPerStack = 0f;

        [RiskOfOptionsConfigureField(LITConfig.ITEMS, ConfigDescOverride = "Amount of armor added while the Repulsion Armor buff is active.")]
        [FormatToken(TOKEN, 1)]
        public static float armorBonus = 500f;

        [RiskOfOptionsConfigureField(LITConfig.ITEMS, ConfigDescOverride = "Amount of time the Repulsion Armor buff lasts.")]
        [FormatToken(TOKEN, 2)]
        public static float buffBaseDuration = 3f;

        [RiskOfOptionsConfigureField(LITConfig.ITEMS, ConfigDescOverride = "Extra aount of time added to the Repulsion Armor buff per stack.")]
        [FormatToken(TOKEN, 3)]
        public static float buffStackDuration = 1.5f;

        [RiskOfOptionsConfigureField(LITConfig.ITEMS, ConfigDescOverride = "Maximum length of the Repulsion Armor buff. Set to 0 to disable.")]
        public static float maximumBuffDuration = 0f;

        public static bool badFix = false;

        public override NullableRef<GameObject> ItemDisplayPrefab => null;
        public override ItemDef ItemDef => _itemDef;
        private ItemDef _itemDef;

        public override void Initialize()
        {
            throw new System.NotImplementedException();
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            throw new System.NotImplementedException();
        }

        public override IEnumerator LoadContentAsync()
        {
            /*
             * ItemDef - "Chestplate" - Items
             */
            yield break;
        }

        public class RepulsionArmorBehavior : BaseItemBodyBehavior, IOnIncomingDamageServerReceiver
        {
            [ItemDefAssociation(useOnClient = true, useOnServer = true)]
            public static ItemDef GetItemDef() => LITContent.Items.Chestplate;

            public void FixedUpdate()
            {
                if (!body.HasBuff(LITContent.Buffs.bdRepulsionArmorActive) && !body.HasBuff(LITContent.Buffs.bdRepulsionArmorCD))
                {
                    body.SetBuffCount(LITContent.Buffs.bdRepulsionArmorCD.buffIndex, hitsNeeded);
                }
            }

            public void OnIncomingDamageServer(DamageInfo damageInfo)
            {
                if (damageInfo.attacker == body) return;

                int currentCDCount = (body.GetBuffCount(LITContent.Buffs.bdRepulsionArmorCD));
                if (currentCDCount > 0)
                {
                    body.RemoveBuff(LITContent.Buffs.bdRepulsionArmorCD);
                    currentCDCount--;

                    if (currentCDCount <= 0)
                    {
                        body.AddTimedBuff(LITContent.Buffs.bdRepulsionArmorActive.buffIndex, (buffBaseDuration + buffStackDuration * (stack - 1)));
                    }
                }
            }
        }
    }
}
