using LostInTransit.Buffs;
using Moonstorm;
using RoR2;
using System;
using RoR2.Items;
using R2API;
using UnityEngine.Networking;
using System.Runtime.CompilerServices;

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

        public static bool hadShield = false;

        /*public override void Initialize()
        { 
            if (LITMain.RiskyModLoaded)
            {
                [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
                static bool RiskyModShieldGateEnabled()
                {
                    return LITMain.RiskyModLoaded && RiskyMod.Tweaks.CharacterMechanics.ShieldGating.enabled;
                }

                [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
                static bool RiskyModTrueOSPEnabled()
                {
                    return LITMain.RiskyModLoaded && RiskyMod.Tweaks.CharacterMechanics.TrueOSP.enabled;
                }

                static DamageAPI.ModdedDamageType RiskyModGetIgnoreShieldGateDamageType()
                {
                    return LITMain.RiskyModLoaded && RiskyMod.Tweaks.CharacterMechanics.ShieldGating.IgnoreShieldGateDamage;
                    ...and that's where that ends, bc I think I need to use full r2api to reference this, and i don't want to do that :(
                }
            }

        }*/

        public class GuardiansHeartBehavior : BaseItemBodyBehavior, IOnIncomingDamageServerReceiver, IBodyStatArgModifier
        {
            [ItemDefAssociation(useOnClient = true, useOnServer = true)]
            public static ItemDef GetItemDef() => LITContent.Items.GuardiansHeart;

            public float currentShield;

            public void Awake()
            {
                body.RecalculateStats();
            }

            private void FixedUpdate()
            {
                if (NetworkServer.active)
                {
                    bool currentlyHasShield = body.healthComponent.shield > 0;

                    if (hadShield && !currentlyHasShield)
                    {
                        body.AddTimedBuffAuthority(LITContent.Buffs.GuardiansHeartBuff.buffIndex, MSUtil.InverseHyperbolicScaling(heartArmorDur, 1.5f, 7f, stack));
                    }

                    hadShield = currentlyHasShield;
                }
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
                }
            }

            public void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args)
            {
                args.baseShieldAdd += extraShield;
            }
        }
    }
}
