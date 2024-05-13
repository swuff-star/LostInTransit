using MSU;
using RoR2;
using RoR2.ContentManagement;
using System.Collections;
using UnityEngine;

namespace LostInTransit.Characters
{
    public class LaserDrone : LITInteractable, IContentPackModifier
    {
        public override GameObject InteractablePrefab => _interactablePrefab;
        public override InteractableCardProvider CardProvider => _cardProvider;
        public GameObject CharacterPrefab => _characterPrefab;

        private GameObject _characterPrefab;
        private AssetCollection _assetCollection;
        private GameObject _interactablePrefab;
        private InteractableCardProvider _cardProvider;

        public override void Initialize()
        {
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public override IEnumerator LoadContentAsync()
        {
            var request = LITAssets.LoadAssetAsync<AssetCollection>("acLaserDrone", LITBundle.Characters);

            request.StartLoad();
            while (!request.IsComplete)
                yield return null;

            _assetCollection = request.Asset;
            _characterPrefab = _assetCollection.FindAsset<GameObject>("LaserDroneBody");
            _cardProvider = _assetCollection.FindAsset<InteractableCardProvider>("msidcLaserDrone");
            _interactablePrefab = _assetCollection.FindAsset<GameObject>("LaserDroneBroken");
        }

        public void ModifyContentPack(ContentPack contentPack)
        {
            contentPack.AddContentFromAssetCollection(_assetCollection);
        }
    }
}
