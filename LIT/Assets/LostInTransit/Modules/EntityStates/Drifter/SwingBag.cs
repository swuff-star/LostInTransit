using LostInTransit.Components;
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

        public static float swingTimeCoefficient = 1.5f;

        public static float baseDurationBeforeInterruptable;

        public static float comboFinisherBaseDuration;
        public static float comboFinisherHitPauseDuration;
        public static float comboFinisherDamageCoefficient;
        //public static float comboFinisherBloom;
        public static float comboFinisherBaseDurationBeforeInterruptable;

        private float durationBeforeInterruptable;

        private DrifterBagComponent dbc;

        public override void OnEnter()
        {
            //Debug.Log("on enter");
            if (isComboFinisher)
            {
                hitBoxGroupName = "SwingLarge";
                hitPauseDuration = comboFinisherHitPauseDuration;
                damageCoefficient = comboFinisherDamageCoefficient;
                baseDuration = comboFinisherBaseDuration;
            }
            durationBeforeInterruptable = (isComboFinisher ? (comboFinisherBaseDurationBeforeInterruptable / attackSpeedStat) : (baseDurationBeforeInterruptable / attackSpeedStat));

            dbc = characterBody.GetComponent<DrifterBagComponent>();
            if (dbc != null)
                dbc.bagDown = true;

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
            //Debug.Log("play anim");
            string animationStateName = "Swing" + swing;
            PlayCrossfade("Gesture, Override", animationStateName, "Primary.playbackRate", duration * swingTimeCoefficient, 0.1f);
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
            //Debug.Log("modify overlap");
            base.AuthorityModifyOverlapAttack(overlapAttack);
            switch(swing)
            {
                case 0:
                    DamageAPI.AddModdedDamageType(overlapAttack, ScrapOnHit10.scrapOnHit10);
                    break;
                case 1:
                    DamageAPI.AddModdedDamageType(overlapAttack, ScrapOnHit20.scrapOnHit20);
                    break;
                case 2:
                    DamageAPI.AddModdedDamageType(overlapAttack, ScrapOnHit30.scrapOnHit30);
                    break;
            }

            if (isComboFinisher)
                overlapAttack.damageType = DamageType.Stun1s;
        }
    }
}
