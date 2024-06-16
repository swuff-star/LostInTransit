using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MSU;
using RoR2;
using RoR2.ContentManagement;

namespace LostInTransit.Characters
{
#if DEBUG
    public sealed class Robomando : LITSurvivor, IContentPackModifier
    {
        public override SurvivorDef SurvivorDef => _survivorDef;
        private SurvivorDef _survivorDef;

        public override NullableRef<GameObject> MasterPrefab => _masterPrefab;
        private GameObject _masterPrefab;

        public override GameObject CharacterPrefab => _characterPrefab;
        private GameObject _characterPrefab;

        private AssetCollection _robomandoAssets;
        public override void Initialize()
        {
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public override IEnumerator LoadContentAsync()
        {
            LITAssetRequest<AssetCollection> request = LITAssets.LoadAssetAsync<AssetCollection>("acRobomando", LITBundle.Characters);

            request.StartLoad();
            while (!request.IsComplete)
                yield return null;

            _robomandoAssets = request.Asset;

            _characterPrefab = _robomandoAssets.FindAsset<GameObject>("RobomandoBody");
        }

        public void ModifyContentPack(ContentPack contentPack)
        {
            contentPack.AddContentFromAssetCollection(_robomandoAssets);
        }
    }
#endif
}
