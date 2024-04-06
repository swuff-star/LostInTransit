using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using RoR2;
using RoR2.UI;

namespace LostInTransit.Components
{

    // needs a blue backdrop on the icon

    // very noticeable slowdown starting around ~100-150 separate timers
    // (though i was only ever getting that many timers because of a different bug)
    public class TemporaryItemHudElement : NetworkBehaviour
    {
        [NonSerialized]
        [SyncVar(hook = "OnDurationChanged")] // wat
        public float duration;

        public float timeLeft;

        public ImageFillController fillController;

        private void OnDurationChanged(float duration)
        {
            this.timeLeft = duration;
        }

        private void FixedUpdate()
        {
            this.timeLeft -= Time.fixedDeltaTime;
            if (fillController)
            {
                fillController.SetTValue(timeLeft / duration);
            }
        }
    }
}
