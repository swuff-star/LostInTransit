using LostInTransit.Buffs;
using Moonstorm;
using RoR2;
using System;
using RoR2.Items;
using R2API;

namespace LostInTransit.Items
{
    //[DisabledContent]
    public class GuardiansHeart : ItemBase
    {
        private const string token = "LIT_ITEM_GUARDIANSHEART_DESC";
        public override ItemDef ItemDef { get; } = LITAssets.LoadAsset<ItemDef>("GuardiansHeart");

        [ConfigurableField(ConfigName = "Shield per Heart", ConfigDesc = "Amount of shield added per heart.")]
        public static float extraShield = 60;

        [ConfigurableField(ConfigName = "Bonus Armor", ConfigDesc = "Amount of armor added when heart breaks.")]
        public static float heartArmor = 40;

        [ConfigurableField(ConfigName = "Bonus Armor Duration", ConfigDesc = "Length of the Heart's armor debuff.")]
        public static float heartArmorDur = 3f;

        [ConfigurableField(ConfigName = "Shield Gating", ConfigDesc = "Whether the Heart should block damage past the remaining shield when broken.")]
        public static bool shieldGating = true;

        public class GuardiansHeartBehavior : BaseItemBodyBehavior, IOnIncomingDamageServerReceiver, IBodyStatArgModifier
        {
            [ItemDefAssociation(useOnClient = true, useOnServer = true)]
            public static ItemDef GetItemDef() => LITContent.Items.GuardiansHeart;

            public float currentShield;

            public void Start()
            {
                body.RecalculateStats();
            }
            private void FixedUpdate()
            {
                currentShield = body.healthComponent.shield;
            }
            public void OnIncomingDamageServer(DamageInfo damageInfo)
            {
                if (body.healthComponent.shield >= 1f && damageInfo.damage >= body.healthComponent.shield + body.healthComponent.barrier)
                {
                    if (shieldGating == true && !(damageInfo.damageType == DamageType.BypassArmor || damageInfo.damageType == DamageType.BypassOneShotProtection))
                    {
                        damageInfo.damage = body.healthComponent.shield + body.healthComponent.barrier;
                        body.healthComponent.shield = 0f;
                    }

                    if (heartArmor > 0f)
                    {
                        body.AddTimedBuffAuthority(LITContent.Buffs.GuardiansHeartBuff.buffIndex, MSUtil.InverseHyperbolicScaling(heartArmorDur, 1.5f, 7f, stack));
                    }
                }
            }

            public void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args)
            {
                args.baseShieldAdd += extraShield;
            }
        }
    }
}
