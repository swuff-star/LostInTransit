using Moonstorm;
using RoR2;

namespace LostInTransit.Equipments
{
    [DisabledContent]
    public class UnstableWatch : EquipmentBase
    {
        public override EquipmentDef EquipmentDef { get; } = LITAssets.LoadAsset<EquipmentDef>("UnstableWatch", LITBundle.Equips);

        public override bool FireAction(EquipmentSlot slot)
        {
            slot.characterBody.AddTimedBuffAuthority(LITContent.Buffs.bdTimeStop.buffIndex, 8);
            return true;                                                 
        }                                                                       
    }                                                                           
                                                                                
}                                                                  
