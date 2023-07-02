using Moonstorm;
using R2API.ScriptableObjects;
using RoR2;
using System.Collections.Generic;
using System.Linq;

namespace LostInTransit.Modules
{
    public sealed class Items : ItemModuleBase
    {
        public static Items Instance { get; private set; }
        public static ItemDef[] LoadedLITItems { get => LITContent.Instance.SerializableContentPack.itemDefs; }
        public override R2APISerializableContentPack SerializableContentPack => LITContent.Instance.SerializableContentPack;

        public override void Initialize()
        {
            Instance = this;
            base.Initialize();
            LITLog.Info($"Initializing Items...");
            GetItemBases();
        }

        protected override IEnumerable<ItemBase> GetItemBases()
        {
            base.GetItemBases()
                .Where(item => LITMain.config.Bind<bool>(item.ItemDef.name, "Enable Item", true, "Wether or not to enable this item.").Value)
                .ToList()
                .ForEach(item => AddItem(item, null));
            return null;
        }
    }
}
