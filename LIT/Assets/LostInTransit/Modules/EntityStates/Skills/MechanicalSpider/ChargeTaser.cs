using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RoR2;
using RoR2.Projectile;
using EntityStates;
using UnityEngine.AddressableAssets;

namespace LostInTransit.LITEntityStates.MechanicalSpider
{
    public class ChargeTaser : BaseState
    {
        public static float baseDuration = 1f;
        public static GameObject chargeVfxPrefab;
        public static string attackString;

        private float duration;

        private GameObject chargeVfxInstance;

        public override void OnEnter()
        {
            base.OnEnter();
            duration = baseDuration / attackSpeedStat;
            GetModelAnimator();
            Transform modelTransform = GetModelTransform();
            Util.PlayAttackSpeedSound(attackString, gameObject, attackSpeedStat);
            if (modelTransform)
            {
                ChildLocator childLocator = modelTransform.GetComponent<ChildLocator>();
                if (childLocator)
                {
                    Transform transform = childLocator.FindChild("TaserMuzzle");
                    if (transform && chargeVfxPrefab)
                    {
                        chargeVfxInstance = Object.Instantiate(chargeVfxPrefab, transform.position, transform.rotation);
                        chargeVfxInstance.transform.parent = transform;
                    }
                }
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            if (chargeVfxInstance)
            {
                Destroy(chargeVfxInstance);
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (fixedAge >= duration && isAuthority)
            {
                FireTaser nextState = new FireTaser();
                outer.SetNextState(nextState);
                return;
            }    
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}
