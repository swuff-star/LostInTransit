using MSU;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace EntityStates.AttackDrone
{
    public class AttackDroneFire : BaseSkillState
    {
        public static float damageCoefficient;
        public static float procCoefficient;
        public static float baseVolley;
        public static float baseFireInterval;
        public static float force;

        [HideInInspector]
        public static GameObject muzzleEffectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/VFX/Muzzleflash1.prefab").WaitForCompletion();
        [HideInInspector]
        public static GameObject tracerPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/VFX/TracerNoSmoke.prefab").WaitForCompletion();
        [HideInInspector]
        public static GameObject hitPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/VFX/Hitspark1.prefab").WaitForCompletion();

        private string muzzleString;
        private string muzzleString2;
        private string trueMuzzleString;
        private float fireInterval;
        private float fireTimer;
        private float volley;
        private float shotCount;
        private bool muzzle;

        public override void OnEnter()
        {
            base.OnEnter();
            volley = baseVolley * attackSpeedStat;
            shotCount = 0;
            fireInterval = baseFireInterval / attackSpeedStat;
            fireTimer = 0;
            characterBody.SetAimTimer(2f);
            muzzleString = "Muzzle";
            muzzleString2 = "Muzzle2";
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            fireTimer += Time.fixedDeltaTime;

            if (fireTimer >= fireInterval)
            {
                fireTimer = 0;
                Shoot();
                shotCount++;
            }

            if (shotCount < volley || !isAuthority)
                return;

            outer.SetNextStateToMain();
        }

        private void Shoot()
        {
            bool isCrit = RollCrit();

            if (muzzle)
                trueMuzzleString = muzzleString;
            else
                trueMuzzleString = muzzleString2;

            muzzle = !muzzle;

            string soundString = "Play_drone_attack";
            Util.PlaySound(soundString, gameObject);

            if (muzzleEffectPrefab)
                EffectManager.SimpleMuzzleFlash(muzzleEffectPrefab, gameObject, trueMuzzleString, true);

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
                    muzzleName = trueMuzzleString,
                    stopperMask = LayerIndex.world.mask,
                    smartCollision = true,
                    procChainMask = default(ProcChainMask),
                    procCoefficient = procCoefficient,
                    radius = 0.5f,
                    tracerEffectPrefab = tracerPrefab,
                    hitEffectPrefab = hitPrefab
                };
                bullet.Fire();
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}