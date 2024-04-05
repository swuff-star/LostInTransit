using RoR2.ContentManagement;
using R2API.ScriptableObjects;
using R2API.ContentManagement;
using System;
using System.Linq;
using RoR2;
using UnityEngine;
using System.Collections;
using MSU;
using RoR2.ExpansionManagement;

namespace LostInTransit
{
    public class LITContent : IContentPackProvider
    {
        public string identifier => LITMain.GUID;
        public static ReadOnlyContentPack ReadOnlyContentPack => new ReadOnlyContentPack(LITContentPack);
        internal static ContentPack LITContentPack { get; } = new ContentPack();

        internal static ParallelCoroutineHelper _parallelPreLoadDispatchers = new ParallelCoroutineHelper();
        internal static Func<IEnumerator>[] _loadDispatchers;
        internal static ParallelCoroutineHelper _parallelPostLoadDispatchers = new ParallelCoroutineHelper();

        private static Action[] _fieldAssignDispatchers;

        public IEnumerator LoadStaticContentAsync(LoadStaticContentAsyncArgs args)
        {
            var enumerator = LITAssets.Initialize();
            while (enumerator.MoveNext()) yield return null;

            _parallelPreLoadDispatchers.Start();
            while (!_parallelPreLoadDispatchers.IsDone()) yield return null;

            for(int i = 0; i < _loadDispatchers.Length; i++)
            {
                args.ReportProgress(Util.Remap(i + 1, 0f, _loadDispatchers.Length, 0.1f, 0.2f));
                enumerator = _loadDispatchers[i]();

                while (enumerator.MoveNext()) yield return null;
            }

            _parallelPostLoadDispatchers.Start();
            while (!_parallelPostLoadDispatchers.IsDone()) yield return null;

            for (int i = 0; i < _fieldAssignDispatchers.Length; i++)
            {
                args.ReportProgress(Util.Remap(i + 1, 0f, _loadDispatchers.Length, 0.1f, 0.2f));
                _fieldAssignDispatchers[i]();
                yield return null;
            }
        }

        public IEnumerator FinalizeAsync(FinalizeAsyncArgs args)
        {
            args.ReportProgress(1f);
            yield break;
        }

        public IEnumerator GenerateContentPackAsync(GetContentPackAsyncArgs args)
        {
            ContentPack.Copy(LITContentPack, args.output);
            yield break;
        }

        private void AddSelf(ContentManager.AddContentPackProviderDelegate addContentPackProvider)
        {
            addContentPackProvider(this);
        }

        private IEnumerator AddExpansionDef()
        {
            LITAssetRequest<ExpansionDef> expansionDefRequest = LITAssets.LoadAssetAsync<ExpansionDef>("LITExpansionDef", LITBundle.Main);

            expansionDefRequest.StartLoad();
            while (!expansionDefRequest.IsComplete)
                yield return null;

            LITContentPack.expansionDefs.AddSingle(expansionDefRequest.Asset);
        }

        internal LITContent()
        {
            ContentManager.collectContentPackProviders += AddSelf;

            LITAssets.AssetsAvailability.CallWhenAvailable(() =>
            {
                _parallelPreLoadDispatchers.Add(LITConfig.RegisterToModSettingsManager);
                _parallelPreLoadDispatchers.Add(AddExpansionDef);
            });

            LITMain main = LITMain.Instance;
            _loadDispatchers = new Func<IEnumerator>[]
            {
                () =>
                {
                    EquipmentModule.AddProvider(main, ContentUtil.CreateContentPieceProvider<EquipmentDef>(main, LITContentPack));
                    return EquipmentModule.InitialzeEquipments(main);
                },
                () =>
                {
                    ItemModule.AddProvider(main, ContentUtil.CreateContentPieceProvider<ItemDef>(main, LITContentPack));
                    return ItemModule.InitializeItems(main);
                },
                () =>
                {
                    CharacterModule.AddProvider(main, ContentUtil.CreateGameObjectContentPieceProvider<CharacterBody>(main, LITContentPack));
                    return CharacterModule.InitializeCharacters(main);
                }
            };

            _fieldAssignDispatchers = new Action[]
            {
                () =>
                {
                    ContentUtil.PopulateTypeFields(typeof(Buffs), LITContentPack.buffDefs);
                },
                () =>
                {
                    ContentUtil.PopulateTypeFields(typeof(Elites), LITContentPack.eliteDefs);
                },
                () =>
                {
                    ContentUtil.PopulateTypeFields(typeof(Equipments), LITContentPack.equipmentDefs);
                },
                () =>
                {
                    ContentUtil.PopulateTypeFields(typeof(Items), LITContentPack.itemDefs);
                },
                () =>
                {
                    ContentUtil.PopulateTypeFields(typeof(Artifacts), LITContentPack.artifactDefs);
                }
            };


            LITHooks.Init();
        }

