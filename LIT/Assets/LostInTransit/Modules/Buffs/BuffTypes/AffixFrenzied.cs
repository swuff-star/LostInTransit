using Moonstorm;
using Moonstorm.Components;
using RoR2;
using System.Linq;
using UnityEngine;

namespace LostInTransit.Buffs
{
    public class AffixFrenzied : BuffBase
    {
        public override BuffDef BuffDef { get; } = LITAssets.LoadAsset<BuffDef>("AffixFrenzied");

        public class AffixFrenziedBehavior : BaseBuffBodyBehavior, IStatItemBehavior
        {
            [BuffDefAssociation(useOnClient = true, useOnServer = true)]
            public static BuffDef GetBuffDef() => LITContent.Buffs.AffixFrenzied;

            public float blinkCooldown = 8;

            public GameObject BlinkReadyEffect = LITAssets.LoadAsset<GameObject>("EffectFrenziedTPReady");

            public GameObject AbilityEffect = LITAssets.LoadAsset<GameObject>("EffectFrenziedAbility");

            private GameObject BlinkReadyInstance;

            private GameObject AbilityInstance;

            private bool blinkReady = false;

            private bool doingAbility = false;

            private float blinkStopwatch;

            private float abilityStopwatch;

            private float cdrMult = 1;

            private void Start()
            {
                body.RecalculateStats();
            }

            private void FixedUpdate()
            {
                blinkStopwatch += Time.fixedDeltaTime;
                //abilityStopwatch += doingAbility ? Time.fixedDeltaTime : 0;

                if (blinkStopwatch > blinkCooldown / cdrMult)
                {
                    blinkReady = true;
                    Debug.Log("network authority: " + body.hasAuthority);
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

                if (body.hasAuthority) //Util.HasEffectiveAuthority(gameObject)
                {
                    /*if (blinkReady && body.isPlayerControlled && Input.GetKeyDown(LITConfig.frenziedBlink.Value))
                        Blink();*/
                    if (blinkReady && !body.isPlayerControlled)
                        Blink();
                }
            }

            //Todo: turn this ESM stuff into probably a networked body attachment?
            private void Blink()
            {
                if (BlinkReadyInstance)
                    Destroy(BlinkReadyInstance);
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
