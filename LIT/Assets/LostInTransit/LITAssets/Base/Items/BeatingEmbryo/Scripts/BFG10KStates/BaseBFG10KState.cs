using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace EntityStates.BFG
{
    public abstract class BaseBFG10KState : BaseBodyAttachmentState
    {
        public static string fireAnim;
        public static string idleAnim;
        public static string chargeAnim;

        public EquipmentSlot Slot { get; private set; }
        public GameObject BFGDisplay { get; private set; }
        public Transform BFGMuzzle { get; private set; }
        public Animator BFGAnimator { get; private set; }
        public InputBankTest BodyInputBank { get; private set; }

        public override void OnEnter()
        {
            base.OnEnter();
            BodyInputBank = attachedBody ? attachedBody.inputBank : null;
            Slot = attachedBody.equipmentSlot;
            if(Slot)
            {
                var bfgTransform = Slot.FindActiveEquipmentDisplay();
                if(!bfgTransform)
                {
                    return;
                }

                BFGDisplay = bfgTransform.gameObject;
                var childLocator = BFGDisplay.GetComponentInChildren<ChildLocator>();
                if(childLocator)
                {
                    BFGMuzzle = childLocator.FindChild("Muzzle");
                }
                BFGAnimator = BFGDisplay.GetComponentInChildren<Animator>();
            }
        }
    }
}
