using Moonstorm;
using RoR2;

namespace LostInTransit.Equipments
{
    [DisabledContent]
    public class FieldGeneratorUsed : EquipmentBase
    {
        public override EquipmentDef EquipmentDef { get; } = LITAssets.LoadAsset<EquipmentDef>("FieldGeneratorUsed", LITBundle.Equips);
        public override bool FireAction(EquipmentSlot slot)
        {
            return false;
        }
    }
}
