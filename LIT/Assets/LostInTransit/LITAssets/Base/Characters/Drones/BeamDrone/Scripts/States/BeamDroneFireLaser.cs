using EntityStates.EngiTurret.EngiTurretWeapon;
using MSU;
using RoR2;
using RoR2.Audio;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace EntityStates.BeamDrone
{
    public class BeamDroneFireLaser : FireBeam
    {
        public static float duration;
        public static float aimMaxSpeed;
        [HideInInspector]
        public static LoopSoundDef loopSoundDef = Addressables.LoadAssetAsync<LoopSoundDef>("RoR2/DLC1/MajorAndMinorConstruct/lsdMajorConstructLaser.asset").WaitForCompletion();
        private static LoopSoundManager.SoundLoopPtr loopPtr;
        private static AimAnimator.DirectionOverrideRequest animatorDirectionOverrideRequest;
        private static Vector3 aimDirection;

        private float originalMoveSpeed;
        private RigidbodyDirection rbd;

        public override void OnEnter()
        {
            base.OnEnter();

            string soundString = "Play_railgunner_R_fire";
            Util.PlaySound(soundString, gameObject);

            if (loopSoundDef)
                loopPtr = LoopSoundManager.PlaySoundLoopLocal(gameObject, loopSoundDef);

            rbd = GetComponent<RigidbodyDirection>();
            if (rbd != null)
            {
                rbd.enabled = false;
            }

            if (hitEffectPrefab != null)
            {
                hitEffectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/VFX/Hitspark1.prefab").WaitForCompletion();
            }

            AimAnimator aa = GetComponent<AimAnimator>();
            if (aa)
            {
                animatorDirectionOverrideRequest = aa.RequestDirectionOverride(new Func<Vector3>(GetAimDirection));
            }
            aimDirection = GetTargetDirection();

            originalMoveSpeed = characterBody.moveSpeed;
        }

        public override void OnExit()
        {
            AimAnimator.DirectionOverrideRequest directionOverrideRequest = animatorDirectionOverrideRequest;
            if (directionOverrideRequest != null)
            {
                directionOverrideRequest.Dispose();
            }
            rbd.enabled = true;
            LoopSoundManager.StopSoundLoopLocal(loopPtr);
            characterBody.moveSpeed = originalMoveSpeed;
            base.OnExit();
        }

        public override void FixedUpdate()
        {
            characterBody.moveSpeed = originalMoveSpeed * 0.1f;
            aimDirection = Vector3.RotateTowards(aimDirection, GetTargetDirection(), aimMaxSpeed * 0.018f * Time.fixedDeltaTime, float.PositiveInfinity);
            base.FixedUpdate();
        }

        public override EntityState GetNextState()
        {
            return new BeamDroneTerminateLaser();
        }

        public override void ModifyBullet(BulletAttack bulletAttack)
        {
            bulletAttack.radius = 2f;
        }

        public override bool ShouldFireLaser()
        {
            return duration > fixedAge;
        }

        private Vector3 GetAimDirection()
        {
            return aimDirection;
        }

        private Vector3 GetTargetDirection()
        {
            if (inputBank)
            {
                return inputBank.aimDirection;
            }
            return transform.forward;
        }

        public override Ray GetLaserRay()
        {
            if (inputBank)
            {
                return new Ray(inputBank.aimOrigin, aimDirection);
            }
            return new Ray(transform.position, aimDirection);
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Pain;
        }
    }
}