using LostInTransit.Components;
using Moonstorm;
using R2API;
using RoR2;
using RoR2.ExpansionManagement;
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

        public static ReadOnlyCollection<BodyIndex> PrestigeBodyBlacklist { get; private set; }
        private static List<CharacterBody> _prestigeBodyBlacklist = new List<CharacterBody>();

        public static ReadOnlyCollection<BodyIndex> RegularBodyBlacklist { get; private set; }
        private static List<CharacterBody> _regularBodyBlacklist = new List<CharacterBody>();

        public static void AddBodyToRegularBlacklist(CharacterBody body) => _regularBodyBlacklist.Add(body);
        public static void AddBodyToPrestigeBlacklist(CharacterBody body) => _prestigeBodyBlacklist.Add(body);

        public static void AddEliteToHonorEnabledList(EliteDef eliteDef) => _elitesHonorEnabled.Add(eliteDef);
        public static void AddEliteToHonorDisabledList(EliteDef eliteDef) => _elitesHonorDisabled.Add(eliteDef);

        [SystemInitializer(typeof(EliteCatalog), typeof(BodyCatalog))]
        private static void Initialize()
        {
            if (!EliteCatalog.eliteDefs.Contains(LITAssets.LoadAsset<MSEliteDef>("Blighted", LITBundle.Equips)))
            {
                return;
            }
            LITLog.Info("Blighted elites enabled, setting up underlying systems...");
            Initialized = true;

            RoR2Application.onLoad += AddDefaultCollectionEntries;
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

        private static void AddDefaultCollectionEntries()
        {
            AddElitesToLists();
            AddBodiesToLists();
        }

        private static void AddElitesToLists()
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

        private static void AddBodiesToLists()
        {
            //Altar Skeleton
            AddToBothBlacklists("AltarSkeleton");

            //Reliquary
            AddToBothBlacklists("ArtifactShell");

            //Birdshark
            AddToBothBlacklists("Birdshark");

            //N- Id like to have mmtirhix have a chance to be blighted with prestige for the funny, but i cant be bothered atm to make the hurt and haunt body blighted only if the original body was.
            AddToBothBlacklists("Brother");
            AddToBothBlacklists("BrotherHurt");
            AddToBothBlacklists("BrotherHaunt");

            //Drones and turret
            AddToBothBlacklists("BackupDrone");
            AddToBothBlacklists("Drone1");
            AddToBothBlacklists("Drone2");
            AddToBothBlacklists("EmergencyDrone");
            AddToBothBlacklists("EquipmentDrone");
            AddToBothBlacklists("FlameDrone");
            AddToBothBlacklists("MegaDrone");
            AddToBothBlacklists("MissileDrone");
            AddToBothBlacklists("Turret1");
            AddToBothBlacklists("DroneCommander");

            //Worms
            AddToBothBlacklists("ElectricWorm");
            AddToBothBlacklists("MagmaWorm");


            //Engi's Turrets, They inherit the equipment anyways so if the og engi is blighted then the others will
            AddToBothBlacklists("EngiTurret");
            AddToBothBlacklists("EngiWalkerTurret");

            //Explosive barrels
            AddToBothBlacklists("ExplosivePotDestructible");
            AddToBothBlacklists("FusionCellDestructible");
            AddToBothBlacklists("SulfurPod");

            //Chimera
            AddToBothBlacklists("LunarExploder");
            AddToBothBlacklists("LunarGolem");
            AddToBothBlacklists("LunarWisp");

            //Voids...? i'll leave them commented for now
            /*AddToBothBlacklists("Nullifier");
            AddToBothBlacklists("VoidBarnacle");
            AddToBothBlacklists("VoidJailer");
            AddToBothBlacklists("VoidMegaCrab");*/

            //Void Allies...? i'll leave them commented for now
            /*AddToBothBlacklists("NullifierAlly");
            AddToBothBlacklists("VoidJailerAlly");
            AddToBothBlacklists("VoidMmegaCrabAlly");*/

            //Voidling and its morbillion phases
            AddToBothBlacklists("MiniVoidRaidCrabBodyBase");
            AddToBothBlacklists("MiniVoidRaidCrabBodyPhase1");
            AddToBothBlacklists("MiniVoidRaidCrabBodyPhase2");
            AddToBothBlacklists("MiniVoidRaidCrabBodyPhase3");
            AddToBothBlacklists("VoidRaidCrab");

            //Void Infestor
            AddToBothBlacklists("VoidInfestor");

            //Newt
            AddToBothBlacklists("Shopkeeper");

            //Spectators
            AddToBothBlacklists("Spectator");
            AddToBothBlacklists("SpectatorSlow");

            //Malachite Urchin
            AddToBothBlacklists("UrchinTurret");

            //Mending Heal Bomb
            AddToBothBlacklists("AffixEarthHealer");

            //Time Crystals
            AddToBothBlacklists("TimeCrystal");

            //Roboball buddies
            AddToBothBlacklists("RoboBallRedBuddy");
            AddToBothBlacklists("RoboBallGreenBuddy");

            //Gland Beetle Guard
            AddToBothBlacklists("BeetleGuardAlly");

            //Alpha Construct ally
            AddToBothBlacklists("MinorConstructOnKill");

            //Squid Turrets
            AddToBothBlacklists("SquidTurret");

            //Geep
            AddToBothBlacklists("Geep");
            //Gip
            AddToBothBlacklists("Gip");

            void AddToBothBlacklists(string name)
            {
                AddBodyToPrestigeBlacklist(GetBody(name));
                AddBodyToRegularBlacklist(GetBody(name));
            }
            CharacterBody GetBody(string name)
            {
                return BodyCatalog.GetBodyPrefabBodyComponent(BodyCatalog.FindBodyIndexCaseInsensitive(name.Contains("Body") ? name : name + "Body"));
            }
        }

        private static void CreateFinalizedCollections(Run obj)
        {
            CreateEliteCollections(obj);
            CreateBlacklistCollections(obj);
        }

        private static void CreateEliteCollections(Run run)
        {
            var honorDisabledCollection = new List<EliteDef>();
            var honorEnabledCollection = new List<EliteDef>();

            foreach (var nonHonorElite in _elitesHonorDisabled)
            {
                if (nonHonorElite.eliteIndex == EliteIndex.None)
                    continue;

                if (!nonHonorElite.eliteEquipmentDef.requiredExpansion)
                {
                    honorDisabledCollection.Add(nonHonorElite);
                    continue;
                }

                if (run.IsExpansionEnabled(nonHonorElite.eliteEquipmentDef.requiredExpansion))
                    honorDisabledCollection.Add(nonHonorElite);
            }
            foreach (var honorElite in _elitesHonorEnabled)
            {
                if (honorElite.eliteIndex == EliteIndex.None)
                    continue;

                if (!honorElite.eliteEquipmentDef.requiredExpansion)
                {
                    honorEnabledCollection.Add(honorElite);
                    continue;
                }

                if (run.IsExpansionEnabled(honorElite.eliteEquipmentDef.requiredExpansion))
                    honorEnabledCollection.Add(honorElite);
            }

            ElitesHonorEnabled = new ReadOnlyCollection<EliteDef>(honorEnabledCollection);
            ElitesHonorDisabled = new ReadOnlyCollection<EliteDef>(honorDisabledCollection);
        }

        private static void CreateBlacklistCollections(Run run)
        {
            var prestigeBlacklist = new List<BodyIndex>();
            var regularBlacklist = new List<BodyIndex>();

            foreach(var prestigeBody in _prestigeBodyBlacklist)
            {
                if (prestigeBody.bodyIndex == BodyIndex.None)
                    continue;

                if(!prestigeBody.TryGetComponent<ExpansionRequirementComponent>(out var expansionRequired))
                {
                    prestigeBlacklist.Add(prestigeBody.bodyIndex);
                    continue;
                }

                if (run.IsExpansionEnabled(expansionRequired.requiredExpansion))
                {
                    prestigeBlacklist.Add(prestigeBody.bodyIndex);
                }
            }

            foreach (var regularBody in _regularBodyBlacklist)
            {
                if (regularBody.bodyIndex == BodyIndex.None)
                    continue;

                if (!regularBody.TryGetComponent<ExpansionRequirementComponent>(out var expansionRequired))
                {
                    prestigeBlacklist.Add(regularBody.bodyIndex);
                    continue;
                }

                if (run.IsExpansionEnabled(expansionRequired.requiredExpansion))
                {
                    prestigeBlacklist.Add(regularBody.bodyIndex);
                }
            }

            PrestigeBodyBlacklist = new ReadOnlyCollection<BodyIndex>(prestigeBlacklist);
            RegularBodyBlacklist = new ReadOnlyCollection<BodyIndex>(regularBlacklist);
        }
    }
}
