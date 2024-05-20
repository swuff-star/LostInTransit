using LostInTransit.Interactables;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace LostInTransit.Components
{
    [RequireComponent(typeof(PurchaseInteraction))]
    public class EquipmentStorageChestController : MonoBehaviour
    {
        private void OnEnable()
        {
            PurchaseInteraction.onEquipmentSpentOnPurchase -= PurchaseInteraction_onEquipmentSpentOnPurchase;
            PurchaseInteraction.onEquipmentSpentOnPurchase += PurchaseInteraction_onEquipmentSpentOnPurchase;
        }

        private void PurchaseInteraction_onEquipmentSpentOnPurchase(PurchaseInteraction arg1, Interactor arg2, EquipmentIndex arg3)
        {
            if (!NetworkServer.active)
            {
                return;
            }
            var interactorBody = arg2.GetComponent<CharacterBody>();
            Vector3 corePosition = interactorBody ? interactorBody.corePosition : arg2.transform.position;

            EffectData effectData = new EffectData
            {
                origin = corePosition,
                genericFloat = 1.5f,
                genericUInt = (uint)(arg3) + 1
            };
            effectData.SetNetworkedObjectReference(arg1.gameObject);
            EffectManager.SpawnEffect(EquipmentStorageChest.EquipmentTakenOrbPrefab, effectData, true);

            EquipmentStorageChest.SetEquipmentStored(arg3);
        }

        public void OnPurchase(Interactor i)
        {
            transform.localScale = Vector3.one / 2;
        }

        private void OnDisable()
        {
            PurchaseInteraction.onEquipmentSpentOnPurchase -= PurchaseInteraction_onEquipmentSpentOnPurchase;
        }
    }
}
