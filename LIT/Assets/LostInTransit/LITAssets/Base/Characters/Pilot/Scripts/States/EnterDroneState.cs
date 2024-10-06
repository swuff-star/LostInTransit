using EntityStates;
using RoR2;
using UnityEngine;
using RoR2.Skills;
using UnityEngine.Networking;

namespace EntityStates.Pilot
{
    public class EnterDroneState : BaseSkillState
    {
        public static float verticalBoost = 2f;
        public static float horizontalBoost = 2f;
        public static float baseDuration = 0.75f;

        public override void OnEnter()
        {
            base.OnEnter();

            base.characterMotor.Jump(horizontalBoost, verticalBoost);
            base.characterMotor.jumpCount++;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if(base.isAuthority && base.fixedAge > baseDuration)
            {
                this.outer.SetNextState(new DroneFlightState());
            }
        }
    }
}
