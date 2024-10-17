using EntityStates;
using RoR2;
using UnityEngine;
using RoR2.Skills;
using UnityEngine.Networking;

namespace EntityStates.Pilot
{
    public class DroneFlightState : GenericCharacterMain
    {
        public static float maxFlightTime = 5f;
        public static float boostDecayCoefficient = 5f / 2f;
        public static float antiGravityStrength = 5f;
        public static float boostStrength = 5f;

        private float flightRemaining;
        private bool jumpInputHeld;
        public override void OnEnter()
        {
            base.OnEnter();
            this.flightRemaining = maxFlightTime;
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();          

            if(base.isAuthority)
            {
                float timeScale = 1f;
                float upForce = antiGravityStrength;
                jumpInputHeld = base.inputBank.jump.down;

                if(jumpInputHeld)
                {
                    upForce += boostStrength;
                    timeScale *= boostDecayCoefficient;
                }

                base.characterMotor.velocity.y += upForce * Time.fixedDeltaTime;

                this.flightRemaining -= Time.fixedDeltaTime * timeScale;

                if(base.inputBank.skill3.justPressed || this.flightRemaining <= 0f)
                {
                    this.outer.SetNextStateToMain();
                }
            }

        }
        public override void ProcessJump()
        {
            base.ProcessJump();
        }
    }
}
