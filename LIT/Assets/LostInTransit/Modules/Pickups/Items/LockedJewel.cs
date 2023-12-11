using Moonstorm;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace LostInTransit.Items
{
    public class LockedJewel : ItemBase
    {
        public override ItemDef ItemDef => LITAssets.LoadAsset<ItemDef>("LockedJewel", LITBundle.Items);

        public static float barrierGain = 20;
        public static int moneyGain = 8;
        public override void Initialize()
        {
            GlobalEventManager.OnInteractionsGlobal += GlobalEventManager_OnInteractionsGlobal;
        }

        private void GlobalEventManager_OnInteractionsGlobal(Interactor arg1, IInteractable arg2, UnityEngine.GameObject arg3)
        {
            if (!NetworkServer.active)
                return;

            if (!InteractableIsPermittedForSpawn(arg3))
                return;

            if(!arg1.TryGetComponent<CharacterBody>(out var body))
                return;

            var itemCount = body.GetItemCount(ItemDef);
            if (itemCount == 0)
                return;

            var healthComponent = body.healthComponent;
            if (!healthComponent)
                return;

            var maxBarrier = healthComponent.fullBarrier;
            var barrierPercentage = (barrierGain + ((barrierGain / 2) * (itemCount - 1))) / 100;
            healthComponent.AddBarrier(Mathf.Min(maxBarrier, maxBarrier * barrierPercentage));

            if (body.master)
                body.master.GiveMoney((uint)Run.instance.GetDifficultyScaledCost(moneyGain));
        }

        //N- Todo, add this method as a utility in MSU.
        bool InteractableIsPermittedForSpawn(GameObject interactableGameObject)
        {
            if (!interactableGameObject)
                return false;

            if(interactableGameObject.TryGetComponent<InteractionProcFilter>(out var val))
                return val.shouldAllowOnInteractionBeginProc;

            if(interactableGameObject.TryGetComponent<GenericPickupController>(out _))
                return false;

            if (interactableGameObject.TryGetComponent<VehicleSeat>(out _))
                return false;

            if (interactableGameObject.TryGetComponent<NetworkUIPromptController>(out _))
                return false;

            return true;
        }
    }
}