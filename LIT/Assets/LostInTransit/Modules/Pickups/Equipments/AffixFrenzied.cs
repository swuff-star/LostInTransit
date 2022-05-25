using Moonstorm;
using RoR2;
using System.Collections.Generic;

namespace LostInTransit.Equipments
{
    public class AffixFrenzied : EliteEquipmentBase
    {
        public override List<MSEliteDef> EliteDefs { get; } = new List<MSEliteDef>
        {
             LITAssets.Instance.MainAssetBundle.LoadAsset<MSEliteDef>("Frenzied"),
              LITAssets.Instance.MainAssetBundle.LoadAsset<MSEliteDef>("FrenziedHonor")
        };
        public override EquipmentDef EquipmentDef { get; } = LITAssets.Instance.MainAssetBundle.LoadAsset<EquipmentDef>("AffixFrenzied");
        //public override MSAspectAbilityDataHolder AspectAbilityData { get; } = LITAssets.Instance.MainAssetBundle.LoadAsset<MSAspectAbilityDataHolder>("AbilityFrenzied");
        
        public override bool FireAction(EquipmentSlot slot)
        {
            /*if (MSUtil.IsModInstalled("com.TheMysticSword.AspectAbilities"))
            {
                var component = slot.characterBody.GetComponent<Buffs.AffixFrenzied.AffixFrenziedBehavior>();
                if (component)
                {
                    component.Ability();
                    return true;
                }
            }*/

            return false;
        }
    }
}
