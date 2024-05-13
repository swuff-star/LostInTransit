using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.LITArchWisp
{
    /*
     * TODO:
     * - Implement Explosion VFX
     * - Implement mask cracking vfx
     */
    public class DeathState : GenericCharacterDeath
    {
        public static GameObject deathEffect;
        public static float duration;

        private float _stopwatch;
        private Transform _modelBaseTransform;
        private bool _attemptedDeathBehaviour;

        public override void OnEnter()
        {
            base.OnEnter();
            if (!modelLocator)
                return;

            _modelBaseTransform = modelLocator.modelBaseTransform;
        }

        private void AttemptDeathBehaviour()
        {
            if (!_attemptedDeathBehaviour)
                _attemptedDeathBehaviour = true;

            if(deathEffect && NetworkServer.active)
            {
                EffectManager.SpawnEffect(deathEffect, new EffectData
                {
                    origin = transform.position,
                }, true);
                if(_modelBaseTransform)
                {
                    Destroy(_modelBaseTransform.gameObject);
                    _modelBaseTransform = null;
                }
                if(NetworkServer.active)
                {
                    Destroy(gameObject);
                }
            }
        }

        public override void FixedUpdate()
        {
            _stopwatch += Time.fixedDeltaTime;
            if(_stopwatch >= duration)
            {
                AttemptDeathBehaviour();
            }
        }

        public override void OnExit()
        {
            if(!outer.destroying)
            {
                AttemptDeathBehaviour();
            }
            base.OnExit();
        }
    }
}
