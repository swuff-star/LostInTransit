using RoR2;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EntityStates.Robomando.RobomandoWeapon
{
    public class RobomandoPistol : BaseSkillState
    {
        public static float dmgCoef;
        public static float force = Commando.CommandoWeapon.FirePistol2.force;
        public static float baseDur = Commando.CommandoWeapon.FirePistol2.baseDuration;
        public static string fireSoundString = Commando.CommandoWeapon.FirePistol2.firePistolSoundString;
        public static float recoilAmplitude = Commando.CommandoWeapon.FirePistol2.recoilAmplitude;
        public static float spreadBloomValue = Commando.CommandoWeapon.FirePistol2.spreadBloomValue;
        public static string muzzleString;

        private Ray aimRay;
        private float dur;

        //public static GameObject muzzleEffectPrefab = Commando.CommandoWeapon.FirePistol2.muzzleEffectPrefab;
        //public static GameObject hitEffectPrefab = Commando.CommandoWeapon.FirePistol2.hitEffectPrefab;
        //public static GameObject tracerEffectPrefab = Commando.CommandoWeapon.FirePistol2.tracerEffectPrefab;

        public override void OnEnter()
        {
            base.OnEnter();
            dur = baseDur / attackSpeedStat;
            aimRay = GetAimRay();
            StartAimMode(aimRay, 3f, false);
            //PlayAnimation
            FireBullet();
        }

        private void FireBullet()
        {
            Util.PlaySound(fireSoundString, gameObject);
            if (muzzleString != null)
            {
                EffectManager.SimpleMuzzleFlash(Commando.CommandoWeapon.FirePistol2.muzzleEffectPrefab, gameObject, muzzleString, false);
            }
            AddRecoil(-0.4f * recoilAmplitude, -0.8f * recoilAmplitude, -0.3f * recoilAmplitude, 0.3f * recoilAmplitude);
            if (isAuthority)
            {
                new BulletAttack
                {
                    owner = gameObject,
                    weapon = gameObject,
                    origin = aimRay.origin,
                    aimVector = aimRay.direction,
                    minSpread = 0f,
                    maxSpread = characterBody.spreadBloomAngle,
                    damage = dmgCoef * damageStat,
                    force = force,
                    tracerEffectPrefab = Commando.CommandoWeapon.FirePistol2.tracerEffectPrefab,
                    muzzleName = muzzleString,
                    hitEffectPrefab = Commando.CommandoWeapon.FirePistol2.hitEffectPrefab,
                    isCrit = Util.CheckRoll(critStat, characterBody.master),
                    radius = 0.1f,
                    maxDistance = 128f,
                    smartCollision = true
                }.Fire();
            }
            characterBody.AddSpreadBloom(spreadBloomValue);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (fixedAge < dur || !isAuthority)
            {
                return;
            }
            outer.SetNextStateToMain();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}
