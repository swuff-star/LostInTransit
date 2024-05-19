using MSU;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using EntityStates.Mage.Weapon;

namespace EntityStates.BlazeDrone
{
    public class BlazeDroneFlamethrower : Flamethrower
    {
        public override void OnEnter()
        {
            base.OnEnter();
            radius = 3f;
            totalDamageCoefficient = 25f;
        }
    }
}