using MSU;
using RoR2;
using RoR2.ContentManagement;
using System.Collections;
using UnityEngine;

namespace LostInTransit.Equipments
{
#if DEBUG
    public sealed class FieldGenerator : LITEquipment
    {
        public override NullableRef<GameObject> ItemDisplayPrefab => null;
        public override EquipmentDef EquipmentDef => _equipmentDef;
        private EquipmentDef _equipmentDef;
        private EquipmentDef _consumedEquipmentDef;

        public override bool Execute(EquipmentSlot slot)
        {
            return false;
        }

        public override void Initialize()
        {
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return false;
        }

        public override IEnumerator LoadContentAsync()
        {
            /*
             * EquipmentDef - "FieldGenerator" - Equips
             * EquipmentDef - "FieldGeneratorUsed" - Equips
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
#endif
}
