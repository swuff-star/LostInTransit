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
#if DEBUG
    public sealed class Drifter : LITSurvivor, IContentPackModifier
    {
        public override SurvivorDef SurvivorDef => _survivorDef;
        private SurvivorDef _survivorDef;

        public override NullableRef<GameObject> MasterPrefab => throw new System.NotImplementedException();
        private GameObject _masterPrefab;

        public override GameObject CharacterPrefab => throw new System.NotImplementedException();
        private GameObject _characterPrefab;

        private AssetCollection _assetCollection;

        public static DamageAPI.ModdedDamageType ExecuteToScrap { get; private set; }
        private static GameObject _scrapPickup;
        private static float executeScrapProcChance = 20f;

        public static DamageAPI.ModdedDamageType ScrapOnHit10 { get; private set; }
        public static DamageAPI.ModdedDamageType ScrapOnHit20 { get; private set; }
        public static DamageAPI.ModdedDamageType ScrapOnHit30 { get; private set; }
        public static GameObject DrifterScrapProjectile { get; private set; }

        public override void Initialize()
        {
            ExecuteToScrap = DamageAPI.ReserveDamageType();
            ScrapOnHit10 = DamageAPI.ReserveDamageType();
            ScrapOnHit20 = DamageAPI.ReserveDamageType();
            ScrapOnHit30 = DamageAPI.ReserveDamageType();

            GlobalEventManager.onServerDamageDealt += HandleScrapDamageTypes;
        }

        private void HandleScrapDamageTypes(DamageReport report)
        {
            var victimBody = report.victimBody;
            var attackerBody = report.attackerBody;
            var damageInfo = report.damageInfo;
            if (DamageAPI.HasModdedDamageType(damageInfo, ExecuteToScrap))
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
            if (DamageAPI.HasModdedDamageType(damageInfo, ScrapOnHit10))
            {
                if (Util.CheckRoll(10))
                {
                    GameObject scrap = Object.Instantiate(_scrapPickup, victimBody.transform.position, victimBody.transform.rotation);
                    scrap.GetComponent<TeamFilter>().teamIndex = report.attackerTeamIndex;

                    NetworkServer.Spawn(scrap);
                }
            }
            if (DamageAPI.HasModdedDamageType(damageInfo, ScrapOnHit20))
            {
                if (Util.CheckRoll(20))
                {
                    GameObject scrap = Object.Instantiate(_scrapPickup, victimBody.transform.position, victimBody.transform.rotation);
                    scrap.GetComponent<TeamFilter>().teamIndex = report.attackerTeamIndex;

                    NetworkServer.Spawn(scrap);
                }
            }
            if (DamageAPI.HasModdedDamageType(damageInfo, ScrapOnHit30))
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
            return false;
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
        }

        public void ModifyContentPack(ContentPack contentPack)
        {
            contentPack.AddContentFromAssetCollection(_assetCollection);
        }
    }
#endif
}
