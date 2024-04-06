using MSU;
using RoR2;
using RoR2.ContentManagement;
using System.Collections;
using UnityEngine;

namespace LostInTransit.Equipments
{
#if DEBUG
    public sealed class GoldPlatedBomb : LITEquipment
    {
        public override NullableRef<GameObject> ItemDisplayPrefab => null;

        public override EquipmentDef EquipmentDef => _equipmentDef;
        private EquipmentDef _equipmentDef;

        public override bool Execute(EquipmentSlot slot)
        {
            slot.characterBody.AddTimedBuffAuthority(LITContent.Buffs.bdMeds.buffIndex, 12);
            return true;
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
             * EquipmentDef - "GoldPlatedBomb" - Equips
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
