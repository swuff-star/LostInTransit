using Moonstorm;
using Moonstorm.Components;
using RoR2;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using System;

namespace LostInTransit.Buffs
{
    [DisabledContent]
    public class AffixFrenzied : BuffBase
    {
        public override BuffDef BuffDef { get; } = LITAssets.LoadAsset<BuffDef>("bdAffixFrenzied", LITBundle.Equips);

        public class AffixFrenziedBehavior : BaseBuffBodyBehavior, IStatItemBehavior
        {
            [BuffDefAssociation(useOnClient = true, useOnServer = true)]
            public static BuffDef GetBuffDef() => LITContent.Buffs.bdAffixFrenzied;

            public float blinkCooldown = 8;

            public GameObject BlinkReadyEffect = LITAssets.LoadAsset<GameObject>("EffectFrenziedTPReady", LITBundle.Equips);

            public GameObject AbilityEffect = LITAssets.LoadAsset<GameObject>("EffectFrenziedAbility", LITBundle.Equips);

            public GameObject BlinkReadyInstance;

            private GameObject AbilityInstance;

            public bool blinkReady;

            private bool doingAbility = false;

            public float blinkStopwatch;

            private float abilityStopwatch;

            private float cdrMult = 1;

            private NetworkIdentity networkIdentity;

            private SetStateOnHurt setStateOnhurt;

            private void Start()
            {
                body.RecalculateStats();
                networkIdentity = gameObject.GetComponent<NetworkIdentity>();
                gameObject.GetComponent<SetStateOnHurt>();
            }

            private void FixedUpdate()
            {
                blinkStopwatch += Time.fixedDeltaTime;
                //abilityStopwatch += doingAbility ? Time.fixedDeltaTime : 0;

                if (blinkStopwatch > blinkCooldown / cdrMult)
                {
                    blinkReady = true;
                    //Debug.Log("network authority: " + Util.HasEffectiveAuthority(gameObject));
                    if (!BlinkReadyInstance)
                    {
                        BlinkReadyInstance = Instantiate(BlinkReadyEffect, body.aimOriginTransform);
                        if (BlinkReadyInstance)
                            BlinkReadyInstance.transform.localScale *= body.radius;
                    }
                }
                /*if (abilityStopwatch >= 10)
                {
                    abilityStopwatch = 0;
                    doingAbility = false;
                    cdrMult = 1;
                    body.RecalculateStats();

                    if (AbilityInstance)
                        Destroy(AbilityInstance);
                }*/

                bool resetDash = body.healthComponent.isInFrozenState;
                if (!resetDash)
                {
                    if (setStateOnhurt)
                    {
                        Type state = setStateOnhurt.targetStateMachine.state.GetType();
                        if (state == typeof(EntityStates.StunState) || state == typeof(EntityStates.ShockState))
                        {
                            resetDash = true;
                        }
                    }    
                }    

                if (resetDash)
                {
                    blinkStopwatch = 0f;
                    blinkReady = false;
                }    

                if (Util.HasEffectiveAuthority(networkIdentity))
                {
                    /*if (blinkReady && body.isPlayerControlled && Input.GetKeyDown(LITConfig.frenziedBlink.Value))
                        Blink();*/
                    if (blinkReady && !body.isPlayerControlled)
                        Blink();
                }
                else
                {
                    Debug.Log("No Network Identity found for Frenzying " + body.baseNameToken + ". Canceling dash.");
                    blinkReady = false;
                    }
            }

            //Todo: turn this ESM stuff into probably a networked body attachment?
            private void Blink()
            {
                
                var bodyStateMachine = EntityStateMachine.FindByCustomName(body.gameObject, "Body");
                if (body.healthComponent.alive && bodyStateMachine)
                {
                    //Todd Howard Voice: It just works.
                    bodyStateMachine.SetNextState(new EntityStates.Elites.FrenziedBlink());
                    blinkStopwatch = 0;
                    blinkReady = false;
                }
            }

            internal void Ability()
            {
                /*doingAbility = true;
                cdrMult = 2;
                AbilityInstance = Instantiate(AbilityEffect, body.aimOriginTransform);
                if (AbilityInstance)
                    AbilityInstance.transform.localScale *= body.bestFitRadius;
                body.RecalculateStats();*/
            }

            public void RecalculateStatsStart() { }
            public void RecalculateStatsEnd()
            {
                body.moveSpeed *= 1.5f;
                body.attackSpeed *= 1.5f;

                //Ability Innactive = 0.5f, 50% cdr
                //Ability Active = 0.75f, 75% cdr
                var cooldownModifier = 0.5f - (0.5f / cdrMult * (cdrMult - 1));

                if (body.skillLocator.primary)
                    body.skillLocator.primary.cooldownScale *= cooldownModifier;
                if (body.skillLocator.secondary)
                    body.skillLocator.secondary.cooldownScale *= cooldownModifier;
                if (body.skillLocator.utility)
                    body.skillLocator.utility.cooldownScale *= cooldownModifier;
                if (body.skillLocator.special)
                    body.skillLocator.special.cooldownScale *= cooldownModifier;
            }

            private void OnDestroy()
            {
                if (BlinkReadyInstance)
                    Destroy(BlinkReadyInstance);
                if (AbilityInstance)
                    Destroy(AbilityInstance);
            }
        }
    }
}
