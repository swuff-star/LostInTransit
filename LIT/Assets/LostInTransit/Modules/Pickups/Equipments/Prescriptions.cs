using LostInTransit.Items;
using Moonstorm;
using RoR2;

namespace LostInTransit.Equipments
{
    //[DisabledContent]
    public class Prescriptions : EquipmentBase
    {
        public override EquipmentDef EquipmentDef { get; } = LITAssets.LoadAsset<EquipmentDef>("Prescriptions", LITBundle.Equips);

        public override bool FireAction(EquipmentSlot slot)
        {
            float timeMult = BeatingEmbryoManager.Procs(slot) ? 1 : 2;
            slot.characterBody.AddTimedBuffAuthority(LITContent.Buffs.bdMeds.buffIndex, 12 * timeMult);
            return true;                                                 
        }                                                                       
    }                                                                           
                                                                                
}                                                                  
