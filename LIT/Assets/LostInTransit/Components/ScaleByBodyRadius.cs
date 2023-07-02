using RoR2;
using UnityEngine;

namespace LostInTransit.Components
{
    public class ScaleByBodyRadius : MonoBehaviour
    {
        [Tooltip("The character body reference.\nIf left null, it'll try to get the component from its parent objects.")]
        public CharacterBody body;

        public void Start()
        {
            if (body == null)
            {
                var component = GetComponentInParent<CharacterBody>();
                if (component)
                {
                    body = component;
                    Scale();
                    return;
                }
                else
                {
                    component = GetComponentInChildren<CharacterBody>();
                    if (component)
                    {
                        body = component;
                        Scale();
                        return;
                    }
                    else
                    {

                    }
                    LITLog.Warning("Could not find CharacterBody!");
                    Destroy(this);
                    return;
                }
            }
        }

        private void Scale()
        {
            transform.localScale *= body.radius;
        }
    }
}