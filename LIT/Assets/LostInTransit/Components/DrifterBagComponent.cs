using RoR2;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Networking;

namespace LostInTransit.Components
{
    public class DrifterBagComponent : NetworkBehaviour
    {
        public bool bagDown;
        
        private bool inCombat;
        private bool previousInCombat;

        private CharacterBody body;
        private Animator animator;
        private GameObject modelTransform;
        private ChildLocator childLocator;
        private GameObject baseStraps;
        private GameObject bagStraps;
        private GameObject attackStraps;

        private void OnEnable()
        {
            body = GetComponent<CharacterBody>();

            modelTransform = body.modelLocator.modelTransform.gameObject;

            animator = modelTransform.GetComponent<Animator>();
            childLocator = modelTransform.GetComponent<ChildLocator>();
            baseStraps = childLocator.FindChild("BaseStraps").gameObject;
            bagStraps = childLocator.FindChild("BagStraps").gameObject;
            attackStraps = childLocator.FindChild("AttackStraps").gameObject;
        }

        private void FixedUpdate()
        {
            animator.SetBool("inCombat", bagDown);

            inCombat = !body.outOfCombat;
            if (previousInCombat != inCombat && !inCombat)
                RaiseBag();
            previousInCombat = inCombat;

            baseStraps.SetActive(!bagDown);
            attackStraps.SetActive(bagDown);
        }

        private void RaiseBag()
        {
            animator.speed = 1f;
            animator.Update(0f);
            int layerIndex = animator.GetLayerIndex("Gesture, Override");
            animator.CrossFadeInFixedTime("RaiseBag", 0.05f, layerIndex);
            animator.Update(0f);

            bagDown = false;
        }
    }
}
