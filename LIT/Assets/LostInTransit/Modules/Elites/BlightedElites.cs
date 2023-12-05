using LostInTransit.Components;
using Moonstorm;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace LostInTransit.Elites
{
    public static class BlightedElites
    {
        public static bool Initialized { get; private set; }
        public static ReadOnlyCollection<EliteDef> ElitesHonorDisabled { get; private set; }
        private static List<EliteDef> _elitesHonorDisabled = new List<EliteDef>();
        public static ReadOnlyCollection<EliteDef> ElitesHonorEnabled { get; private set; }
        private static List<EliteDef> _elitesHonorEnabled = new List<EliteDef>();

        public static void AddEliteToHonorEnabledList(EliteDef eliteDef) => _elitesHonorEnabled.Add(eliteDef);
        public static void AddEliteToHonorDisabledList(EliteDef eliteDef) => _elitesHonorDisabled.Add(eliteDef);

        [SystemInitializer(typeof(EliteCatalog))]
        private static void Initialize()
        {
            if (!EliteCatalog.eliteDefs.Contains(LITAssets.LoadAsset<MSEliteDef>("Blighted", LITBundle.Equips)))
            {
                return;
            }
            LITLog.Info("Blighted elites enabled, setting up underlying systems...");
            Initialized = true;

            RoR2Application.onLoad += AddDefaultElites;
            Run.onRunStartGlobal += CreateFinalizedCollections;
            On.RoR2.Util.GetBestBodyName += MakeBlightedName;
        }

        private static string MakeBlightedName(On.RoR2.Util.orig_GetBestBodyName orig, GameObject bodyObject)
        {
            var text = orig(bodyObject);
            var blightedIndex = LITContent.Buffs.bdAffixBlighted.buffIndex;
            if (!bodyObject)
                return text;

            if (!bodyObject.TryGetComponent<CharacterBody>(out var body))
                return text;

            if(!body.HasBuff(blightedIndex))
            {
                return text;
            }

            foreach(BuffIndex buffIndex in BuffCatalog.eliteBuffIndices)
            {
                if (buffIndex == blightedIndex)
                    continue;

                var eliteToken = Language.GetString(BuffCatalog.GetBuffDef(buffIndex).eliteDef.modifierToken);
                eliteToken = eliteToken.Replace("{0}", string.Empty);
                text = text.Replace(eliteToken, string.Empty);
            }
            return text;
        }

        private static void AddDefaultElites()
        {
            AddEliteToHonorDisabledList(RoR2Content.Elites.Fire);
            AddEliteToHonorDisabledList(RoR2Content.Elites.Lightning);
            AddEliteToHonorDisabledList(RoR2Content.Elites.Ice);
            AddEliteToHonorDisabledList(DLC1Content.Elites.Earth);
            AddEliteToHonorDisabledList(LITContent.Elites.Frenzied);
            AddEliteToHonorDisabledList(LITContent.Elites.Volatile);

            AddEliteToHonorEnabledList(RoR2Content.Elites.FireHonor);
            AddEliteToHonorEnabledList(RoR2Content.Elites.LightningHonor);
            AddEliteToHonorEnabledList(RoR2Content.Elites.IceHonor);
            AddEliteToHonorEnabledList(DLC1Content.Elites.EarthHonor);
            AddEliteToHonorEnabledList(LITContent.Elites.FrenziedHonor);
            AddEliteToHonorEnabledList(LITContent.Elites.VolatileHonor);
        }

        private static void CreateFinalizedCollections(Run obj)
        {
            var honorDisabledCollection = new List<EliteDef>();
            var honorEnabledCollection = new List<EliteDef>();

            foreach(var nonHonorElite in _elitesHonorDisabled)
            {
                if(!nonHonorElite.eliteEquipmentDef.requiredExpansion)
                {
                    honorDisabledCollection.Add(nonHonorElite);
                    continue;
                }

                if (obj.IsExpansionEnabled(nonHonorElite.eliteEquipmentDef.requiredExpansion))
                    honorDisabledCollection.Add(nonHonorElite);
            }
            foreach(var honorElite in _elitesHonorEnabled)
            {
                if(!honorElite.eliteEquipmentDef.requiredExpansion)
                {
                    honorEnabledCollection.Add(honorElite);
                    continue;
                }

                if (obj.IsExpansionEnabled(honorElite.eliteEquipmentDef.requiredExpansion))
                    honorEnabledCollection.Add(honorElite);
            }

            ElitesHonorEnabled = new ReadOnlyCollection<EliteDef>(honorEnabledCollection);
            ElitesHonorDisabled = new ReadOnlyCollection<EliteDef>(honorDisabledCollection);
        }
    }
}
