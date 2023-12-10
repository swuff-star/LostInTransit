using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace EntityStates.BFG
{
    public class ChargeBFG10KLaser : BaseBFG10KState
    {
        public static float duration;
        public static string enterSoundString;
        public static GameObject muzzleEffectPrefab;

        public override void OnEnter()
        {
            base.OnEnter();
            if(BFGDisplay)
            {
                PlayAnimationOnAnimator(BFGAnimator, "Base", chargeAnim);

                if(muzzleEffectPrefab && BFGDisplay)
                {
                    EffectManager.SimpleMuzzleFlash(muzzleEffectPrefab, BFGDisplay, "Muzzle", false);
                }
            }
            Util.PlaySound(enterSoundString, gameObject);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if(fixedAge >= duration && isAuthority)
            {
                outer.SetNextState(new FireBFG10KLaser());
                if(BFGAnimator)
                {
                    BFGAnimator.StopPlayback();
                }
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Pain;
        }
    }
}
