using MSU;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace EntityStates.LaserDrone
{
    public class LaserDroneFire : BaseSkillState
    {
        public static float damageCoefficient;
        public static float procCoefficient;
        public static float baseDuration;
        public static float force;

        [HideInInspector]
        public static GameObject muzzleEffectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Golem/MuzzleflashGolem.prefab").WaitForCompletion();
        [HideInInspector]
        public static GameObject tracerPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Golem/TracerGolem.prefab").WaitForCompletion();
        [HideInInspector]
        public static GameObject hitPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Golem/ExplosionGolem.prefab").WaitForCompletion();

        private float duration;
        private string muzzleString;
        private bool hasFired;

        public override void OnEnter()
        {
            base.OnEnter();
            duration = baseDuration / attackSpeedStat;
            characterBody.SetAimTimer(2f);
            muzzleString = "Muzzle";
            hasFired = false;
            Shoot();
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (fixedAge < duration || !isAuthority)
                return;
            outer.SetNextStateToMain();
        }

        private void Shoot()
        {
            if (!hasFired)
            {
                hasFired = true;
                bool isCrit = RollCrit();

                string soundString = "Play_railgunner_R_fire";
                Util.PlaySound(soundString, gameObject);

                if (muzzleEffectPrefab)
                    EffectManager.SimpleMuzzleFlash(muzzleEffectPrefab, gameObject, muzzleString, true);

                if (isAuthority)
                {
                    Ray r = GetAimRay();
                    BulletAttack bullet = new BulletAttack
                    {
                        aimVector = r.direction,
                        origin = r.origin,
                        damage = damageCoefficient * damageStat,
                        damageType = DamageType.Stun1s,
                        damageColorIndex = DamageColorIndex.Default,
                        minSpread = 0f,
                        maxSpread = 0f,
                        falloffModel = BulletAttack.FalloffModel.None,
                        force = force,
                        isCrit = isCrit,
                        owner = gameObject,
                        muzzleName = muzzleString,
                        stopperMask = LayerIndex.world.mask,
                        smartCollision = true,
                        procChainMask = default(ProcChainMask),
                        procCoefficient = procCoefficient,
                        radius = 2f,
                        tracerEffectPrefab = tracerPrefab,
                        hitEffectPrefab = hitPrefab
                    };
                    bullet.Fire();
                }
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}