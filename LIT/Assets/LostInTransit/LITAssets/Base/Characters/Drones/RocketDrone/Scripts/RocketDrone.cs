using MSU;
using RoR2;
using RoR2.ContentManagement;
using RoR2.Projectile;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace LostInTransit.Characters
{
    public class RocketDrone : LITInteractable, IContentPackModifier
    {
        public override GameObject InteractablePrefab => _interactablePrefab;
        public override InteractableCardProvider CardProvider => _cardProvider;
        public GameObject CharacterPrefab => _characterPrefab;

        private GameObject _characterPrefab;
        private AssetCollection _assetCollection;
        private GameObject _interactablePrefab;
        private InteractableCardProvider _cardProvider;

        private static GameObject rocketPrefab;

        private SummonMasterBehavior smb;
        private CharacterMaster cm;
        private GameObject bodyPrefab;
        private AkEvent[] droneAkEvents;

        public override void Initialize()
        {
            smb = InteractablePrefab.GetComponent<SummonMasterBehavior>();
            cm = smb.masterPrefab.GetComponent<CharacterMaster>();
            bodyPrefab = cm.bodyPrefab;

            On.EntityStates.Drone.DeathState.OnImpactServer += SpawnInteractableCorpse;

            var droneBody = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Drones/Drone1Body.prefab").WaitForCompletion();

            droneAkEvents = droneBody.GetComponents<AkEvent>();

            foreach (AkEvent akEvent in droneAkEvents)
            {
                var akEventType = akEvent.GetType();
                var newComponent = bodyPrefab.AddComponent(akEventType);

                var fields = akEventType.GetFields();

                foreach (var field in fields)
                {
                    var value = field.GetValue(akEvent);
                    field.SetValue(newComponent, value);
                }
            }
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public override IEnumerator LoadContentAsync()
        {
            var request = LITAssets.LoadAssetAsync<AssetCollection>("acRocketDrone", LITBundle.Characters);

            request.StartLoad();
            while (!request.IsComplete)
                yield return null;

            _assetCollection = request.Asset;
            _characterPrefab = _assetCollection.FindAsset<GameObject>("RocketDroneBody");
            _cardProvider = _assetCollection.FindAsset<InteractableCardProvider>("msidcRocketDrone");
            _interactablePrefab = _assetCollection.FindAsset<GameObject>("RocketDroneBroken");

            /*rocketPrefab = Object.Instantiate(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Drones/PaladinRocket.prefab").WaitForCompletion());
            ProjectileController pc = rocketPrefab.GetComponent<ProjectileController>();
            if (pc != null)
            {
                pc.ghostPrefab = _assetCollection.FindAsset<GameObject>("RocketGhost");
                EntityStates.RocketDrone.RocketDroneFire.projectilePrefab = rocketPrefab;
            }*/
        }

        public void ModifyContentPack(ContentPack contentPack)
        {
            contentPack.AddContentFromAssetCollection(_assetCollection);
        }

        private void SpawnInteractableCorpse(On.EntityStates.Drone.DeathState.orig_OnImpactServer orig, EntityStates.Drone.DeathState self, Vector3 contactPoint)
        {
            if (self.characterBody.bodyIndex == BodyCatalog.FindBodyIndexCaseInsensitive(_characterPrefab.name))
            {
                DirectorPlacementRule placementRule = new DirectorPlacementRule
                {
                    placementMode = DirectorPlacementRule.PlacementMode.Direct,
                    position = contactPoint
                };
                GameObject gameObject = DirectorCore.instance.TrySpawnObject(new DirectorSpawnRequest(CardProvider.BuildSpawnCardSet().FirstOrDefault(), placementRule, new Xoroshiro128Plus(0UL)));
                if (gameObject)
                {
                    PurchaseInteraction component = gameObject.GetComponent<PurchaseInteraction>();
                    if (component && component.costType == CostTypeIndex.Money)
                    {
                        component.Networkcost = Run.instance.GetDifficultyScaledCost(component.cost);
                    }
                }

            }
            else
            {
                orig(self, contactPoint);
            }
        }
    }
}
