using System;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;


namespace LostInTransit.Components
{
    internal class NuggetPickup : MonoBehaviour
    {
        public GameObject baseObject;
        public TeamFilter team;

        [HideInInspector]
        public float BuffTimer, RegenMult;
        [HideInInspector]
        public int RegenStacks;

        [SerializeField]
        private BuffDef buff;

        private bool alive = true;

        //★ i feel shame seeing code this well constructed in the same project as mine
        private void OnTriggerStay(Collider other)
        {
            if (!NetworkServer.active || !alive)
                return;
            TeamIndex objTeam = TeamComponent.GetObjectTeam(other.gameObject);
            if(objTeam == team.teamIndex)
            {
                CharacterBody body = other.GetComponent<CharacterBody>();
                if(body)
                {
                    alive = false;
                    if(body.GetBuffCount(buff) < RegenStacks)
                    {
                        body.AddTimedBuff(buff, BuffTimer);
                    }
                    else
                    {
                        body.timedBuffs.Find(tb => tb.buffIndex == buff.buffIndex).timer = BuffTimer;
                    }
                    //Meat eat sound here (rex fruit?)
                    Destroy(baseObject);
                }
            }
        }
    }
}
