using LostInTransit.Buffs;
using MSU;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using RoR2.Items;

namespace LostInTransit.Items
{
    //[DisabledContent]
    public class RepulsionArmor : LITItem
    {
        private const string token = "LIT_ITEM_REPULCHEST_DESC";
        public override ItemDef ItemDef { get; } = LITAssets.LoadAsset<ItemDef>("Chestplate", LITBundle.Items);

        [RiskOfOptionsConfigureField(ConfigNameOverride = "Hits Needed to Activate", ConfigDescOverride = "Amount of times required to take damage before activating Repulsion Armor.")]
        [TokenModifier(token, StatTypes.Default, 0)]
        public static int hitsNeededConfig = 6;

        [RiskOfOptionsConfigureField(ConfigNameOverride = "Hits Needed per Stack", ConfigDescOverride = "Amount of extra hits needed per stack to activate Repulsion Armor.")] //This kinda sucks but is easy to include if anyone wanted it for some god-forsaken reason.
        public static float hitsNeededConfigStack = 0f;

        [RiskOfOptionsConfigureField(ConfigNameOverride = "Base Duration of Buff", ConfigDescOverride = "Amount of time the Repulsion Armor buff lasts.")]
        [TokenModifier(token, StatTypes.Default, 2)]
        public static float buffBaseLength = 3f;

        [RiskOfOptionsConfigureField(ConfigNameOverride = "Stacking Duration of Buff", ConfigDescOverride = "Extra aount of time added to the Repulsion Armor buff per stack.")]
        [TokenModifier(token, StatTypes.Default, 3)]
        public static float buffStackLength = 1.5f;

        [RiskOfOptionsConfigureField(ConfigNameOverride = "Maximum Duration", ConfigDescOverride = "Maximum length of the Repulsion Armor buff. Set to 0 to disable.")]
        public static float durCap = 0f;

        [RiskOfOptionsConfigureField(ConfigNameOverride = "Damage Reduction", ConfigDescOverride = "Amount of armor added while the Repulsion Armor buff is active.")]
        [TokenModifier(token, StatTypes.Default, 1)]
        public static float damageResist = 500f;

        public static bool badFix = false;

        public class RepulsionArmorBehavior : BaseItemBodyBehavior, IOnIncomingDamageServerReceiver
        {
            [ItemDefAssociation(useOnClient = true, useOnServer = true)]
            public static ItemDef GetItemDef() => LITContent.Items.Chestplate;

            public void FixedUpdate()
            {
                if (!body.HasBuff(LITContent.Buffs.bdRepulsionArmorActive) && !body.HasBuff(LITContent.Buffs.bdRepulsionArmorCD))
                {
                    body.SetBuffCount(LITContent.Buffs.bdRepulsionArmorCD.buffIndex, hitsNeededConfig);    //hitsNeededConfig should be an int
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
                        body.AddTimedBuff(LITContent.Buffs.bdRepulsionArmorActive.buffIndex, (buffBaseLength + buffStackLength * (stack - 1)));
                    }
                }
            }
        }
    }
}
