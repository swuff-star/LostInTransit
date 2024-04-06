using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoR2;
using UnityEngine;

namespace LostInTransit.Components
{
    public class TemporaryItemPickupComponent : MonoBehaviour
    {
        // need to manually reenable the particle systems per tier, since temporary items are separate tiers
        // unless it looks too busy with the blue glow

        public static Action<TemporaryItemPickupComponent> onAwakeGlobal;

        public float itemDuration = LITTempItems.fallbackTemporaryItemDuration;

        private void Awake()
        {
            if(onAwakeGlobal != null)
            {
                onAwakeGlobal.Invoke(this);
            }
        }


    }
}
