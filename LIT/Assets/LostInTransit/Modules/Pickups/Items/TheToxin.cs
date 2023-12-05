using Moonstorm;
using RoR2;
using R2API;
using RoR2.Items;
using UnityEngine.Networking;

namespace LostInTransit.Items
{
    public class TheToxin : ItemBase
    {
        private const string token = "LIT_ITEM_THETOXIN_DESC";
        public override ItemDef ItemDef { get; } = LITAssets.LoadAsset<ItemDef>("TheToxin", LITBundle.Items);

        [ConfigurableField(ConfigName = "Toxin Cooldown", ConfigDesc = "Time in seconds until The Toxin can re-infect.")]
        [TokenModifier(token, StatTypes.Default, 0)]
        public static float toxinCD = 6f;

        [ConfigurableField(ConfigName = "Toxin Duration", ConfigDesc = "Time in seconds that The Toxin infects enemies.")]
        [TokenModifier(token, StatTypes.Default, 1)]
        public static float toxinDur = 8f;

        [ConfigurableField(ConfigName = "Toxin Infection Range", ConfigDesc = "Range of which enemies will become infected by The Toxin.")]
        [TokenModifier(token, StatTypes.Default, 2)]
        public static float toxinRadius = 4f;

        [ConfigurableField(ConfigName = "Toxin Armor Debuff", ConfigDesc = "Armor removed by the debuff inflicted by The Toxin.")]
        [TokenModifier(token, StatTypes.Default, 4)]
        public static float toxinArmorDebuff = 40f;

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
        }
    }
}
