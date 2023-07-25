using LostInTransit.Components;
using RoR2;
using RoR2.Projectile;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//this is kinda coded like shit
//should probably refactor charging / toss to be separate states.
//i think the way this is done isn't really going to be viable when it comes to animating and polish.
//(or, at least, not ideal)
//problem for me then! just need to see how it feels!
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
        public static float projSpeedFast;

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

        public virtual void ThrowScrap()
        {
            if (!hasFired)
            {
                float damage = damageCoef * damageStat;
                AddRecoil(-2f * recoil, -3f * recoil, -1f * recoil, 1f * recoil);
                characterBody.AddSpreadBloom(0.33f * recoil);
                Ray aimRay = GetAimRay();

                Vector3 directionA = CalculateDirection(aimRay.direction, (fixedAge < duration) ? 0f : 0f);
                FireProjectile(aimRay.origin, directionA, damage);

                Vector3 directionB = CalculateDirection(aimRay.direction, (fixedAge < duration) ? -2.5f : -0.25f);
                FireProjectile(aimRay.origin, directionB, damage);

                Vector3 directionC = CalculateDirection(aimRay.direction, (fixedAge < duration) ? 2.5f : 0.25f);
                FireProjectile(aimRay.origin, directionC, damage);

                Vector3 directionD = CalculateDirection(aimRay.direction, (fixedAge < duration) ? -5f : -0.5f);
                FireProjectile(aimRay.origin, directionD, damage);

                Vector3 directionE = CalculateDirection(aimRay.direction, (fixedAge < duration) ? 5f : 0.5f);
                FireProjectile(aimRay.origin, directionE, damage);

                dsc.AddScrap(scrapCost);

                hasFired = true;
            }
        }

        private Vector3 CalculateDirection(Vector3 aimDirection, float angleOffsetY)
        {
            Quaternion rotation = Quaternion.Euler(0f, angleOffsetY, 0f);
            return rotation * aimDirection;
        }

        private void FireProjectile(Vector3 origin, Vector3 direction, float damage)
        {
            float speed = (fixedAge >= duration) ? projSpeedFast : projSpeed;

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
                speed);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (fixedAge >= duration * 0.7f && !hasFired && !inputBank.skill2.down)
                ThrowScrap();

            if (fixedAge >= duration && hasFired)
                outer.SetNextStateToMain();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}
