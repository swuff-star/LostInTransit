using LostInTransit.Buffs;
using MSU;
using MSU.Config;
using RoR2;
using RoR2.ContentManagement;
using RoR2.Items;
using System.Collections;
using UnityEngine;

namespace LostInTransit.Items
{
    public sealed class PrisonShackles : LITItem
    {
        private const string TOKEN = "LIT_ITEM_PRISONSHACKLES_DESC";

        [RiskOfOptionsConfigureField(LITConfig.ITEMS, ConfigDescOverride = "Multiplier added to the shackled body's movement speed.")]
        [FormatToken(TOKEN, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100)]
        public static float slowMultiplier = 0.3f;

        [RiskOfOptionsConfigureField(LITConfig.ITEMS, ConfigDescOverride = "Base duration of the Shackled debuff.")]
        [FormatToken(TOKEN, 1)]
        public static int duration = 2;

        [RiskOfOptionsConfigureField(LITConfig.ITEMS, ConfigDescOverride = "Extra duration of the Shackled debuff per stack of shackles.")]
        [FormatToken(TOKEN, 2)]
        public static int durationStack = 2;

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
             * ItemDef - "PrisonShackles" - Items
             */
            yield break;
        }

        public class PrisonShacklesBehavior : BaseItemBodyBehavior, IOnDamageDealtServerReceiver
        {
            [ItemDefAssociation(useOnClient = true, useOnServer = true)]
            public static ItemDef GetItemDef() => LITContent.Items.PrisonShackles;
            public void OnDamageDealtServer(DamageReport damageReport)
            {
                if (damageReport.damageInfo.procCoefficient > 0)
                    damageReport.victimBody.AddTimedBuff(LITContent.Buffs.bdShackled, duration + durationStack * (stack - 1));
            }
        }
    }
}
