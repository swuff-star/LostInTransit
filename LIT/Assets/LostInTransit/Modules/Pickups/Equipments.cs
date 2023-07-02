using Moonstorm;
using R2API.ScriptableObjects;
using RoR2;
using System.Collections.Generic;
using System.Linq;

namespace LostInTransit.Modules
{
    public sealed class Equipments : EquipmentModuleBase
    {
        public static Equipments Instance { get; private set; }
        public static EquipmentDef[] LoadedLITEquipments { get => LITContent.Instance.SerializableContentPack.equipmentDefs; }
        public override R2APISerializableContentPack SerializableContentPack => LITContent.Instance.SerializableContentPack;

        public override void Initialize()
        {
            Instance = this;
            base.Initialize();
            LITLog.Info($"Initializing Equipments...");
            GetEquipmentBases();
            GetEliteEquipmentBases();
        }

        protected override IEnumerable<EquipmentBase> GetEquipmentBases()
        {
            base.GetEquipmentBases()
                .Where(eqp => LITMain.config.Bind<bool>(eqp.EquipmentDef.name, "Enable Equipment", true, "Wether or not to enable this equipment").Value)
                .ToList()
                .ForEach(eqp => AddEquipment(eqp));
            return null;
        }

        protected override IEnumerable<EliteEquipmentBase> GetEliteEquipmentBases()
        {
            base.GetEliteEquipmentBases()
                .ToList()
                .ForEach(eeqp => AddEliteEquipment(eeqp));
            return null;
        }
    }
}
