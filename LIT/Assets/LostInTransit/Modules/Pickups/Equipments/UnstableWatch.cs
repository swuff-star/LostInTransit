using LostInTransit.Items;
using MSU;
using RoR2;
using RoR2.ContentManagement;
using System.Collections;
using UnityEngine;

namespace LostInTransit.Equipments
{
#if DEBUG
    public sealed class UnstableWatch : LITEquipment
    {
        public override NullableRef<GameObject> ItemDisplayPrefab => null;
        public override EquipmentDef EquipmentDef => _equipmentDef;
        private EquipmentDef _equipmentDef;

        public override bool Execute(EquipmentSlot slot)
        {
            float timeMult = BeatingEmbryoManager.BeatingEmbryoProcs(slot) ? 1 : 2;
            slot.characterBody.AddTimedBuffAuthority(LITContent.Buffs.bdTimeStop.buffIndex, 8 * timeMult);
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
             * EquipmentDef - "UnstableWatch" - Equips
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
