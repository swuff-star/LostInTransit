using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RoR2;
using RoR2.Projectile;
using EntityStates;
using UnityEngine.AddressableAssets;

namespace LostInTransit.LITEntityStates.MechanicalSpider
{
    public class FireTaser : BaseState
    {
        [HideInInspector]
        //public static GameObject projectilePrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Captain/CaptainTazer.prefab").WaitForCompletion();
        public static GameObject effectPrefab;

        public static float baseDuration = 2f;
        public static float damageCoefficient = 1.2f;
        public static float force = 20f;

        public static string attackString;

        private float duration;

        public override void OnEnter()
        {
            base.OnEnter();
            Debug.Log("entered");
            duration = baseDuration / attackSpeedStat;
            Debug.Log("set duration");
            //playanimation
            Util.PlaySound(attackString, gameObject);
            Debug.Log("played sound");
            Ray faggot = GetAimRay();
            Debug.Log("got aimray");
            string muzzleName = "TaserMuzzle";
            Debug.Log("set muzzle");
            if (effectPrefab)
            {
                Debug.Log("checked effect");
                EffectManager.SimpleMuzzleFlash(EntityStates.Captain.Weapon.FireTazer.muzzleflashEffectPrefab, gameObject, muzzleName, false);
                Debug.Log("played muzzleflash");
            }
            if (isAuthority)
            {
                FireProjectileInfo fireProjectileInfo = default(FireProjectileInfo);
                fireProjectileInfo.projectilePrefab = EntityStates.Captain.Weapon.FireTazer.projectilePrefab;
                fireProjectileInfo.position = faggot.origin;
                fireProjectileInfo.rotation = Util.QuaternionSafeLookRotation(faggot.direction);
                fireProjectileInfo.owner = gameObject;
                fireProjectileInfo.damage = damageStat * damageCoefficient;
                fireProjectileInfo.force = force;
                fireProjectileInfo.crit = Util.CheckRoll(critStat, characterBody.master);
                Debug.Log("setup projectile");
                ProjectileManager.instance.FireProjectile(fireProjectileInfo);
                Debug.Log("fired projectile");
            }
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        public override void FixedUpdate()
        {
            FixedUpdate();
            if (fixedAge >= duration && isAuthority)
            {
                Debug.Log("Setting next state to main");
                outer.SetNextStateToMain();
                Debug.Log("State set to main");
                return;
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}
