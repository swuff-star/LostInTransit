using EntityStates;
using RoR2;
using UnityEngine;
using RoR2.Skills;
using UnityEngine.Networking;

namespace EntityStates.Pilot
{
    public class FirePistol : GenericBulletBaseState, SteppedSkillDef.IStepSetter
    {
        private bool isComboFinisher
        {
            get => step == 2;
        }
        private int step;
        public void SetStep(int i)
        {
            this.step = i;
        }

        [SerializeField]
        public string pierceSoundString;
        [SerializeField]
        public GameObject pierceMuzzleFlashPrefab;
        [SerializeField]
        public GameObject pierceTracerPrefab;

        public static float pierceDamageCoefficient = 1.8f;
        public override void PlayFireAnimation()
        {
            string animStateName = isComboFinisher ? "FirePistol2" : "FirePistol";
            base.PlayAnimation("Gesture, Override", animStateName);
            
        }
        public override void DoFireEffects()
        {
            Util.PlaySound(isComboFinisher ? this.pierceSoundString : this.fireSoundString, base.gameObject);
            if (this.muzzleTransform)
            {
                EffectManager.SimpleMuzzleFlash(isComboFinisher ? this.pierceMuzzleFlashPrefab : this.muzzleFlashPrefab, base.gameObject, this.muzzleName, false);
            }
        }
        public override void ModifyBullet(BulletAttack bulletAttack)
        {
            if(isComboFinisher)
            {
                bulletAttack.tracerEffectPrefab = pierceTracerPrefab;
                bulletAttack.stopperMask = LayerIndex.world.mask;
                bulletAttack.damage = this.damageStat * pierceDamageCoefficient;
            }
        }



    }
}
