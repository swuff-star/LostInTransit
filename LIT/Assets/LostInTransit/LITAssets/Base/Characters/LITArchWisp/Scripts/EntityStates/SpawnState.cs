using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace EntityStates.LITArchWisp
{
    public class SpawnState : EntityState
    {
        public static float duration = 2;
        public static GameObject spawnEffect;

        public override void OnEnter()
        {
            base.OnEnter();
            PlayAnimation("Body", "Spawn", "Spawn.playbackRate", duration);
            EffectManager.SimpleEffect(spawnEffect, transform.position, Quaternion.identity, true);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if(fixedAge >= duration)
            {
                outer.SetNextStateToMain();
            }
        }
    }
}