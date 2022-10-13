using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Moonstorm;
using R2API;
using RoR2.Items;

namespace LostInTransit
{
    public class LITHooks : MonoBehaviour
    {
        public static void Init()
        {
            On.RoR2.Inventory.CalculateEquipmentCooldownScale += (orig, self) =>
            {
                float num = orig.Invoke(self);
                num *= (1 - MSUtil.InverseHyperbolicScaling(Items.RapidMitosis.mitosisCD, Items.RapidMitosis.mitosisCD, 0.7f, self.GetItemCount(LITContent.Items.RapidMitosis)));
                return num;
            };
        }
    }
}
