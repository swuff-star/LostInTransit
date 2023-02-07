using RoR2.ContentManagement;
using R2API.ScriptableObjects;
using R2API.ContentManagement;
using Moonstorm.Loaders;
using System;
using LostInTransit.Modules;
using System.Linq;
using RoR2;
using UnityEngine;

namespace LostInTransit
{
    public class LITContent : ContentLoader<LITContent>
    {
        public static class Buffs
        {
            public static BuffDef AffixBlighted;
            public static BuffDef AffixFrenzied;
            public static BuffDef AffixLeeching;
            public static BuffDef AffixVolatile;
            public static BuffDef DiceArmor;
            public static BuffDef DiceAtk;
            public static BuffDef DiceCrit;
            public static BuffDef DiceLuck;
            public static BuffDef DiceMove;
            public static BuffDef DiceRegen;
            public static BuffDef FieldGeneratorPassive;
            public static BuffDef GoldenGun;
            public static BuffDef GuardiansHeartBuff;
            public static BuffDef HitListBuff;
            public static BuffDef HitListMarked;
            public static BuffDef Meds;
            public static BuffDef MitosisBuff;
            public static BuffDef NuggetRegen;
            public static BuffDef Pillaging;
            public static BuffDef RepulsionArmorActive;
            public static BuffDef RepulsionArmorCD;
            public static BuffDef RootRegen;
            public static BuffDef Shackled;
            public static BuffDef ThalliumPoison;
        }

        public static class Elites
        {
            public static EliteDef Blighted;
            public static EliteDef Frenzied;
            public static EliteDef Leeching;
            public static EliteDef Volatile;
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
        }

        public static class Items
        {
            public static ItemDef ArmsRaceDroneMods;
            public static ItemDef ArmsRace;
            public static ItemDef BeckoningCat;
            public static ItemDef BitterRoot;
            public static ItemDef BlessedDice;
            public static ItemDef EnergyCell;
            public static ItemDef GoldenGun;
            public static ItemDef GuardiansHeart;
            public static ItemDef LifeSavings;
            public static ItemDef LifeSavingsUsed;
            public static ItemDef MeatNugget;
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
        }
        public override string identifier => LITMain.GUID;

        public override R2APISerializableContentPack SerializableContentPack { get; protected set; } = LITAssets.LoadAsset<R2APISerializableContentPack>("ContentPack");
        public override Action[] LoadDispatchers { get; protected set; }
        public override Action[] PopulateFieldsDispatchers { get; protected set; }

        public override void Init()
        {
            base.Init();
            LoadDispatchers = new Action[]
            {
                delegate
                {
                    new LostInTransit.Buffs.Buffs().Initialize();
                },
                delegate
                {
                    new DamageTypes.DamageTypes().Initialize();
                },
                delegate
                {
                    new Modules.Projectiles().Initialize();
                },
                delegate
                {
                    new Modules.Equipments().Initialize();
                },
                delegate
                {
                    new Modules.Items().Initialize();
                },
                delegate
                {
                    new Modules.Elites().Initialize();
                },
                delegate
                {
                    new Characters.Characters().Initialize();
                },
                delegate
                {
                    SerializableContentPack.entityStateTypes = typeof(LITContent).Assembly.GetTypes()
                        .Where(type => typeof(EntityStates.EntityState).IsAssignableFrom(type))
                        .Select(type => new EntityStates.SerializableEntityStateType(type))
                        .ToArray();
                },
                delegate
                {
                    SerializableContentPack.effectPrefabs = LITAssets.LoadAllAssetsOfType<GameObject>().Where(go => go.GetComponent<EffectComponent>()).ToArray();
                },
                delegate
                {
                    LITAssets.Instance.SwapMaterialShaders();
                    LITAssets.Instance.FinalizeCopiedMaterials();
                }
            };

            PopulateFieldsDispatchers = new Action[]
            {
                delegate
                {
                    PopulateTypeFields(typeof(Buffs), ContentPack.buffDefs);
                },
                delegate
                {
                    PopulateTypeFields(typeof(Elites), ContentPack.eliteDefs);
                },
                delegate
                {
                    PopulateTypeFields(typeof(Equipments), ContentPack.equipmentDefs);
                },
                delegate
                {
                    PopulateTypeFields(typeof(Items), ContentPack.itemDefs);
                }
            };

            LITHooks.Init();
        }
    }
    /*internal class LITContent : IContentPackProvider
    {
        public static SerializableContentPack serializableContentPack;
        internal ContentPack contentPack;
        public string identifier => LITMain.GUID;

        public void Initialize()
        {
            contentPack = serializableContentPack.CreateContentPack();
            contentPack.identifier = identifier;
            ContentManager.collectContentPackProviders += ContentManager_collectContentPackProviders;
        }

        private void ContentManager_collectContentPackProviders(ContentManager.AddContentPackProviderDelegate addContentPackProvider)
        {
            addContentPackProvider(this);
        }

        public System.Collections.IEnumerator LoadStaticContentAsync(LoadStaticContentAsyncArgs args)
        {
            args.ReportProgress(1f);
            yield break;
        }

        public System.Collections.IEnumerator GenerateContentPackAsync(GetContentPackAsyncArgs args)
        {
            ContentPack.Copy(contentPack, args.output);
            args.ReportProgress(1f);
            yield break;
        }

        public System.Collections.IEnumerator FinalizeAsync(FinalizeAsyncArgs args)
        {
            args.ReportProgress(1f);
            yield break;
        }
    }*/
}
