using MSU;
using RoR2;
using RoR2.ContentManagement;
using RoR2.Items;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace LostInTransit.Interactables
{
    public class Upgrader : LITInteractable, IContentPackModifier
    {
        public override GameObject InteractablePrefab => _interactablePrefab;
        public override NullableRef<InteractableCardProvider> CardProvider => _cardProvider;

        private AssetCollection _assetCollection;
        private GameObject _interactablePrefab;
        private InteractableCardProvider _cardProvider;
        private static GameObject _bodyOrb;

        public static Dictionary<string, GameObject> droneUpgrades = new Dictionary<string, GameObject>();

        public static CostTypeDef droneCostDef;
        public static int droneCostIndex;

        //private static GameObject bodyOrb;

        public override void Initialize()
        {
            CostTypeCatalog.modHelper.getAdditionalEntries += AddDroneCostType;

            var interactionToken = InteractablePrefab.AddComponent<UpgraderInteractionToken>();
            interactionToken.PurchaseInteraction = InteractablePrefab.GetComponent<PurchaseInteraction>();
            SetupDroneUpgrades();
        }

        private void SetupDroneUpgrades()
        {
            droneUpgrades.Add("DRONE_GUNNER_BODY_NAME", Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Drones/Drone1Master.prefab").WaitForCompletion());
            droneUpgrades.Add("DRONE_HEALING_BODY_NAME", Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Drones/Drone2Master.prefab").WaitForCompletion());
            droneUpgrades.Add("FLAMEDRONE_BODY_NAME", Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Drones/FlameDroneMaster.prefab").WaitForCompletion());
            droneUpgrades.Add("DRONE_MISSILE_BODY_NAME", Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Drones/DroneMissileMaster.prefab").WaitForCompletion());
            droneUpgrades.Add("LIT_DRONE_LASER_NAME", LITAssets.LoadAsset<GameObject>("LaserDroneMaster", LITBundle.Characters));
            droneUpgrades.Add("LIT_DRONE_ATTACK_NAME", LITAssets.LoadAsset<GameObject>("GoldAttackDroneMaster", LITBundle.Characters));
            droneUpgrades.Add("DRONE_EMERGENCY_BODY_NAME", LITAssets.LoadAsset<GameObject>("GoldEmergencyDroneMaster", LITBundle.Characters));
            droneUpgrades.Add("LIT_DRONE_BLAZE_NAME", LITAssets.LoadAsset<GameObject>("GoldBlazeDroneMaster", LITBundle.Characters));
            droneUpgrades.Add("LIT_DRONE_ROCKET_NAME", LITAssets.LoadAsset<GameObject>("GoldRocketDroneMaster", LITBundle.Characters));
            droneUpgrades.Add("LIT_DRONE_BEAM_NAME", LITAssets.LoadAsset<GameObject>("GoldBeamDroneMaster", LITBundle.Characters));
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public override IEnumerator LoadContentAsync()
        {
            var request = LITAssets.LoadAssetAsync<AssetCollection>("acUpgrader", LITBundle.Characters);

            request.StartLoad();
            while (!request.IsComplete)
                yield return null;

            _assetCollection = request.Asset;
            _cardProvider = _assetCollection.FindAsset<InteractableCardProvider>("msidcUpgrader");
            _interactablePrefab = _assetCollection.FindAsset<GameObject>("UpgraderPrefab");
            _bodyOrb = _assetCollection.FindAsset<GameObject>("CharacterBodyOrbEffect");
        }

        public void ModifyContentPack(ContentPack contentPack)
        {
            contentPack.AddContentFromAssetCollection(_assetCollection);
        }

        public class UpgraderInteractionToken : MonoBehaviour
        {
            public CharacterBody LastActivator;
            public PurchaseInteraction PurchaseInteraction;

            public void Start()
            {
                if (NetworkServer.active && Run.instance)
                {
                    PurchaseInteraction.SetAvailableTrue();
                }
                PurchaseInteraction.costType = (CostTypeIndex)droneCostIndex;
                PurchaseInteraction.onPurchase.AddListener(DronePurchaseAttempt);
            }

            public void DronePurchaseAttempt(Interactor interactor)
            {
                if (!interactor) { return; }

                var body = interactor.GetComponent<CharacterBody>();
                if (body && body.master)
                {
                    if (NetworkServer.active)
                    {
                        LastActivator = body;
                    }
                }
            }
        }

        private void AddDroneCostType(List<CostTypeDef> obj)
        {
            droneCostDef = new CostTypeDef();
            droneCostDef.costStringFormatToken = "LIT_COST_THREE_DRONES";
            droneCostDef.isAffordable = new CostTypeDef.IsAffordableDelegate(DroneCostTypeHelper.IsAffordable);
            droneCostDef.payCost = new CostTypeDef.PayCostDelegate(DroneCostTypeHelper.PayCost);
            droneCostDef.colorIndex = ColorCatalog.ColorIndex.Interactable;
            droneCostDef.saturateWorldStyledCostString = true;
            droneCostDef.darkenWorldStyledCostString = false;
            droneCostIndex = CostTypeCatalog.costTypeDefs.Length + obj.Count;
            obj.Add(droneCostDef);
        }

        private static class DroneCostTypeHelper
        {
            public static void PayCost(CostTypeDef costTypeDef, CostTypeDef.PayCostContext context)
            {
                CharacterBody body = context.activator.GetComponent<CharacterBody>();

                MinionOwnership.MinionGroup minionGroup = MinionOwnership.MinionGroup.FindGroup(body.master.netId);
                if (minionGroup != null)
                {
                    var members = minionGroup.members;
                    List<CharacterMaster> drone1Minions = new List<CharacterMaster>();
                    List<CharacterMaster> drone2Minions = new List<CharacterMaster>();
                    List<CharacterMaster> flameDroneMinions = new List<CharacterMaster>();
                    List<CharacterMaster> missileDroneMinions = new List<CharacterMaster>();
                    List<CharacterMaster> laserDroneMinions = new List<CharacterMaster>();

                    List<CharacterMaster> attackDroneMinions = new List<CharacterMaster>();
                    List<CharacterMaster> emergencyDroneMinions = new List<CharacterMaster>();
                    List<CharacterMaster> blazeDroneMinions = new List<CharacterMaster>();
                    List<CharacterMaster> rocketDroneMinions = new List<CharacterMaster>();
                    List<CharacterMaster> beamDroneMinions = new List<CharacterMaster>();

                    foreach (var drone in members)
                    {
                        if (drone)
                        {
                            CharacterMaster master = drone.GetComponent<CharacterMaster>();
                            if (master)
                            {
                                var droneBody = master.GetBody();
                                if (droneBody)
                                {
                                    if ((droneBody.bodyFlags & CharacterBody.BodyFlags.Mechanical) > CharacterBody.BodyFlags.None)
                                    {
                                        switch (droneBody.baseNameToken)
                                        {
                                            default:
                                                break;
                                            case "DRONE_GUNNER_BODY_NAME":
                                                drone1Minions.Add(master);
                                                break;
                                            case "DRONE_HEALING_BODY_NAME":
                                                drone2Minions.Add(master);
                                                break;
                                            case "FLAMEDRONE_BODY_NAME":
                                                flameDroneMinions.Add(master);
                                                break;
                                            case "DRONE_MISSILE_BODY_NAME":
                                                missileDroneMinions.Add(master);
                                                break;
                                            case "LIT_DRONE_LASER_NAME":
                                                laserDroneMinions.Add(master);
                                                break;
                                            case "LIT_DRONE_ATTACK_NAME":
                                                attackDroneMinions.Add(master);
                                                break;
                                            case "DRONE_EMERGENCY_BODY_NAME":
                                                emergencyDroneMinions.Add(master);
                                                break;
                                            case "LIT_DRONE_BLAZE_NAME":
                                                blazeDroneMinions.Add(master);
                                                break;
                                            case "LIT_DRONE_ROCKET_NAME":
                                                rocketDroneMinions.Add(master);
                                                break;
                                            case "LIT_DRONE_BEAM_NAME":
                                                beamDroneMinions.Add(master);
                                                break;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    List<CharacterMaster> validMinions = new List<CharacterMaster>();

                    //if you're seeing this and thinking "that's so fucking stupid" please for the love of god suggest a better way to write this
                    /*if (drone1Minions.Count >= 3)
                        validMinions = drone1Minions;

                    if (drone2Minions.Count >= 3)
                        validMinions = drone2Minions;

                    if (flameDroneMinions.Count >= 3)
                        validMinions = flameDroneMinions;

                    if (missileDroneMinions.Count >= 3)
                        validMinions = missileDroneMinions;

                    if (laserDroneMinions.Count >= 3)
                        validMinions = laserDroneMinions;

                    if (attackDroneMinions.Count >= 3)
                        validMinions = attackDroneMinions;

                    if (emergencyDroneMinions.Count >= 3)
                        validMinions = emergencyDroneMinions;

                    if (blazeDroneMinions.Count >= 3)
                        validMinions = blazeDroneMinions;

                    if (rocketDroneMinions.Count >= 3)
                        validMinions = rocketDroneMinions;

                    if (beamDroneMinions.Count >= 3)
                        validMinions = beamDroneMinions;*/

                    List<List<CharacterMaster>> masterLists = new List<List<CharacterMaster>>()
                    {
                        drone1Minions,
                        drone2Minions,
                        flameDroneMinions,
                        missileDroneMinions,
                        laserDroneMinions,
                        attackDroneMinions,
                        emergencyDroneMinions,
                        blazeDroneMinions,
                        rocketDroneMinions,
                        beamDroneMinions
                    };

                    foreach (var minionList in masterLists)
                    {
                        if (minionList.Count >= 3)
                        {
                            validMinions = minionList;
                            break;
                        }
                    }

                    MultiShopCardUtils.OnNonMoneyPurchase(context);

                    var model = context.purchasedObject;

                    var esm = model.GetComponent<EntityStateMachine>();
                    if (esm)
                    {
                        EntityStates.Upgrader.Action nextState = new EntityStates.Upgrader.Action();
                        GameObject masterPrefab = null;
                        droneUpgrades.TryGetValue(validMinions[0].GetBody().baseNameToken, out masterPrefab);
                        if (masterPrefab == null)
                            return;
                        nextState.droneMasterPrefab = masterPrefab;
                        nextState.summonerBody = body.gameObject;
                        esm.SetNextState(nextState);
                    }

                    Debug.Log("valid minions: " + validMinions.Count);

                    for (int i = 0; i < 3; i++)
                    {
                        Debug.Log(i + " drone #");
                        if (validMinions[i])
                        {
                            Debug.Log("valid minion: " + i);
                            var drone = validMinions[i].GetBody();

                            EffectData effectData = new EffectData
                            {
                                origin = drone.corePosition,
                                genericFloat = 1.5f,
                                genericUInt = (uint)(drone.bodyIndex + 1),
                                genericBool = true

                            };

                            effectData.SetNetworkedObjectReference(context.purchasedObject);  //behaves strangely if target is networked ref

                            EffectManager.SpawnEffect(_bodyOrb, effectData, true);


                            //Util.PlaySound("RefabricatorSelect2", model);

                            drone.healthComponent.Suicide();
                        }
                    }
                }
            }
            public static bool IsAffordable(CostTypeDef costTypeDef, CostTypeDef.IsAffordableContext context)
            {
                CharacterBody body = context.activator.GetComponent<CharacterBody>();
                if (!body)
                {
                    return false;
                }
                Inventory inventory = body.inventory;
                if (!inventory)
                {
                    return false;
                }
                int cost = context.cost;

                if ((body != null) ? body.master : null)
                {
                    MinionOwnership.MinionGroup minionGroup = MinionOwnership.MinionGroup.FindGroup(body.master.netId);
                    if (minionGroup != null)
                    {
                        var members = minionGroup.members;

                        List<CharacterMaster> drone1Minions = new List<CharacterMaster>();
                        List<CharacterMaster> drone2Minions = new List<CharacterMaster>();
                        List<CharacterMaster> flameDroneMinions = new List<CharacterMaster>();
                        List<CharacterMaster> missileDroneMinions = new List<CharacterMaster>();
                        List<CharacterMaster> laserDroneMinions = new List<CharacterMaster>();

                        foreach (var drone in members)
                        {
                            if (drone)
                            {
                                CharacterMaster master = drone.GetComponent<CharacterMaster>();
                                if (master)
                                {
                                    var droneBody = master.GetBody();
                                    if (droneBody)
                                    {
                                        if ((droneBody.bodyFlags & CharacterBody.BodyFlags.Mechanical) > CharacterBody.BodyFlags.None)
                                        {
                                            switch (droneBody.baseNameToken)
                                            {
                                                default:
                                                    break;
                                                case "DRONE_GUNNER_BODY_NAME":
                                                    drone1Minions.Add(master);
                                                    break;
                                                case "DRONE_HEALING_BODY_NAME":
                                                    drone2Minions.Add(master);
                                                    break;
                                                case "FLAMEDRONE_BODY_NAME":
                                                    flameDroneMinions.Add(master);
                                                    break;
                                                case "DRONE_MISSILE_BODY_NAME":
                                                    missileDroneMinions.Add(master);
                                                    break;
                                                case "LIT_DRONE_LASER_NAME":
                                                    laserDroneMinions.Add(master);
                                                    break;
                                            }
                                            if (drone1Minions.Count >= cost || drone2Minions.Count >= cost || flameDroneMinions.Count >= cost || missileDroneMinions.Count >= cost || laserDroneMinions.Count >= cost)
                                            {
                                                return true;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                return false;
            }
        }
    }
}
