using LostInTransit.Buffs;
using Moonstorm;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using RoR2.Items;

namespace LostInTransit.Items
{
    //[DisabledContent]
    public class RepulsionArmor : ItemBase
    {
        private const string token = "LIT_ITEM_REPULCHEST_DESC";
        public override ItemDef ItemDef { get; } = LITAssets.LoadAsset<ItemDef>("Chestplate");

        [ConfigurableField(ConfigName = "Hits Needed to Activate", ConfigDesc = "Amount of times required to take damage before activating Repulsion Armor.")]
        [TokenModifier(token, StatTypes.Default, 0)]
        public static int hitsNeededConfig = 6;

        [ConfigurableField(ConfigName = "Hits Needed per Stack", ConfigDesc = "Amount of extra hits needed per stack to activate Repulsion Armor.")] //This kinda sucks but is easy to include if anyone wanted it for some god-forsaken reason.
        public static float hitsNeededConfigStack = 0f;

        [ConfigurableField(ConfigName = "Base Duration of Buff", ConfigDesc = "Amount of time the Repulsion Armor buff lasts.")]
        [TokenModifier(token, StatTypes.Default, 2)]
        public static float buffBaseLength = 3f;

        [ConfigurableField(ConfigName = "Stacking Duration of Buff", ConfigDesc = "Extra aount of time added to the Repulsion Armor buff per stack.")]
        [TokenModifier(token, StatTypes.Default, 3)]
        public static float buffStackLength = 1.5f;

        [ConfigurableField(ConfigName = "Maximum Duration", ConfigDesc = "Maximum length of the Repulsion Armor buff. Set to 0 to disable.")]
        public static float durCap = 0f;

        [ConfigurableField(ConfigName = "Damage Reduction", ConfigDesc = "Amount of armor added while the Repulsion Armor buff is active.")]
        [TokenModifier(token, StatTypes.Default, 1)]
        public static float damageResist = 500f;

        public static bool badFix = false;

        public class RepulsionArmorBehavior : BaseItemBodyBehavior, IOnIncomingDamageServerReceiver
        {
            [ItemDefAssociation(useOnClient = true, useOnServer = true)]
            public static ItemDef GetItemDef() => LITContent.Items.Chestplate;

            public void FixedUpdate()
            {
                if (!body.HasBuff(LITContent.Buffs.RepulsionArmorActive) && !body.HasBuff(LITContent.Buffs.RepulsionArmorCD))
                {
                    body.SetBuffCount(LITContent.Buffs.RepulsionArmorCD.buffIndex, hitsNeededConfig);    //hitsNeededConfig should be an int
                }
            }

            public void OnIncomingDamageServer(DamageInfo damageInfo)
            {
                if (damageInfo.attacker == body) return;

                int currentCDCount = (body.GetBuffCount(LITContent.Buffs.RepulsionArmorCD));
                if (currentCDCount > 0)
                {
                    body.RemoveBuff(LITContent.Buffs.RepulsionArmorCD);
                    currentCDCount--;

                    if (currentCDCount <= 0)
                    {
                        body.AddTimedBuff(LITContent.Buffs.RepulsionArmorActive.buffIndex, (buffBaseLength + buffStackLength * (stack - 1)));
                    }
                }
            }
        }
    }
}
