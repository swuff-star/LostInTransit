using MSU;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace EntityStates.BeamDrone
{
    public class GoldBeamDroneTerminateLaser : BaseState
    {
        public static float duration;
        public static string muzzleName;
        public static GameObject muzzleEffectPrefab;
        public static string enterSoundString;

        public GoldBeamDroneTerminateLaser()
        {
        }

        public override void OnEnter()
        {
            if (muzzleEffectPrefab)
            {
                EffectManager.SimpleMuzzleFlash(muzzleEffectPrefab, gameObject, muzzleName, false);
            }
        }

        public override void FixedUpdate()
        {
            if (fixedAge >= duration && isAuthority)
            {
                outer.SetNextStateToMain();
            }
            base.FixedUpdate();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }

    }
}