using LostInTransit.Items;
using MSU;
using RoR2;
using RoR2.ContentManagement;
using System.Collections;
using UnityEngine;

namespace LostInTransit.Equipments
{
    public class Prescriptions : LITEquipment
    {
        public override NullableRef<GameObject> ItemDisplayPrefab => null;

        public override EquipmentDef EquipmentDef => _equipmentDef;
        private EquipmentDef _equipmentDef;

        public override bool Execute(EquipmentSlot slot)
        {
            float timeMult = BeatingEmbryoManager.BeatingEmbryoProcs(slot) ? 1 : 2;
            slot.characterBody.AddTimedBuffAuthority(LITContent.Buffs.bdMeds.buffIndex, 12 * timeMult);
            return true;
        }

        public override void Initialize()
        {
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public override IEnumerator LoadContentAsync()
        {
            /*
             * EquipmentDef - "Prescriptions" - Equips
             */
            yield break;
        }

        public override void OnEquipmentLost(CharacterBody body)
        {
        }

        public override void OnEquipmentObtained(CharacterBody body)
        {
        }
    }
}                                                                  
