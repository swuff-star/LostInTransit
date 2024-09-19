using RoR2;
using UnityEngine;

namespace LostInTransit.Components
{
    [RequireComponent(typeof(ObjectScaleCurve))]
    public class LoopObjectScaleCurve : MonoBehaviour
    {
        public ObjectScaleCurve component { get => gameObject.GetComponent<ObjectScaleCurve>(); }

        public void Update()
        {
            if (component.time > component.timeMax)
            {
                component.Reset();
            }
        }
    }
}
