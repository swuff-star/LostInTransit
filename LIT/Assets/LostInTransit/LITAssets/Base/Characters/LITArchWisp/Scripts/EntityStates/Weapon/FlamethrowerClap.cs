using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Text;
using System.Threading.Tasks;

namespace EntityStates.LITArchWisp.Weapon
{
    //TODO: Add actual clap blast attack with vfx
    public class FlamethrowerClap : BaseState
    {
        public static float baseDuration;
        public static float baseDurationUntilClap;
        public static float clapRadius;
        public static float clapDamageCoefficient;
        public static float clapForce;
        public static Vector3 bonusForce;
        public static GameObject clapEffectPrefab;
        public static GameObject impactEffect;

        private Transform _clapOriginTransform;
        private float _duration;
        private float _durationUntilClap;
        private float _clapDamage;
        private EffectIndex _effectIndex;

        private bool _hasClapped;
        public override void OnEnter()
        {
            base.OnEnter();
            _duration = baseDuration / attackSpeedStat;
            _durationUntilClap = baseDurationUntilClap / attackSpeedStat;
            _clapDamage = damageStat * clapDamageCoefficient;
            var childLocator = GetModelChildLocator();
            if(childLocator)
            {
                _clapOriginTransform = childLocator.FindChild("MuzzleHandR");
            }
            _effectIndex = impactEffect.TryGetComponent<EffectComponent>(out var comp) ? comp.effectIndex : EffectIndex.Invalid;
            PlayCrossfade("Body", "Clap", "Clap.playbackRate", _duration, 0.5f);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if(fixedAge >= _durationUntilClap && !_hasClapped)
            {
                _hasClapped = true;
                Clap();
            }
            if(fixedAge >= _duration && isAuthority)
            {
                outer.SetNextState(new FireFlamethrower());
            }
        }

        private void Clap()
        {

            if (!isAuthority)
                return;

            Vector3 position = _clapOriginTransform ? _clapOriginTransform.position : transform.position;
            EffectManager.SpawnEffect(clapEffectPrefab, new EffectData
            {
                origin = position,
                scale = clapRadius
            }, true);
            BlastAttack attack = new BlastAttack
            {
                attacker = gameObject,
                attackerFiltering = AttackerFiltering.NeverHitSelf,
                baseDamage = _clapDamage,
                baseForce = clapForce,
                bonusForce = bonusForce,
                canRejectForce = false,
                crit = RollCrit(),
                falloffModel = BlastAttack.FalloffModel.SweetSpot,
                inflictor = gameObject,
                losType = BlastAttack.LoSType.NearestHit,
                position = _clapOriginTransform ? _clapOriginTransform.position : transform.position,
                procCoefficient = 1,
                radius = clapRadius,
                teamIndex = teamComponent.teamIndex,
                impactEffect = _effectIndex
            };
            attack.Fire();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Pain;
        }
    }
}
