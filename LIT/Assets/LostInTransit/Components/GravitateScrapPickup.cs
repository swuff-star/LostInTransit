using RoR2;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace LostInTransit.Components
{
    public class GravitateScrapPickup : MonoBehaviour
    {
        private Transform gravitateTarget;

        [Tooltip("The rigidbody to set the velocity of.")]
        public Rigidbody rb;
        [Tooltip("The TeamFilter which controls which team can activate this trigger.")]
        public TeamFilter teamFilter;
        [Tooltip("The CharacterBody which controls which body can activate this trigger.")]
        public CharacterBody characterBody;

        public float acceleration;
        public float maxSpeed;

        private void Start()
        {
        }

        private void OnTriggerEnter(Collider other)
        {
            if (NetworkServer.active && !gravitateTarget && teamFilter.teamIndex != TeamIndex.None)
            {
                CharacterBody characterBody = other.gameObject.GetComponent<CharacterBody>();
                if (TeamComponent.GetObjectTeam(other.gameObject) == teamFilter.teamIndex && characterBody.bodyIndex == this.characterBody.bodyIndex)
                {
                    gravitateTarget = other.gameObject.transform;
                }
            }
        }

        private void FixedUpdate()
        {
            if (gravitateTarget)
            {
                rb.velocity = Vector3.MoveTowards(rb.velocity, (gravitateTarget.transform.position - transform.position).normalized * maxSpeed, acceleration);
            }
        }
    }
}
