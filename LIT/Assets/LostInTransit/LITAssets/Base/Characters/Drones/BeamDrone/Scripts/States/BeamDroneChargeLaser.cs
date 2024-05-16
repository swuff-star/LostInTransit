using MSU;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace EntityStates.BeamDrone
{
    public class BeamDroneChargeLaser : BaseState
    {
        public static float duration;
        public static string muzzleName;
        public static GameObject muzzleEffectPrefab;
        public static string enterSoundString;

        private float originalMoveSpeed;

        public override void OnEnter()
        {
            if (muzzleEffectPrefab)
            {
                EffectManager.SimpleMuzzleFlash(muzzleEffectPrefab, gameObject, muzzleName, false);
            }
            originalMoveSpeed = characterBody.moveSpeed;
        }

        public override void FixedUpdate()
        {
            characterBody.moveSpeed = originalMoveSpeed * 0.1f;
            if (fixedAge >= duration && isAuthority)
            {
                outer.SetNextState(new BeamDroneFireLaser());
            }
            base.FixedUpdate();
        }

        public override void OnExit()
        {
            base.OnExit();

            characterBody.moveSpeed = originalMoveSpeed;
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Pain;
        }

    }
}