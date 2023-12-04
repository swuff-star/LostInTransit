using Moonstorm;
using RoR2;
using System.Collections.Generic;
using UnityEngine;

namespace LostInTransit.Equipments
{
    public class AffixFrenzied : EliteEquipmentBase
    {
        public override List<MSEliteDef> EliteDefs { get; } = new List<MSEliteDef>
        {
             LITAssets.LoadAsset<MSEliteDef>("Frenzied", LITBundle.Equips),
             LITAssets.LoadAsset<MSEliteDef>("FrenziedHonor", LITBundle.Equips)
        };
        public override EquipmentDef EquipmentDef { get; } = LITAssets.LoadAsset<EquipmentDef>("AffixFrenzied", LITBundle.Items);
        
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

        public override bool FireAction(EquipmentSlot slot)
        {
            return FireActionStatic(slot.gameObject);
        }
    }
}
