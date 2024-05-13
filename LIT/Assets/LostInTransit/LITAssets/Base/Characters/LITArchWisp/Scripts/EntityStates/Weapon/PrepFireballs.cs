using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityStates.LITArchWisp.Weapon
{
    public class PrepFireballs : FireballsBaseState
    {
        public static float baseDuration;
        private float _duration;

        public override void OnEnter()
        {
            base.OnEnter();
            _duration = baseDuration / attackSpeedStat;
            PlayCrossfade("Body", "PrepFireballs", "PrepFireballs.playbackRate", _duration, 0.25f);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if(fixedAge >= _duration && isAuthority)
            {
                outer.SetNextState(new FireFireballs());
            }
        }
    }
}