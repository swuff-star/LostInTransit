using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace LostInTransit
{
    public class WardUtils : NetworkBehaviour
    {
        public float radius;
        public Transform rangeIndicator;
        public float scaleTime;

        public bool shouldDestroy = false;
        private Vector3 targetScale;

        private float timer = 0f;
        private float timer2 = 0f;

        private void Start()
        {
            rangeIndicator.localScale = Vector3.zero;
            targetScale = new Vector3(radius, radius, radius);
        }

        public void FixedUpdate()
        {
            if (shouldDestroy == false && rangeIndicator.localScale != targetScale)
            {
                rangeIndicator.localScale = Vector3.Lerp(Vector3.zero, targetScale, timer / scaleTime);
                timer += Time.fixedDeltaTime;
            }

            if (shouldDestroy == true && rangeIndicator.localScale != Vector3.zero)
            {
                rangeIndicator.localScale = Vector3.Lerp(targetScale, Vector3.zero, timer2 / scaleTime);
                timer2 += Time.fixedDeltaTime;
            }

            if (timer2 >= scaleTime)
                Destroy(gameObject);
        }
    }
}
