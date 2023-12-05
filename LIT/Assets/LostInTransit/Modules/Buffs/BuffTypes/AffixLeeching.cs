//Gooddnight sweet prince... N-
/*using Moonstorm;
using Moonstorm.Components;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace LostInTransit.Buffs
{
    /*public class AffixLeeching : BuffBase
    {
        public override BuffDef BuffDef { get; } = LITAssets.LoadAsset<BuffDef>("bdAffixLeeching");

        public class AffixLeechingBehavior : BaseBuffBodyBehavior, IOnDamageDealtServerReceiver, IOnTakeDamageServerReceiver
        {
            [BuffDefAssociation(useOnClient = true, useOnServer = true)]
            public static BuffDef GetBuffDef() => LITContent.Buffs.bdAffixLeeching;
            
            public float timeBetweenBursts = 10;

            public float chargingTime = 5;

            public float abilityDuration = 20;

            public float regenPercentage;

            public GameObject PrepBurstEffect = LITAssets.LoadAsset<GameObject>("EffectLeechingPrepBurst");

            public GameObject HealingEffect = LITAssets.LoadAsset<GameObject>("EffectLeechingBurst");

            public GameObject TracerEffect = LITAssets.LoadAsset<GameObject>("TracerLeeching");

            public GameObject AbilityEffect = LITAssets.LoadAsset<GameObject>("EffectLeechingAbility");

            private GameObject AbilityEffectInstance;

            private List<HealthComponent> healthComponents = new List<HealthComponent>();

            private SphereSearch healSearch = new SphereSearch();

            private float burstStopwatch;

            private float abilityStopwatch;

            private float prepBurstStopwatch;

            private bool doingAbility;

            private bool preppingRegenBurst;

            private void Start()
            {
                if (body.healthComponent)
                {
                    HG.ArrayUtils.ArrayAppend(ref body.healthComponent.onTakeDamageReceivers, this);
                }
            }

            public void OnDamageDealtServer(DamageReport damageReport)
            {
                int divisor = body.isPlayerControlled ? 2 : 1;
                if (!doingAbility)
                {
                    damageReport.attackerBody?.healthComponent?.Heal((damageReport.damageDealt * damageReport.damageInfo.procCoefficient) / divisor, default);
                }
                else
                {
                    SearchForAllies();
                    float healing = (damageReport.damageDealt * 1.5f / divisor) / healthComponents.Count;
                    foreach (HealthComponent component in healthComponents)
                    {
                        EffectManager.SpawnEffect(TracerEffect, new EffectData
                        {
                            origin = component.body.corePosition,
                            start = body.corePosition
                        }, true);

                        component.Heal(healing, default);
                    }
                }
            }

            public void OnTakeDamageServer(DamageReport damageReport)
            {
                if(preppingRegenBurst)
                {
                    regenPercentage -= 0.5f;
                }
            }

            private void Update()
            {
                prepBurstStopwatch += preppingRegenBurst ? Time.deltaTime : 0;
                abilityStopwatch += doingAbility ? Time.deltaTime : 0;
                burstStopwatch += preppingRegenBurst ? 0 : Time.deltaTime;

                if(burstStopwatch > timeBetweenBursts)
                {
                    burstStopwatch = 0;
                    PrepareBurst();
                }
                if (abilityStopwatch > abilityDuration)
                {
                    doingAbility = false;
                    abilityStopwatch = 0;
                    if (AbilityEffectInstance)
                        Destroy(AbilityEffectInstance);
                }
                if(prepBurstStopwatch > chargingTime && (bool)body.healthComponent?.alive && NetworkServer.active)
                {
                    Burst();
                }
            }

            private void PrepareBurst()
            {
                if(NetworkServer.active)
                {
                    EffectManager.SpawnEffect(PrepBurstEffect, new EffectData
                    {
                        origin = body.corePosition,
                        rootObject = body.gameObject
                    }, true);
                }
                preppingRegenBurst = true;
                regenPercentage = 10;
            }

            private void Burst()
            {
                prepBurstStopwatch = 0;
                preppingRegenBurst = false;
                SearchForAllies();
                bool hasBursted = false;
                healthComponents.Where(hc => hc.body != body)
                    .Where(hc => !hc.body.HasBuff(LITContent.Buffs.bdAffixLeeching))
                    .Where(hc => !hc.body.GetComponent<AffixLeechingBehavior>())
                    .ToList()
                    .ForEach(hc =>
                    {
                        hc.body.AddTimedBuff(LITContent.Buffs.bdAffixLeeching, 5);

                        var component = hc.body.GetComponent<LeechingRegen.LeechingRegenBehavior>();
                        if(component)
                        {
                            component.regenStrength = Math.Max(5, regenPercentage);
                        }

                        EffectManager.SpawnEffect(TracerEffect, new EffectData
                        {
                            origin = hc.body.corePosition,
                            start = body.corePosition
                        }, true);

                        if (!hasBursted)
                        {
                            hasBursted = true;
                            EffectManager.SpawnEffect(HealingEffect, new EffectData
                            {
                                scale = body.radius,
                                origin = body.aimOrigin
                            }, true);
                        }
                    });
            }

            private void SearchForAllies()
            {
                List<HurtBox> hurtBoxes = new List<HurtBox>();
                healthComponents.Clear();

                TeamMask mask = default(TeamMask);
                mask.AddTeam(body.teamComponent.teamIndex);
                healSearch.mask = LayerIndex.entityPrecise.mask;
                healSearch.radius = 25;
                healSearch.origin = body.corePosition;
                healSearch.RefreshCandidates();
                healSearch.FilterCandidatesByHurtBoxTeam(mask);
                healSearch.GetHurtBoxes(hurtBoxes);

                hurtBoxes.ForEach(hb =>
                {
                    if (!healthComponents.Contains(hb.healthComponent) && hb.healthComponent.health < hb.healthComponent.fullHealth)
                        healthComponents.Add(hb.healthComponent);
                });
            }
            public void Ability()
            {
                doingAbility = true;
                abilityStopwatch = 0;
                if (!AbilityEffectInstance)
                {
                    AbilityEffectInstance = Instantiate(AbilityEffect, body.transform);
                    if (AbilityEffectInstance)
                    {
                        AbilityEffectInstance.transform.localScale *= body.bestFitRadius;
                    }
                }
            }

            public void OnDestroy()
            {
                if (AbilityEffectInstance)
                    Destroy(AbilityEffectInstance);

                if (body.healthComponent)
                {
                    int i = Array.IndexOf(body.healthComponent.onIncomingDamageReceivers, this);
                    if (i > -1)
                    {
                        HG.ArrayUtils.ArrayRemoveAtAndResize(ref body.healthComponent.onIncomingDamageReceivers, body.healthComponent.onIncomingDamageReceivers.Length, i);
                    }
                }
            }
        }
    }
}
*/