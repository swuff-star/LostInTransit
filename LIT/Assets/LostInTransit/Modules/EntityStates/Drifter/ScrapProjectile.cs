using LostInTransit.Components;
using RoR2;
using RoR2.Projectile;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EntityStates.Drifter
{
    public class ScrapProjectile : BaseSkillState
    {
        public static float baseDuration;
        private float duration;

        public static GameObject projectilePrefab;
        public static float damageCoef;
        public static float procCoef;
        public static float recoil;
        public static float projSpeed;

        private bool hasFired;

        public static float scrapCost;

        private DrifterScrapComponent dsc;

        public override void OnEnter()
        {
            base.OnEnter();

            dsc = characterBody.GetComponent<DrifterScrapComponent>();

            if (dsc == null)
            {
                Debug.Log("dsc is null!");
                outer.SetNextStateToMain();
                return;
            }

            hasFired = false;
            duration = baseDuration / attackSpeedStat;
            characterBody.SetAimTimer(duration * 1.5f);

            //Util.PlayAttackSpeedSound();
        }

        public virtual void FireProjectile()
        {
            if (!hasFired)
            {
                hasFired = true;

                float damage = damageCoef * damageStat;
                AddRecoil(-2f * recoil, -3f * recoil, -1f * recoil, 1f * recoil);
                characterBody.AddSpreadBloom(0.33f * recoil);
                Ray aimRay = GetAimRay();

                Vector3 directionA = CalculateDirection(aimRay.direction, 0.4f);
                FireProjectile(aimRay.origin, directionA, damage);

                Vector3 directionB = CalculateDirection(aimRay.direction, -0.4f);
                FireProjectile(aimRay.origin, directionB, damage);

                Vector3 directionC = CalculateDirection(aimRay.direction, 0.8f);
                FireProjectile(aimRay.origin, directionC, damage);

                Vector3 directionD = CalculateDirection(aimRay.direction, -0.8f);
                FireProjectile(aimRay.origin, directionD, damage);

                dsc.AddScrap(scrapCost);
            }
        }

        private Vector3 CalculateDirection(Vector3 aimDirection, float angleOffsetX)
        {
            Quaternion rotation = Quaternion.Euler(angleOffsetX, 0f, 0f);
            return rotation * aimDirection;
        }

        private void FireProjectile(Vector3 origin, Vector3 direction, float damage)
        {
            ProjectileManager.instance.FireProjectile(
                projectilePrefab,
                origin,
                Util.QuaternionSafeLookRotation(direction),
                gameObject,
                damage,
                0f,
                RollCrit(),
                DamageColorIndex.Default,
                null,
                projSpeed);
        }
    }
}
