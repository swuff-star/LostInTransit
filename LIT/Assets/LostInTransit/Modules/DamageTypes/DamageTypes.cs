using Moonstorm;
using System.Collections.Generic;
using System.Linq;

namespace LostInTransit.DamageTypes
{
    public sealed class DamageTypes : DamageTypeModuleBase
    {
        public static DamageTypes Instance { get; private set; }

        public override void Initialize()
        {
            Instance = this;
            base.Initialize();
            LITLog.Info($"Initializing Damage Types.");
            GetDamageTypeBases();
        }

        protected override IEnumerable<DamageTypeBase> GetDamageTypeBases()
        {
            base.GetDamageTypeBases()
                .ToList()
                .ForEach(dType => AddDamageType(dType));
            return null;
        }
    }
}
