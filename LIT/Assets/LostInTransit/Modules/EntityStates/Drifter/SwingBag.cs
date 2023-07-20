using LostInTransit.DamageTypes;
using R2API;
using RoR2;
using RoR2.Skills;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Drifter
{
    class SwingBag : BasicMeleeAttack, SteppedSkillDef.IStepSetter
    {
        public int swing = 0;

        public static float baseDurationBeforeInterruptable;

        public static float comboFinisherBaseDuration;
        public static float comboFinisherHitPauseDuration;
        public static float comboFinisherDamageCoefficient;
        //public static float comboFinisherBloom;
        public static float comboFinisherBaseDurationBeforeInterruptable;

        private float durationBeforeInterruptable;

        public override void OnEnter()
        {
            Debug.Log("on enter");
            if (isComboFinisher)
            {
                hitBoxGroupName = "SwingLarge";
                hitPauseDuration = comboFinisherHitPauseDuration;
                damageCoefficient = comboFinisherDamageCoefficient;
                baseDuration = comboFinisherBaseDuration;
            }
            durationBeforeInterruptable = (isComboFinisher ? (comboFinisherBaseDurationBeforeInterruptable / attackSpeedStat) : (baseDurationBeforeInterruptable / attackSpeedStat));

            base.OnEnter();
        }

        private bool isComboFinisher
        {
            get
            {
                return swing == 2;
            }
        }

        public override void PlayAnimation()
        {
            Debug.Log("play anim");
            string animationStateName = "Swing" + swing;
            //PlayCrossfade("Gesture, Override", animationStateName, "Primary.playbackRate", duration, 0.1f);
        }

        void SteppedSkillDef.IStepSetter.SetStep(int i)
        {
            swing = i;
            //swingEffectMuzzleString = "Swing" + swing;
        }

        public override void OnSerialize(NetworkWriter writer)
        {
            base.OnSerialize(writer);
            writer.Write((byte)swing);
        }

        public override void OnDeserialize(NetworkReader reader)
        {
            base.OnDeserialize(reader);
            swing = (int)reader.ReadByte();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            if (fixedAge >= durationBeforeInterruptable)
                return InterruptPriority.Skill;
            return InterruptPriority.PrioritySkill;
        }

        public override void AuthorityModifyOverlapAttack(OverlapAttack overlapAttack)
        {
            Debug.Log("modify overlap");
            base.AuthorityModifyOverlapAttack(overlapAttack);
            DamageAPI.AddModdedDamageType(overlapAttack, ScrapOnHit.scrapOnHit);
        }
    }
}
