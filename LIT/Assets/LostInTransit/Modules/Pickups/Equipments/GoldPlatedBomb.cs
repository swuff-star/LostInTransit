using Moonstorm;
using RoR2;

namespace LostInTransit.Equipments
{
    //[DisabledContent]
    public class GoldPlatedBomb : EquipmentBase
    {
        public override EquipmentDef EquipmentDef { get; } = LITAssets.LoadAsset<EquipmentDef>("GoldPlatedBomb", LITBundle.Equips);

        public override bool FireAction(EquipmentSlot slot)
        {
            slot.characterBody.AddTimedBuffAuthority(LITContent.Buffs.bdMeds.buffIndex, 12);
            return true;                                                 
        }                                                                       
    }                                                                           
                                                                                
}                                                                  
