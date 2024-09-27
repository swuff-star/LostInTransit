using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MSU;
using RoR2;
using RoR2.ContentManagement;
using R2API;
using LostInTransit.Components;
using UnityEngine.Networking;

namespace LostInTransit.Characters
{
    public sealed class Drifter : LITSurvivor, IContentPackModifier
    {
        public override SurvivorDef survivorDef => _survivorDef;
        private SurvivorDef _survivorDef;

        public override NullableRef<GameObject> masterPrefab => _masterPrefab;
        private GameObject _masterPrefab;

        public override GameObject characterPrefab => _characterPrefab;
        private GameObject _characterPrefab;

        private AssetCollection _assetCollection;

        public static DamageAPI.ModdedDamageType executeToScrap { get; private set; }
        private static GameObject _scrapPickup;
        private static float executeScrapProcChance = 20f;

        public static DamageAPI.ModdedDamageType scrapOnHit10 { get; private set; }
        public static DamageAPI.ModdedDamageType scrapOnHit20 { get; private set; }
        public static DamageAPI.ModdedDamageType scrapOnHit30 { get; private set; }
        public static GameObject drifterScrapProjectile { get; private set; }

        public override void Initialize()
        {
            executeToScrap = DamageAPI.ReserveDamageType();
            scrapOnHit10 = DamageAPI.ReserveDamageType();
            scrapOnHit20 = DamageAPI.ReserveDamageType();
            scrapOnHit30 = DamageAPI.ReserveDamageType();

            GlobalEventManager.onServerDamageDealt += HandleScrapDamageTypes;
        }

        private void HandleScrapDamageTypes(DamageReport report)
        {
            var victimBody = report.victimBody;
            var attackerBody = report.attackerBody;
            var damageInfo = report.damageInfo;
            if (DamageAPI.HasModdedDamageType(damageInfo, executeToScrap))
            {
                if (victimBody.healthComponent.combinedHealth < victimBody.healthComponent.fullCombinedHealth * 0.2f)
                {
                    victimBody.healthComponent.Suicide();
                    DrifterScrapComponent dsc = attackerBody.GetComponent<DrifterScrapComponent>();
                    if (dsc != null)
                    {
                        dsc.AddScrap(3f);
                    }
                }
                else if (Util.CheckRoll(executeScrapProcChance))
                {
                    GameObject scrap = Object.Instantiate(_scrapPickup, victimBody.transform.position, victimBody.transform.rotation);
                    scrap.GetComponent<TeamFilter>().teamIndex = report.attackerTeamIndex;

                    NetworkServer.Spawn(scrap);
                }
            }
            if (DamageAPI.HasModdedDamageType(damageInfo, scrapOnHit10))
            {
                if (Util.CheckRoll(10))
                {
                    GameObject scrap = Object.Instantiate(_scrapPickup, victimBody.transform.position, victimBody.transform.rotation);
                    scrap.GetComponent<TeamFilter>().teamIndex = report.attackerTeamIndex;

                    NetworkServer.Spawn(scrap);
                }
            }
            if (DamageAPI.HasModdedDamageType(damageInfo, scrapOnHit20))
            {
                if (Util.CheckRoll(20))
                {
                    GameObject scrap = Object.Instantiate(_scrapPickup, victimBody.transform.position, victimBody.transform.rotation);
                    scrap.GetComponent<TeamFilter>().teamIndex = report.attackerTeamIndex;

                    NetworkServer.Spawn(scrap);
                }
            }
            if (DamageAPI.HasModdedDamageType(damageInfo, scrapOnHit30))
            {
                if (Util.CheckRoll(30))
                {
                    GameObject scrap = Object.Instantiate(_scrapPickup, victimBody.transform.position, victimBody.transform.rotation);
                    scrap.GetComponent<TeamFilter>().teamIndex = report.attackerTeamIndex;

                    NetworkServer.Spawn(scrap);
                }
            }
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public override IEnumerator LoadContentAsync()
        {
            LITAssetRequest<AssetCollection> drifterAssetCollection = LITAssets.LoadAssetAsync<AssetCollection>("acDrifter", LITBundle.Characters);

            drifterAssetCollection.StartLoad();
            while(!drifterAssetCollection.IsComplete)
                yield return null;

            _assetCollection = drifterAssetCollection.Asset;

            _survivorDef = _assetCollection.FindAsset<SurvivorDef>("SurvivorDrifter");
            _characterPrefab = _assetCollection.FindAsset<GameObject>("DrifterBody");
            _scrapPickup = _assetCollection.FindAsset<GameObject>("ScrapPickup");
        }

        public void ModifyContentPack(ContentPack contentPack)
        {
            contentPack.AddContentFromAssetCollection(_assetCollection);
        }
    }
}
