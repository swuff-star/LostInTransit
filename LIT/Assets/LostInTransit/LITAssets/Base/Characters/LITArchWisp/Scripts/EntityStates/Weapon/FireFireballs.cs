using LostInTransit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;
using RoR2.Projectile;
using RoR2;

namespace EntityStates.LITArchWisp.Weapon
{
    public class FireFireballs : FireballsBaseState
    {
        public static float baseDuration;
        public static float baseDurationUntilFiring;
        public static float totalDamageCoefficient;
        public static GameObject chargeVFX;
        public static GameObject projectilePrefab;

        private float _duration;
        private float _durationUntilFiring;
        private float _fireTimer;

        private Transform[] _muzzleVFXTransforms;
        private Transform _muzzleProjectileTransform;
        private GameObject[] _chargeFXInstances = new GameObject[3];
        private Quaternion[] _fireSpread = new Quaternion[3]
        {
            Quaternion.Euler(-3, 0, 0),
            Quaternion.Euler(3, 3, 0),
            Quaternion.Euler(3, -3, 0)
        };

        private float _damagePerFireball;
        private bool _hasFiredThisCycle = true;
        private int _muzzleIndex = 1;

        public override void OnEnter()
        {
            base.OnEnter();
            _duration = baseDuration / attackSpeedStat;
            _durationUntilFiring = baseDurationUntilFiring / attackSpeedStat;
            _fireTimer = _duration;
            _damagePerFireball = damageStat * (totalDamageCoefficient / 3);
            StartCycle();
        }

        private void StartCycle()
        {
            _fireTimer -= _duration;
            _hasFiredThisCycle = false;
            _muzzleIndex = _muzzleIndex == 0 ? 1 : 0;
            string animName = "FireballFire";
            switch (_muzzleIndex)
            {
                case 0:
                    animName += "L";
                    _muzzleVFXTransforms = leftHandMuzzleTransforms;
                    _muzzleProjectileTransform = leftHandCentralMuzzleTransform;
                    break;
                case 1:
                    animName += "R";
                    _muzzleVFXTransforms = rightHandMuzzleTransforms;
                    _muzzleProjectileTransform = rightHandCentralMuzzleTransform;
                    break;
            }
            PlayCrossfade("Body", animName, "FireballFire.playbackRate", _duration, 0.25f);

            //Spawn vfx on each muzzle and time them;
            for (int i = 0; i < _chargeFXInstances.Length; i++)
            {
                Transform t = _muzzleVFXTransforms[i];
                var instance = GameObject.Instantiate(chargeVFX, t.position, t.rotation, t);
                _chargeFXInstances[i] = instance;
            }

#if DEBUG
            LITLog.Info($"FireTimer={_fireTimer}, MuzzleIndex={_muzzleIndex}({(_muzzleIndex == 0 ? "Left" : "Right")})");
#endif
        }

        private void Fire()
        {
            for(int i = 0; i < _chargeFXInstances.Length; i++)
            {
                if (_chargeFXInstances[i])
                {
                    Destroy(_chargeFXInstances[i]);
                }
            }

            //Spawn fireball fire vfx
            //Spawn triple fireball from each muzzle

            var ray = GetAimRay();
            FireProjectileInfo baseInfo = new FireProjectileInfo
            {
                crit = RollCrit(),
                damage = _damagePerFireball,
                owner = gameObject,
                position = _muzzleProjectileTransform.position,
                projectilePrefab = projectilePrefab,
            };

            var worldBaseAimRotation = Util.QuaternionSafeLookRotation(ray.direction, transform.up);
            for (int i = 0; i < 3; i++)
            {
                var info = baseInfo;
                var rot = worldBaseAimRotation * _fireSpread[i];
                info.rotation = rot;
                ProjectileManager.instance.FireProjectile(info);

            }
        }

        public override void OnExit()
        {
            base.OnExit();
            for (int i = 0; i < _chargeFXInstances.Length; i++)
            {
                if (_chargeFXInstances[i])
                {
                    Destroy(_chargeFXInstances[i]);
                }
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            _fireTimer += Time.fixedDeltaTime;

            if (_fireTimer >= _durationUntilFiring && !_hasFiredThisCycle)
            {
                _hasFiredThisCycle = true;
                Fire();
            }
            if (_fireTimer >= _duration)
            {
                if(!skillButtonState.down && isAuthority)
                {
                    outer.SetNextState(new FireballsStop());
                    return;
                }
                StartCycle();
            }
        }
    }
}