using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EntityStates.Drone.DroneWeapon;
using UnityEngine.AddressableAssets;

namespace EntityStates.EmergencyDrone
{
    public class GoldEmergencyHealBeam : StartHealBeam
    {
        public override void OnEnter()
        {
            healBeamPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Drones/EmergencyDroneHealBeam.prefab").WaitForCompletion();
            base.OnEnter();
        }
    }
}
