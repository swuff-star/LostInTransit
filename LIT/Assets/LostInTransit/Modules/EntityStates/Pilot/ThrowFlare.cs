using EntityStates;
using RoR2;
using UnityEngine;
using RoR2.Skills;
using UnityEngine.Networking;

namespace EntityStates.Pilot
{
    public class ThrowFlare : GenericProjectileBaseState    
    {
        public override void PlayAnimation(float duration)
        {
            base.PlayAnimation("Gesture, Override", "ThrowFlare");
        }
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}
