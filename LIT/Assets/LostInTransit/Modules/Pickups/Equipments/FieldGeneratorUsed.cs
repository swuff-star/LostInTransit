using MSU;
using RoR2;

namespace LostInTransit.Equipments
{
    [DisabledContent]
    public class FieldGeneratorUsed : LITEquipment
    {
        public override EquipmentDef EquipmentDef { get; } = LITAssets.LoadAsset<EquipmentDef>("FieldGeneratorUsed", LITBundle.Equips);
        public override bool FireAction(EquipmentSlot slot)
        {
            return false;
        }
    }
}
