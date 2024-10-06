using EntityStates;
using RoR2;
using UnityEngine;
using RoR2.Skills;
using UnityEngine.Networking;

namespace EntityStates.Pilot
{
    public class FireSMG : GenericBulletBaseState
    {
        public override void PlayFireAnimation()
        {
            base.PlayAnimation("Gesture, Override", "FirePistol");

        }
    }
}
