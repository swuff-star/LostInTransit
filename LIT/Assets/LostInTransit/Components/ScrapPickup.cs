using RoR2;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace LostInTransit.Components
{
    public class ScrapPickup : MonoBehaviour
    {
        [Tooltip("The base object to destroy when this pickup is consumed.")]
        public GameObject baseObject;
        [Tooltip("The team filter object which determines who can pick up this pack.")]
        public TeamFilter teamFilter;
        [Tooltip("The character body which determines who can pick up this pack.")]
        public CharacterBody characterBody;

        public GameObject pickupEffectPrefab;
        public float scrapValue;
        private bool alive = true;

        private void OnTriggerStay(Collider other)
        {
            if (NetworkServer.active && alive)
            {
                TeamIndex objectTeam = TeamComponent.GetObjectTeam(other.gameObject);
                CharacterBody objectBody = other.gameObject.GetComponent<CharacterBody>();
                DrifterScrapComponent dsc = other.gameObject.GetComponent<DrifterScrapComponent>();
                if (objectTeam == teamFilter.teamIndex && objectBody.bodyIndex == characterBody.bodyIndex && dsc)
                {
                    alive = false;
                    Vector3 position = transform.position;
                    dsc.AddScrap(scrapValue);
                    if (pickupEffectPrefab)
                    {
                        EffectManager.SimpleEffect(pickupEffectPrefab, position, Quaternion.identity, true);
                    }
                    Destroy(baseObject);
                }
            }
        }
    }
}
