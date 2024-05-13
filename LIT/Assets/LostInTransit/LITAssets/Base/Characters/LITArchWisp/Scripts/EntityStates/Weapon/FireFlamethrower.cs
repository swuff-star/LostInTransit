using LostInTransit;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace EntityStates.LITArchWisp.Weapon
{
    public class FireFlamethrower : BaseState
    {
        public static float baseDuration;
        public static float totalDamageCoefficient;
        public static float procCoefficientPerTick;
        public static float tickFrequency;
        public static GameObject _flamethrowerEffectPrefab;

        [Header("Bullet Metadata")]
        public static float radius;
        public static float maxDistance;
        public static float force;

        private float _duration;
        private float _tickDamageCoefficient;
        private float _flamethrowerStopwatch;
        private bool _isCrit = false;
        private Transform _muzzle;
        private Transform _flamethrowerEffectInstanceTransform;

        public override void OnEnter()
        {
            base.OnEnter();
            _duration = baseDuration;
            ChildLocator locator = GetModelChildLocator();
            _muzzle = locator ? locator.FindChild("FlamethrowerOrigin") : transform;
            int num = Mathf.CeilToInt(_duration * tickFrequency);
            _tickDamageCoefficient = totalDamageCoefficient / num;
            
            if(isAuthority)
            {
                _isCrit = RollCrit();
            }

            PlayCrossfade("Body", "Flamethrower", 0.25f);

            //Setup VFX
            if(_muzzle)
            {
                _flamethrowerEffectInstanceTransform = GameObject.Instantiate(_flamethrowerEffectPrefab, _muzzle).transform;
                _flamethrowerEffectInstanceTransform.localPosition = Vector3.zero;
            }
            FireGauntlet(GetAimRay());
        }

        public override void OnExit()
        {
            Destroy(_flamethrowerEffectInstanceTransform.gameObject);
            PlayCrossfade("Body", "FlamethrowerStop", 0.25f);
            //Spawn fx accordingly
            base.OnExit();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            _flamethrowerStopwatch += Time.fixedDeltaTime;
            float num = 1f / tickFrequency / attackSpeedStat;
            Ray aimRay = GetAimRay();
            if (_flamethrowerStopwatch > num)
            {
                _flamethrowerStopwatch -= num;
                FireGauntlet(aimRay);
            }
            //Update flamethrower fx
            _flamethrowerEffectInstanceTransform.forward = aimRay.direction;
            if (fixedAge >= _duration && isAuthority)
                outer.SetNextStateToMain();
        }

        private void FireGauntlet(Ray aimRay)
        {
            if(!isAuthority)
            {
                return;
            }
            BulletAttack attack = new BulletAttack
            {
                aimVector = aimRay.direction,
                bulletCount = 1,
                damage = _tickDamageCoefficient * damageStat,
                falloffModel = BulletAttack.FalloffModel.None,
                force = force,
                isCrit = _isCrit,
                maxDistance = maxDistance,
                minSpread = 0,
                muzzleName = "FlamethrowerOrigin",
                origin = _muzzle ? _muzzle.position : aimRay.origin,
                owner = gameObject,
                procCoefficient = procCoefficientPerTick,
                radius = radius,
                smartCollision = true,
                stopperMask = LayerIndex.world.mask,
                weapon = gameObject,
            };
            attack.Fire();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Pain;
        }
    }
}