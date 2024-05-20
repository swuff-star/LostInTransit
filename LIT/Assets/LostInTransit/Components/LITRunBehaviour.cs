using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LostInTransit.Components
{
    public class LITRunBehaviour : MonoBehaviour
    {
        public static LITRunBehaviour Instance { get; private set; }
        public bool HasAnyEquipmentBarrelBeenOpened { get; private set; } = false;
        public bool HasEquipmentBeenStoredThisRun { get; private set; }
        private GameObject _firstEquipmentBarrelObject;
    
        private void OnEnable()
        {
            Instance = this;
            GlobalEventManager.OnInteractionsGlobal -= EquipmentStorageInteractionChecks;
            GlobalEventManager.OnInteractionsGlobal += EquipmentStorageInteractionChecks;
        }

        private void EquipmentStorageInteractionChecks(Interactor arg1, IInteractable arg2, GameObject arg3)
        {
            CheckForEquipmentStorageChestInteraction(arg2);
            CheckForBarrelOpening(arg2, arg3);
            
        }

        private void CheckForBarrelOpening(IInteractable interactableComponent, GameObject interactableObject)
        {
            if (HasAnyEquipmentBarrelBeenOpened)
            {
                return;
            }

            if (!(interactableComponent is PurchaseInteraction pi))
            {
                return;
            }

            //Why are there no interactable indices...
            string displayNameToken = pi.displayNameToken;
            if (displayNameToken == "EQUIPMENTBARREL_NAME")
            {
                HasAnyEquipmentBarrelBeenOpened = true;
                _firstEquipmentBarrelObject = interactableObject;
            }
        }

        private void CheckForEquipmentStorageChestInteraction( IInteractable interactableComponent)
        {
            if (HasEquipmentBeenStoredThisRun)
                return;

            if(!(interactableComponent is PurchaseInteraction pi))
            {
                return;
            }

            string displayNameToken = pi.displayNameToken;
            if(displayNameToken == "LIT_EQUIPMENTSTORAGE_NAME")
            {
                HasEquipmentBeenStoredThisRun = true;
            }
        }

        public bool IsGameObjectTheFirstEquipmentBarrelBeingOpen(GameObject go)
        {
            return _firstEquipmentBarrelObject ? go == _firstEquipmentBarrelObject : false;
        }

        private void OnDisable()
        {
            if(Instance == this)
            {
                Instance = null;

                GlobalEventManager.OnInteractionsGlobal -= EquipmentStorageInteractionChecks;
            }
        }
    }
}
