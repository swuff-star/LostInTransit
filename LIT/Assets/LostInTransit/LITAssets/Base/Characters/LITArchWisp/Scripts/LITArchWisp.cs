using MSU;
using R2API;
using RoR2;
using RoR2.ContentManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LostInTransit.Characters
{
    public class LITArchWisp : LITMonster, IContentPackModifier
    {
        public override NullableRef<MonsterCardProvider> cardProvider => _cardProvider;
        private MonsterCardProvider _cardProvider;

        public override NullableRef<DirectorCardHolderExtended> dissonanceCard => _dissonanceCard;
        private DirectorCardHolderExtended _dissonanceCard;

        public override NullableRef<GameObject> masterPrefab => _masterPrefab;
        private GameObject _masterPrefab;

        public override GameObject characterPrefab => _characterPrefab;
        private GameObject _characterPrefab;
        private AssetCollection _assetcollection;

        public override void Initialize()
        {
            _dissonanceCard = new DirectorCardHolderExtended
            {
                Card = new DirectorCard
                {
                    spawnDistance = DirectorCore.MonsterSpawnDistance.Standard,
                    spawnCard = _assetcollection.FindAsset<CharacterSpawnCard>("cscLITArchWisp"),
                    preventOverhead = true,
                    selectionWeight = 1,
                },
                MonsterCategory = DirectorAPI.MonsterCategory.Minibosses,
                InteractableCategory = DirectorAPI.InteractableCategory.Invalid,
            };
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public override IEnumerator LoadContentAsync()
        {
            var request = LITAssets.LoadAssetAsync<AssetCollection>("acArchWisp", LITBundle.Characters);

            request.StartLoad();
            while (!request.IsComplete)
                yield return null;

            _assetcollection = request.Asset;
            _characterPrefab = _assetcollection.FindAsset<GameObject>("LITArchWispBody");
            _masterPrefab = _assetcollection.FindAsset<GameObject>("LITArchWispMaster");
            _cardProvider = _assetcollection.FindAsset<MonsterCardProvider>("mcpLITArchWisp");
        }

        public void ModifyContentPack(ContentPack contentPack)
        {
            contentPack.AddContentFromAssetCollection(_assetcollection);
        }
    }
}