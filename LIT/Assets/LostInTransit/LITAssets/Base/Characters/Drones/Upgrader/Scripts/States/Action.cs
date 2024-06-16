using RoR2;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace EntityStates.Upgrader
{
    public class Action : UpgraderBaseState
    {
        public static float duration;
        private bool hasActivated = false;

        public GameObject droneMasterPrefab;
        public GameObject summonerBody;
        protected override bool enableInteraction
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
            if (fixedAge >= duration / 2 && !hasActivated)
            {
                hasActivated = true;
                PlayCrossfade("Base", "Action", "Playback", duration / 2f, 0.05f);
            }
            if (fixedAge >= duration)
                outer.SetNextStateToMain();
        }

        public override void OnExit()
        {
            base.OnExit();

            if (droneMasterPrefab != null)
            {
                var summon = new MasterSummon();
                summon.position = transform.position + (Vector3.up * 4);
                summon.masterPrefab = droneMasterPrefab;
                summon.summonerBodyObject = summonerBody;
                var droneMaster = summon.Perform();
                if (droneMaster)
                {
                    Transform mdlTransform = droneMaster.bodyPrefab.GetComponent<ModelLocator>().modelTransform;
                    if (mdlTransform)
                    {
                        TemporaryOverlay temporaryOverlay = mdlTransform.gameObject.AddComponent<TemporaryOverlay>();

                        temporaryOverlay.duration = 1f;
                        temporaryOverlay.animateShaderAlpha = true;
                        temporaryOverlay.alphaCurve = AnimationCurve.EaseInOut(0f, 0.5f, 1f, 0f);
                        temporaryOverlay.destroyComponentOnEnd = true;
                        temporaryOverlay.originalMaterial = Addressables.LoadAssetAsync<Material>("RoR2/Base/Huntress/matHuntressFlashBright.mat").WaitForCompletion();

                        temporaryOverlay.AddToCharacerModel(mdlTransform.GetComponent<CharacterModel>());
                    }
                    //idk any other behavior ig
                }
            }
        }
    }
}
