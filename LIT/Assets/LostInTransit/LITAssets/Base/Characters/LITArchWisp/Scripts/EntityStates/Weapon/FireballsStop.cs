using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityStates.LITArchWisp.Weapon
{
    public class FireballsStop : FireballsBaseState
    {
        public static float baseDuration;
        private float _duration;

        public override void OnExit()
        {
            base.OnExit();
            _duration = baseDuration / attackSpeedStat;
            PlayCrossfade("Body", "FireballsStop", "FireballsStop.playbackRate", _duration, 0.25f);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if(fixedAge >= _duration && isAuthority)
            {
                outer.SetNextStateToMain();
            }
        }
    }
}