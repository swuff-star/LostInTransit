using Moonstorm;
using RoR2;

namespace LostInTransit.Equipments
{
    [DisabledContent]
    public class FieldGenerator : EquipmentBase
    {
        public override EquipmentDef EquipmentDef { get; } = LITAssets.LoadAsset<EquipmentDef>("FieldGenerator");
        public override bool FireAction(EquipmentSlot slot)
        {
            return false;
        }
    }
}
