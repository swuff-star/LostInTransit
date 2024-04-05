using MSU;
using System;

namespace LostInTransit.Equipments
{
    /*[Obsolete("Aspect abilities itself is deprecated, no clue if we should keep this stuff or not.")]
    public abstract class LITEliteEquip : EliteEquipmentBase                   ★ i think this mod is dead now? just deprecating support entirely.
    {
        //public abstract MSAspectAbilityDataHolder AspectAbilityData { get; set; }

        public override void Initialize()
        {
            if (MSUtil.IsModInstalled("com.TheMysticSword.AspectAbilities"))
            {
                if (AspectAbilityData != null)
                    RunAspectAbility();
                else
                    LITLogger.LogD($"The EliteEquipment for {EliteDef.name} doesnt have an Aspect Ability Data asset associated with it.");
            }
        }

        private void RunAspectAbility()               
        {
            AspectAbilities.AspectAbility ability = new AspectAbilities.AspectAbility
            {
                aiMaxUseDistance = AspectAbilityData.aiMaxUseDistance,
                aiHealthFractionToUseChance = AspectAbilityData.aiHealthFractionToUseChance,
                equipmentDef = AspectAbilityData.equipmentDef,
                onUseOverride = FireAction,
            };

            AspectAbilities.AspectAbilitiesPlugin.RegisterAspectAbility(ability);
        }
    }*/
}
