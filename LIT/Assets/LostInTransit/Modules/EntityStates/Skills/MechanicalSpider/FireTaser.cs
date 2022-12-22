using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RoR2;
using RoR2.Projectile;
using EntityStates;

namespace LostInTransit.LITEntityStates.MechanicalSpider
{
    public class FireTaser : BaseState
    {
        public static GameObject projectilePrefab;
        public static GameObject chargeEffectPrefab;
        public static GameObject muzzleflashEffectPrefab;
        public static float damageCoefficient = 1.2f;
        public static float force = 20f;

        public static string enterSoundString;
        public static string attackString;

        public static string targetMuzzle;

        private bool hasFired;

        private float duration;
        private float delay;
        private static float baseDelay = 0.5f;
        private static float baseDuration = 2f;

        public override void OnEnter()
        {
            base.OnEnter();
            duration = baseDuration / attackSpeedStat;
            delay = baseDelay / attackSpeedStat;
            StartAimMode(duration + 2f, false);

            if (chargeEffectPrefab)
            {
                EffectManager.SimpleMuzzleFlash(chargeEffectPrefab, gameObject, "TaserMuzzle", false);
            }

            Util.PlayAttackSpeedSound(enterSoundString, gameObject, attackSpeedStat);
        }

        private void Fire()
        {
            hasFired = true;
            Util.PlaySound(attackString, gameObject);
            Ray aimRay = GetAimRay();
            if (muzzleflashEffectPrefab)
            {
                EffectManager.SimpleMuzzleFlash(muzzleflashEffectPrefab, gameObject, "TaserMuzzle", false);
            }
            if (isAuthority)
            {
                FireProjectileInfo fireProjectileInfo = default(FireProjectileInfo);
                fireProjectileInfo.projectilePrefab = projectilePrefab;
                fireProjectileInfo.position = aimRay.origin;
                fireProjectileInfo.rotation = Util.QuaternionSafeLookRotation(aimRay.direction);
                fireProjectileInfo.owner = gameObject;
                fireProjectileInfo.damage = damageStat * damageCoefficient;
                fireProjectileInfo.force = force;
                fireProjectileInfo.crit = RollCrit();
            }
        }
        public override void OnExit()
        {
            base.OnExit();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (fixedAge >= delay && !hasFired)
            {
                Fire();
            }
            if (fixedAge >= duration && isAuthority)
            {
                outer.SetNextStateToMain();
                return;
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Any;
        }
    }
}
