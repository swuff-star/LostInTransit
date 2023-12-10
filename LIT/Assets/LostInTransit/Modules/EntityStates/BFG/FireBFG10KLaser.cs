using RoR2;
using RoR2.Audio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace EntityStates.BFG
{
    public class FireBFG10KLaser : BaseBFG10KState
    {
        public static float duration;
        [HideInInspector]
        public static LoopSoundDef loopSoundDef = Addressables.LoadAssetAsync<LoopSoundDef>("RoR2/DLC1/MajorAndMinorConstruct/lsdMajorConstructLaser.asset").WaitForCompletion();
        public static GameObject hitEffectPrefab;
        public static GameObject laserPrefab;
        public static float damageCoefficient;
        public static float procCoefficient;
        public static float force;
        public static float minSpread;
        public static float maxSpread;
        public static uint bulletCount;
        public static float fireFrequency;
        public static float maxDistance;

        private LoopSoundManager.SoundLoopPtr loopPtr;
        private Ray ray;
        private float fireTimer = 0f;
        private GameObject laserEffectInstance;
        private Transform laserEffectInstanceEndTransform;

        public override void OnEnter()
        {
            base.OnEnter();
            if(loopSoundDef)
            {
                loopPtr = LoopSoundManager.PlaySoundLoopLocal(gameObject, loopSoundDef);
            }
            if(BFGMuzzle)
            {
                laserEffectInstance = UnityEngine.Object.Instantiate(laserPrefab, BFGMuzzle.transform.position, BFGMuzzle.transform.rotation);
                laserEffectInstance.transform.parent = BFGMuzzle;
                laserEffectInstanceEndTransform = laserEffectInstance.GetComponent<ChildLocator>().FindChild("LaserEnd");
            }
        }

        public override void OnExit()
        {
            if(laserEffectInstance)
            {
                Destroy(laserEffectInstance);
            }
            LoopSoundManager.StopSoundLoopLocal(loopPtr);
            base.OnExit();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            ray = GetRay();
            fireTimer += Time.fixedDeltaTime;
            float frequency = fireFrequency * attachedBody.attackSpeed;
            float num = 1f / frequency;

            if(fireTimer > num)
            {
                FireBullet(BFGDisplay.transform, ray, "Muzzle");
                fireTimer = 0f;
            }
            if(laserEffectInstance && laserEffectInstanceEndTransform)
            {
                laserEffectInstanceEndTransform.position = GetBulletEndPoint();
            }
            if(isAuthority && fixedAge > duration)
            {
                outer.SetNextState(new TerminateBFG10KLaser(GetBulletEndPoint()));
            }
        }

        private void FireBullet(Transform bfgTransform, Ray ray, string muzzle)
        {
            if(isAuthority)
            {
                BulletAttack attack = new BulletAttack();
                attack.owner = attachedBody.gameObject;
                attack.weapon = bfgTransform.gameObject;
                attack.origin = ray.origin;
                attack.aimVector = ray.direction;
                attack.minSpread = minSpread;
                attack.maxSpread = maxSpread;
                attack.bulletCount = bulletCount;
                attack.damage = (damageCoefficient * attachedBody.damage) / fireFrequency;
                attack.procCoefficient = procCoefficient / fireFrequency;
                attack.force = force;
                attack.muzzleName = muzzle;
                attack.hitEffectPrefab = hitEffectPrefab;
                attack.isCrit = attachedBody.RollCrit();
                attack.HitEffectNormal = false;
                attack.radius = 0f;
                attack.maxDistance = maxDistance;
                attack.Fire();
            }
        }

        private Ray GetRay()
        {
            var origin = BFGMuzzle ? BFGMuzzle.position : attachedBody.aimOrigin;
            return BodyInputBank ? BodyInputBank.GetAimRay() : new Ray(origin, attachedBody.transform.forward);
        }

        private Vector3 GetBulletEndPoint()
        {
            Vector3 point = ray.GetPoint(maxDistance);
            if (Util.CharacterRaycast(attachedBody.gameObject, ray, out var hitInfo, maxDistance, LayerIndex.CommonMasks.bullet, QueryTriggerInteraction.UseGlobal))
            {
                point = hitInfo.point;
            }
            return point;
        }
    }
}
