using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace EntityStates.BFG
{
    public class TerminateBFG10KLaser : BaseBFG10KState
    {
        public static float duration;
        public static GameObject muzzleEffectPrefab;
        public static GameObject explosionEffectPrefab;
        public static float blastDamageCoefficient;
        public static float blastForce;
        public static float radius;
        public static Vector3 blastBonusForce;
        public static string enterSoundString;

        private Vector3 blastPosition;
        public TerminateBFG10KLaser() { }
        public TerminateBFG10KLaser(Vector3 pos)
        {
            blastPosition = pos;
        }

        public override void OnEnter()
        {
            base.OnEnter();
            if(BFGAnimator)
            {
                PlayAnimationOnAnimator(BFGAnimator, "Base", fireAnim);
            }
            if(muzzleEffectPrefab)
            {
                EffectManager.SimpleMuzzleFlash(muzzleEffectPrefab, BFGDisplay, "Muzzle", false);
            }
            Util.PlaySound(enterSoundString, gameObject);
            if(isAuthority)
            {
                BlastAttack blastAttack = new BlastAttack();
                blastAttack.attacker = attachedBody.gameObject;
                blastAttack.inflictor = BFGDisplay;
                blastAttack.teamIndex = attachedBody.teamComponent.teamIndex;
                blastAttack.baseDamage = blastDamageCoefficient * blastDamageCoefficient;
                blastAttack.baseForce = blastForce;
                blastAttack.position = blastPosition;
                blastAttack.radius = radius;
                blastAttack.bonusForce = blastBonusForce;
                blastAttack.Fire();
                if (explosionEffectPrefab)
                    EffectManager.SpawnEffect(explosionEffectPrefab, new EffectData
                    {
                        origin = blastPosition,
                        scale = radius
                    }, true);
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (fixedAge >= duration)
                Destroy(gameObject);
        }
    }
}
