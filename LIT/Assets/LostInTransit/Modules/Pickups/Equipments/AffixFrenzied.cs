using MSU;
using RoR2;
using RoR2.ContentManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LostInTransit.Equipments
{
    public sealed class AffixFrenzied : LITEliteEquipment
    {
        public override List<EliteDef> EliteDefs => _eliteDefs;
        private List<EliteDef> _eliteDefs;

        public override NullableRef<GameObject> ItemDisplayPrefab => null;
        public override EquipmentDef EquipmentDef => _equipmentDef;
        private EquipmentDef _equipmentDef;

        public override bool Execute(EquipmentSlot slot)
        {
            return FireActionStatic(slot.gameObject);
        }

        internal static bool FireActionStatic(GameObject bodyObj)
        {
            var bodyStateMachine = EntityStateMachine.FindByCustomName(bodyObj, "Body");
            var healthComponent = bodyObj.GetComponent<HealthComponent>();
            if (healthComponent.alive && bodyStateMachine)
            {
                //Todd Howard Voice: It just works.
                bodyStateMachine.SetNextState(new EntityStates.AffixFrenzied.FrenziedTeleport());
                return true;
            }

            return false;
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
             * ExtendedEliteDef - "Frenzied" - Equips
             * ExtendedEliteDef - "FrenziedHonor" - Equips
             * EquipmentDef - "AffixFrenzied" - Equips
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
