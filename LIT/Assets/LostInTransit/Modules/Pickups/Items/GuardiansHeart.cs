using LostInTransit.Buffs;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Moonstorm;
using RoR2;
using System;
using RoR2.Items;
using R2API;
using UnityEngine.Networking;
using System.Runtime.CompilerServices;
using UnityEngine;
using System.Collections.Generic;

namespace LostInTransit.Items
{
    //[DisabledContent]
    public class GuardiansHeart : ItemBase
    {
        private const string token = "LIT_ITEM_GUARDIANSHEART_DESC";
        public override ItemDef ItemDef { get; } = LITAssets.LoadAsset<ItemDef>("GuardiansHeart", LITBundle.Items);

        [ConfigurableField(ConfigName = "Shield per Heart", ConfigDesc = "Amount of shield added per heart.")]
        public static float extraShield = 60;

        [ConfigurableField(ConfigName = "Bonus Armor", ConfigDesc = "Amount of armor added when heart breaks.")]
        public static float heartArmor = 40;

        [ConfigurableField(ConfigName = "Bonus Armor Duration", ConfigDesc = "Length of the Heart's armor debuff.")]
        public static float heartArmorDur = 3f;

        [ConfigurableField(ConfigName = "Shield Gating", ConfigDesc = "Whether the Heart should block damage past the remaining shield when broken.")]
        public static bool shieldGating = true;

        public static bool hadShield = false;

        public static bool hooked = false;

        public override void Initialize()
        {
            if (LITMain.RiskyModLoaded)
            {
                if (RiskyModShieldGateEnabled())
                {
                    Debug.Log("RiskyMod Shieldgating detected - disabling Guardian's Heart shieldgating");
                    shieldGating = false;
                }
            }

            //Time for an IL hook, i'm sure this will not be a problem :clueless:
            if (shieldGating)
            {
                IL.RoR2.HealthComponent.TakeDamage += (il) =>
                {
                    //This cursor should match to right before "Nonlethal" damage is checked, but after shield and barrier have been removed. This is _after_ riskymod's IL hook
                    ILCursor c = new ILCursor(il);
                    if (c.TryGotoNext(
                        x => x.MatchLdarg(0),
                        x => x.MatchLdfld<HealthComponent>("health"),
                        x => x.MatchLdloc(7),
                        x => x.MatchSub(),
                        x => x.MatchStloc(52),
                        x => x.MatchLdarg(0)
                        ))
                    {
                        c.Index += 4;
                        c.Emit(OpCodes.Ldarg_0);
                        c.Emit(OpCodes.Ldarg_1);
                        c.EmitDelegate<Func<float, HealthComponent, DamageInfo, float>>((healthAfterShieldBreak, self, damageInfo) =>
                        {
                            if (self.body.inventory.GetItemCount(LITContent.Items.GuardiansHeart) > 0
                                && !((damageInfo.damageType & DamageType.BypassArmor) == DamageType.BypassArmor
                                    || (damageInfo.damageType & DamageType.BypassBlock) == DamageType.BypassBlock
                                    || (damageInfo.damageType & DamageType.BypassOneShotProtection) == DamageType.BypassOneShotProtection
                                ))
                            {
                                healthAfterShieldBreak = self.body.maxHealth;
                                self.body.AddTimedBuffAuthority(LITContent.Buffs.bdGuardiansHeartBuff.buffIndex, MSUtil.InverseHyperbolicScaling(heartArmorDur, 1.5f, 7f, self.body.inventory.GetItemCount(LITContent.Items.GuardiansHeart)));
                            }
                            return healthAfterShieldBreak;
                        });
                        hooked = true;
                    }
                    else
                    {
                        LITLog.Fatal("Shieldgating IL Hook failed! Reverting to OnIncomingDamageServer method.");
                        LITLog.Fatal("Guardian's Heart Shieldgating may trigger incorrectly, especially if on enemies.");
                    }
                };
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        private static bool RiskyModShieldGateEnabled()
        {
            return RiskyMod.Tweaks.CharacterMechanics.ShieldGating.enabled;
        }

        public class GuardiansHeartBehavior : BaseItemBodyBehavior, IOnIncomingDamageServerReceiver, IBodyStatArgModifier
        {
            [ItemDefAssociation(useOnClient = true, useOnServer = true)]
            public static ItemDef GetItemDef() => LITContent.Items.GuardiansHeart;

            public float currentShield;
            private CharacterModel model;
            private List<GameObject> displayList;
            private GameObject displayObject;
            private ChildLocator displayCL;
            private Animator displayAnimator;

            public void Awake()
            {
                base.Awake();
                body.RecalculateStats();
            }

            private void FixedUpdate()
            {
                if (NetworkServer.active)
                {
                    bool currentlyHasShield = body.healthComponent.shield > 0;

                    if (displayAnimator != null)
                    {
                        if (body.HasBuff(LITContent.Buffs.bdGuardiansHeartBuff))
                            displayAnimator.speed = 2;
                        else if (currentlyHasShield)
                            displayAnimator.speed = 1;
                        else
                            displayAnimator.speed = 0;
                    }
                }
            }

            private void Start()
            {
                model = body.modelLocator.modelTransform.GetComponent<CharacterModel>();

                if (model != null)
                {
                    displayList = model.GetItemDisplayObjects(LITContent.Items.GuardiansHeart.itemIndex);

                    if (displayList != null)
                    {
                        displayObject = displayList[0];
                        if (displayObject != null)
                        {
                            displayCL = displayObject.GetComponent<ChildLocator>();
                            if (displayCL != null)
                            {
                                displayAnimator = displayCL.FindChild("Base").gameObject.GetComponent<Animator>();
                            }
                        }
                    }
                }
            }

            public void OnIncomingDamageServer(DamageInfo damageInfo)
            {
                //this entire method can only see damage values before they're modified by items/buffs/armor
                //should not be used unless the IL hook is broken, which it probably is because I cant test it - G
                if (shieldGating && !hooked && body.healthComponent.shield > 0f
                    && damageInfo.damage > body.healthComponent.shield + body.healthComponent.barrier
                    //saw that riskymod checks for damage types this way, might prevent the errors idk.
                    && !(  (damageInfo.damageType & DamageType.BypassArmor) == DamageType.BypassArmor
                        || (damageInfo.damageType & DamageType.BypassBlock) == DamageType.BypassBlock
                        || (damageInfo.damageType & DamageType.BypassOneShotProtection) == DamageType.BypassOneShotProtection
                    ))
                {
                    damageInfo.damage = 0f;
                    //damageInfo.rejected = true; 
                    body.healthComponent.barrier = 0f;
                    body.healthComponent.shield = 0f;
                    EffectManager.SpawnEffect(HealthComponent.AssetReferences.shieldBreakEffectPrefab, new EffectData
                    {
                        origin = body.transform.position,
                        scale = body.radius
                    }, transmit: true);
                    EffectManager.SpawnEffect(HealthComponent.AssetReferences.damageRejectedPrefab, new EffectData
                    {
                        origin = damageInfo.position,
                        color = Color.blue
                    }, transmit: true);
                }
            }

            public void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args)
            {
                args.baseShieldAdd += extraShield;
            }
        }
    }
}
