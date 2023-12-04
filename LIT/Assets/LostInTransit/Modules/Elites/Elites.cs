using LostInTransit.Elites;
using Moonstorm;
using R2API.ScriptableObjects;
using RoR2;
using RoR2.ContentManagement;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

namespace LostInTransit.Modules
{
    public sealed class Elites : EliteModuleBase
    {
        public static Elites Instance { get; set; }
        public static MSEliteDef[] LoadedLITElites { get => LITContent.Instance.SerializableContentPack.eliteDefs as MSEliteDef[]; }
        public override R2APISerializableContentPack SerializableContentPack => LITContent.Instance.SerializableContentPack;
        public override AssetBundle AssetBundle => LITAssets.Instance.GetAssetBundle(LITBundle.Equips);

        public override void Initialize()
        {
            Instance = this;
            base.Initialize();
            LITLog.Info($"Initializing Elites...");
            GetInitializedEliteEquipmentBases();
            OnListCreated += LateEliteSetup;
        }

        protected override IEnumerable<EliteEquipmentBase> GetInitializedEliteEquipmentBases()
        {
            base.GetInitializedEliteEquipmentBases()
                .Where(elite => LITMain.config.Bind<bool>("Lost in Transit Elites", elite.GetType().Name, true, "Enable/disable this Elite Type.").Value)
                .ToList()
                .ForEach(elite => AddElite(elite));
            return null;
        }

        private void LateEliteSetup(ReadOnlyCollection<MSEliteDef> eliteCollection)
        {
            if (eliteCollection.Contains(LITAssets.LoadAsset<MSEliteDef>("Volatile", LITBundle.Equips)))
            {
                VolatileSpitebomb.BeginSetup();
            }
            if (eliteCollection.Contains(LITAssets.LoadAsset<MSEliteDef>("Blighted", LITBundle.Equips)))
            {
                Blight.BeginSetup();
            }
            if (eliteCollection.Contains(LITAssets.LoadAsset<MSEliteDef>("Leeching", LITBundle.Equips)))
            {
                RoR2Application.onLoad += () =>
                {
                    var grandpa = Resources.Load<GameObject>("prefabs/characterbodies/grandparentbody");
                    if(grandpa)
                    {
                        var charLoc = grandpa.GetComponentInChildren<ChildLocator>();
                        if(charLoc)
                        {
                            var headChild = charLoc.FindChild("Head");
                            HG.ArrayUtils.ArrayAppend(ref charLoc.transformPairs, new ChildLocator.NameTransformPair
                            {
                                name = "RingBottom",
                                transform = headChild.Find("head.2")
                            });
                            HG.ArrayUtils.ArrayAppend(ref charLoc.transformPairs, new ChildLocator.NameTransformPair
                            {
                                name = "RingMiddle",
                                transform = headChild.Find("head.2/head.3")
                            });
                            HG.ArrayUtils.ArrayAppend(ref charLoc.transformPairs, new ChildLocator.NameTransformPair
                            {
                                name = "RingTop",
                                transform = headChild.Find("head.2/head.3/head.4")
                            });
                        }
                    }
                };
            }
        }
    }
}