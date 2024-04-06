using HG;
using RoR2;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace EntityStates.AffixVolatile
{
    public class SelfDestruct : BaseBodyAttachmentState
    {
        public static float baseDuration;
        public static GameObject chargingEffectPrefab;
        [HideInInspector]
        public static GameObject explosionEffectPrefab = Resources.Load<GameObject>("Prefabs/Effects/OmniEffect/OmniExplosionVFXQuick");
        public static string chargingSoundString;
        public static string explosionSoundString;
        public static float baseExplosionRadius;
        public static float baseForce;

        private float _duration;
        private float _radius;
        private Transform _modelTransform;
        private GameObject _chargingEffectInstance;
        private uint _soundID;
        private float _stopwatch;
        private bool _hasExploded;
        public override void OnEnter()
        {
            base.OnEnter();
            if (!attachedBody)
                return;
            _modelTransform = attachedBody.modelLocator.modelTransform;
            if (!_modelTransform)
                return;

            _duration = baseDuration / attachedBody.attackSpeed;
            _radius = baseExplosionRadius * attachedBody.radius;

            _soundID = Util.PlayAttackSpeedSound(chargingSoundString, gameObject, attachedBody.attackSpeed);

            if (chargingEffectPrefab && !_chargingEffectInstance)
            {
                _chargingEffectInstance = Object.Instantiate(chargingEffectPrefab, _modelTransform.position, _modelTransform.rotation);
                if (_chargingEffectInstance)
                {
                    _chargingEffectInstance.transform.parent = attachedBody.transform;
                    _chargingEffectInstance.transform.localScale *= _radius * 2.5f;
                    var component1 = _chargingEffectInstance.GetComponent<ScaleParticleSystemDuration>();
                    var component2 = _chargingEffectInstance.GetComponentInChildren<LightIntensityCurve>();

                    if (component1)
                        component1.newDuration = _duration;

                    if (component2)
                    {
                        component2.timeMax = _duration;
                        component2.maxIntensity *= _radius;
                    }
                }
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            _stopwatch += Time.fixedDeltaTime;
            if(_stopwatch > _duration && isAuthority && !_hasExploded)
            {
                Detonate();
                outer.SetNextState(new PostSelfDestruct());
            }
        }

        private void Detonate()
        {
            _hasExploded = true;
            Util.PlaySound(explosionSoundString, gameObject);
            if (_chargingEffectInstance)
                Destroy(_chargingEffectInstance);

            EffectManager.SpawnEffect(explosionEffectPrefab, new EffectData
            {
                origin = attachedBody.transform.position,
                scale = _radius,
                rotation = Util.QuaternionSafeLookRotation(Vector3.one)
            }, transmit: true);

            //We're just using this to get the hits, the blast attack itself doesnt deal damage.
            BlastAttack attack = new BlastAttack
            {
                attacker = attachedBody.gameObject,
                inflictor = attachedBody.gameObject,
                teamIndex = attachedBody.teamComponent.teamIndex,
                baseDamage = 0,
                position = attachedBody.transform.position,
                radius = _radius,
                procCoefficient = 0f,
                attackerFiltering = AttackerFiltering.NeverHitSelf,
                canRejectForce = true,
                damageColorIndex = DamageColorIndex.Default,
                damageType = DamageType.Generic,
                falloffModel = BlastAttack.FalloffModel.SweetSpot,
                losType = BlastAttack.LoSType.NearestHit
            };

            bool isCrit = attachedBody.RollCrit();
            var result = attack.Fire();
            for(int i = 0; i < result.hitCount; i++)
            {
                var hit = result.hitPoints[i];

                var hurtbox = hit.hurtBox;
                if (!hurtbox)
                    continue;

                var healthComponent = hurtbox.healthComponent;
                if (!healthComponent)
                    continue;

                var distance = Mathf.Sqrt(hit.distanceSqr);
                float falloffCoef = 1f - ((distance > _radius / 2) ? 0.75f : 0f);
                Vector3 forceDirection = ((distance > 0f) ? ((hit.hitPosition - attachedBody.transform.position) / distance) : Vector3.zero);
                DamageInfo actualDamage = new DamageInfo
                {
                    attacker = attachedBody.gameObject,
                    inflictor = attachedBody.gameObject,
                    canRejectForce = false,
                    force = baseForce * falloffCoef * forceDirection,
                    crit = isCrit,
                    damage = healthComponent.fullHealth * falloffCoef,
                    damageType = DamageType.AOE | (attachedBody.isPlayerControlled ? DamageType.Generic : DamageType.NonLethal),
                    position = attachedBody.transform.position,
                    procCoefficient = 1f,
                };
                healthComponent.TakeDamage(actualDamage);
            }
            attachedBody.healthComponent.Suicide(attachedBody.gameObject, attachedBody.gameObject, DamageType.Generic);
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Death;
        }

        public override void OnExit()
        {
            base.OnExit();
            //AKSoundEngine.StopPlayingID(soundID);
            if (_chargingEffectInstance)
            {
                Destroy(_chargingEffectInstance);
            }
        }
    }
}
