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
        public override NullableRef<MonsterCardProvider> CardProvider => null;
        public override NullableRef<DirectorAPI.DirectorCardHolder> DissonanceCard => null;

        public override NullableRef<GameObject> MasterPrefab => null;

        public override GameObject CharacterPrefab => _characterPrefab;
        private GameObject _characterPrefab;
        private AssetCollection _assetcollection;

        public override void Initialize()
        {
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
        }

        public void ModifyContentPack(ContentPack contentPack)
        {
            contentPack.AddContentFromAssetCollection(_assetcollection);
        }
    }
}