        public static class Buffs
        {
            public static BuffDef bdAffixBlighted;
            public static BuffDef bdAffixBlightedFake;
            public static BuffDef bdAffixFrenzied;
            public static BuffDef bdAffixLeeching;
            public static BuffDef bdAffixVolatile;
            public static BuffDef bdDiceArmor;
            public static BuffDef bdDiceAtk;
            public static BuffDef bdDiceCrit;
            public static BuffDef bdDiceLuck;
            public static BuffDef bdDiceMove;
            public static BuffDef bdDiceRegen;
            public static BuffDef bdFieldGeneratorPassive;
            public static BuffDef bdGoldenGun;
            public static BuffDef bdGuardiansHeartBuff;
            public static BuffDef bdHitListBuff;
            public static BuffDef bdHitListMarked;
            public static BuffDef bdMeds;
            public static BuffDef bdMitosisBuff;
            public static BuffDef bdNuggetRegen;
            public static BuffDef bdPillaging;
            public static BuffDef bdRepulsionArmorActive;
            public static BuffDef bdRepulsionArmorCD;
            public static BuffDef bdRootRegen;
            public static BuffDef bdShackled;
            public static BuffDef bdThalliumPoison;
            public static BuffDef bdTimeStop;
            public static BuffDef bdTimeStopDebuff;
            public static BuffDef bdToxin;
            public static BuffDef bdToxinCooldown;
            public static BuffDef bdToxinReady;
            public static BuffDef bdHiddenCritDamage;
        }

        public static class Elites
        {
            public static EliteDef Blighted;
            public static EliteDef Frenzied;
            public static EliteDef FrenziedHonor;
            public static EliteDef Volatile;
            public static EliteDef VolatileHonor;
        }

        public static class Equipments
        {
            public static EquipmentDef AffixBlighted;
            public static EquipmentDef AffixFrenzied;
            public static EquipmentDef AffixLeeching;
            public static EquipmentDef AffixVolatile;
            public static EquipmentDef FieldGenerator;
            public static EquipmentDef FieldGeneratorUsed;
            public static EquipmentDef GiganticAmethyst;
            public static EquipmentDef GoldPlatedBomb;
            public static EquipmentDef Thqwib;
            public static EquipmentDef Prescriptions;
            public static EquipmentDef UnstableWatch;
        }

        public static class Items
        {
            public static ItemDef ArmsRaceDroneMods;
            public static ItemDef ArmsRace;
            public static ItemDef BeckoningCat;
            public static ItemDef BitterRoot;
            public static ItemDef BlessedDice;
            public static ItemDef EnergyCell;
            public static ItemDef CoolantCell;
            public static ItemDef GoldenGun;
            public static ItemDef GuardiansHeart;
            public static ItemDef LifeSavings;
            public static ItemDef LifeSavingsUsed;
            public static ItemDef MeatNugget;
            public static ItemDef MuConstruct;
            public static ItemDef MysteriousVial;
            public static ItemDef PhotonCannon;
            public static ItemDef PrisonShackles;
            public static ItemDef RapidMitosis;
            public static ItemDef Chestplate;
            public static ItemDef RustyJetpack;
            public static ItemDef SmartShopper;
            public static ItemDef TelescopicSight;
            public static ItemDef Thallium;
            public static ItemDef Lopper;
            public static ItemDef WickedRingNew;
            public static ItemDef FireShield;
            public static ItemDef BeatingEmbryo;
            public static ItemDef FireBoots;
            public static ItemDef HitList;
            public static ItemDef RazorPenny;
            public static ItemDef TheToxin;
            public static ItemDef LockedJewel;
        }

        public static class Artifacts
        {
            public static ArtifactDef Prestige;
        }
    }
}
