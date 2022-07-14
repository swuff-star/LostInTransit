using Moonstorm;
using RoR2;

namespace LostInTransit.Equipments
{
    [DisabledContent]
    public class Prescriptions : EquipmentBase
    {
        public override EquipmentDef EquipmentDef { get; } = LITAssets.LoadAsset<EquipmentDef>("Prescriptions");

        public override bool FireAction(EquipmentSlot slot)
        {
            slot.characterBody.AddTimedBuffAuthority(LITContent.Buffs.Meds.buffIndex, 12);
            return true;                                                 
        }                                                                       
    }                                                                           
                                                                                
}                                                                  
