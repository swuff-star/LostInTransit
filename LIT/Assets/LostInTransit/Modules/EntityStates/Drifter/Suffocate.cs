using LostInTransit.DamageTypes;
using R2API;
using RoR2;
using RoR2.Skills;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Drifter
{
    class Suffocate : BasicMeleeAttack
    {
        public override void PlayAnimation()
        {
            //PlayCrossfade("Gesture, Override", animationStateName, "Primary.playbackRate", duration, 0.1f);
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
        }

        public override void AuthorityModifyOverlapAttack(OverlapAttack overlapAttack)
        {
            //Debug.Log("modify overlap");
            base.AuthorityModifyOverlapAttack(overlapAttack);
            DamageAPI.AddModdedDamageType(overlapAttack, ExecuteToScrap.executeToScrap);
        }
    }
}
