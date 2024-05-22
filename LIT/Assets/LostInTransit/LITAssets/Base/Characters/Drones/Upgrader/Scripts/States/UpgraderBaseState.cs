using RoR2;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Upgrader
{
    public abstract class UpgraderBaseState : EntityState
    {
        protected PurchaseInteraction pi;
        protected LostInTransit.Interactables.Upgrader.UpgraderInteractionToken uit; //I LOVE NAMESPACES. 

        protected virtual bool enableInteraction
        {
            get
            {
                return false;
            }
        }

        public override void OnEnter()
        {
            base.OnEnter();
            pi = GetComponent<PurchaseInteraction>();
            uit = GetComponent<LostInTransit.Interactables.Upgrader.UpgraderInteractionToken>();
            if (NetworkServer.active)
            {
                pi.SetAvailable(enableInteraction);
            }
        }
    }
}
