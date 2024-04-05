using MSU;
using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace LostInTransit.Equipments
{
    //Add volatile damage type here!
    public class AffixVolatile : LITEliteEquipment
    {
        public override List<MSEliteDef> EliteDefs { get; } = new List<MSEliteDef>
        {
            LITAssets.LoadAsset<MSEliteDef>("Volatile", LITBundle.Equips),
            LITAssets.LoadAsset<MSEliteDef>("VolatileHonor", LITBundle.Equips)
        };

        public override EquipmentDef EquipmentDef { get; } = LITAssets.LoadAsset<EquipmentDef>("AffixVolatile", LITBundle.Equips);


        public override bool FireAction(EquipmentSlot slot)
        {
            if(slot.TryGetComponent<Buffs.AffixVolatile.AffixVolatileSelfDetonateBehaviour>(out var component))
            {
                return component.TryExplode(); ;
            }
            return false;
        }
    }
}
