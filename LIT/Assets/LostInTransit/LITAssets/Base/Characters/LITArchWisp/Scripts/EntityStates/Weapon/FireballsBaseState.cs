using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace EntityStates.LITArchWisp.Weapon
{
    public abstract class FireballsBaseState : BaseState
    {
        public static string centralMuzzleLName;
        public static string muzzleL0Name;
        public static string muzzleL1Name;
        public static string muzzleL2Name;

        public static string centralMuzzleRName;
        public static string muzzleR0Name;
        public static string muzzleR1Name;
        public static string muzzleR2Name;

        protected Transform[] leftHandMuzzleTransforms;
        protected Transform leftHandCentralMuzzleTransform;
        protected Transform[] rightHandMuzzleTransforms;
        protected Transform rightHandCentralMuzzleTransform;

        protected ref InputBankTest.ButtonState skillButtonState => ref inputBank.skill1;

        public override void OnEnter()
        {
            base.OnEnter();
            leftHandMuzzleTransforms = new Transform[3]
            {
                FindModelChild(muzzleL0Name),
                FindModelChild(muzzleL1Name),
                FindModelChild(muzzleL2Name),
            };
            leftHandCentralMuzzleTransform = FindModelChild(centralMuzzleLName);

            rightHandMuzzleTransforms = new Transform[3]
            {
                FindModelChild(muzzleR0Name),
                FindModelChild(muzzleR1Name),
                FindModelChild(muzzleR2Name)
            };
            rightHandCentralMuzzleTransform = FindModelChild(centralMuzzleRName);
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}