using RoR2;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Upgrader
{
    public class Action : UpgraderBaseState
    {
        public static float duration;

        public GameObject droneMasterPrefab;
        public GameObject summonerBody;
        protected virtual bool enableInteraction
        {
            get
            {
                return false;
            }
        }

        public override void OnEnter()
        {
            base.OnEnter();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (fixedAge > duration)
                outer.SetNextStateToMain();
        }

        public override void OnExit()
        {
            base.OnExit();

            if (droneMasterPrefab != null)
            {
                var summon = new MasterSummon();
                summon.position = transform.position + (Vector3.up * 3);
                summon.masterPrefab = droneMasterPrefab;
                summon.summonerBodyObject = summonerBody;
                var droneMaster = summon.Perform();
                if (droneMaster)
                {
                    //idk any other behavior ig
                }
            }
        }
    }
}
