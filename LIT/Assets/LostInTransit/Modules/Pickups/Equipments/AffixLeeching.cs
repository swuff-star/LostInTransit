﻿using Moonstorm;
using RoR2;

namespace LostInTransit.Equipments
{
    public class AffixLeeching : LITEliteEquip
    {
        public override MSEliteDef EliteDef { get; set; } = LITAssets.Instance.MainAssetBundle.LoadAsset<MSEliteDef>("Leeching");
        public override EquipmentDef EquipmentDef { get; set; } = LITAssets.Instance.MainAssetBundle.LoadAsset<EquipmentDef>("AffixLeeching");
        //public override MSAspectAbilityDataHolder AspectAbilityData { get; set; } = LITAssets.Instance.MainAssetBundle.LoadAsset<MSAspectAbilityDataHolder>("AbilityLeeching");

        /*public override bool FireAction(EquipmentSlot slot)
        {
            if (MSUtil.IsModInstalled("com.TheMysticSword.AspectAbilities"))
            {
                var component = slot.characterBody.GetComponent<Buffs.AffixLeeching.AffixLeechingBehavior>();
                if (component)
                {
                    component.Ability();
                    return true;
                }
            }
            return false;
        }*/
    }
}
