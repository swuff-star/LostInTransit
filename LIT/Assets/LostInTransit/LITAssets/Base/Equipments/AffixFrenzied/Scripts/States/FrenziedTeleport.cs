using EntityStates.ParentMonster;
using RoR2;
using RoR2.Navigation;
using UnityEngine;

namespace EntityStates.AffixFrenzied
{
    public class FrenziedTeleport : BaseState
    {
        private Transform modelTransform;

        public static GameObject blinkPrefab;

        public static Material destealthMaterial;

        private float stopwatch;

        private Vector3 blinkDestination = Vector3.zero;

        private Vector3 blinkStart = Vector3.zero;

        public static float duration = 0.3f;

        public static float blinkDistance = 25f;

        public static string beginSoundString;

        public static string endSoundString;

        public static float blinkStopwatch;

        public bool blinkReady = false;

        public static float destealthDuration;

        private CharacterModel characterModel;

        private HurtBoxGroup hurtboxGroup;

        private static bool _hasRunAtLeastOnce = false;
        public override void OnEnter()
        {
            if(!_hasRunAtLeastOnce)
            {
                blinkPrefab = LoomingPresence.blinkPrefab;
                destealthMaterial = LoomingPresence.destealthMaterial;
                duration = LoomingPresence.duration;
                blinkDistance = LoomingPresence.blinkDistance;
                beginSoundString = "FrenzyDash";
                endSoundString = LoomingPresence.endSoundString;
                destealthDuration = LoomingPresence.destealthDuration;
                _hasRunAtLeastOnce = true;
            }
            base.OnEnter();
            Util.PlaySound(beginSoundString, gameObject);
            modelTransform = GetModelTransform();
            if ((bool)modelTransform)
            {
                characterModel = modelTransform.GetComponent<CharacterModel>();
                hurtboxGroup = modelTransform.GetComponent<HurtBoxGroup>();
            }
            if ((bool)characterModel)
            {
                characterModel.invisibilityCount++;
            }
            if ((bool)hurtboxGroup)
            {
                hurtboxGroup.hurtBoxesDeactivatorCounter++;
            }
            if ((bool)characterMotor)
            {
                characterMotor.enabled = false;
            }
            if (isAuthority)
            {
                Vector3 vector = base.inputBank.aimDirection * blinkDistance;
                blinkDestination = base.transform.position;
                blinkStart = base.transform.position;

                bool isAerial = (characterBody && characterBody.isFlying) || (characterMotor && !characterMotor.isGrounded);
                NodeGraph nodes = isAerial ? SceneInfo.instance.airNodes : SceneInfo.instance.groundNodes;
                NodeGraph.NodeIndex nodeIndex = nodes.FindClosestNode(transform.position + vector, characterBody.hullClassification);

                if (nodeIndex == NodeGraph.NodeIndex.invalid)
                {
                    if (isAerial)
                    {
                        nodes = SceneInfo.instance.groundNodes;
                        nodeIndex = nodes.FindClosestNode(transform.position + vector, characterBody.hullClassification);
                    }
                    else
                    {
                        nodes = SceneInfo.instance.airNodes;
                        nodeIndex = nodes.FindClosestNode(transform.position + vector, characterBody.hullClassification);
                    }
                }

                nodes.GetNodePosition(nodeIndex, out blinkDestination);
                blinkDestination += transform.position - characterBody.footPosition;
                vector = blinkDestination - blinkStart;
                //CreateBlinkEffect(Util.GetCorePosition(gameObject), vector);
            }
        }
        private void CreateBlinkEffect(Vector3 origin, Vector3 direction)
        {
            EffectData effectData = new EffectData();
            effectData.rotation = Util.QuaternionSafeLookRotation(direction);
            effectData.origin = origin;
            EffectManager.SpawnEffect(blinkPrefab, effectData, transmit: true);
        }

        private void SetPosition(Vector3 newPos)
        {
            if (characterMotor)
            {
                characterMotor.Motor.SetPositionAndRotation(newPos, characterMotor.transform.rotation);
            }
            else
            {
                if (characterBody.rigidbody)
                    characterBody.rigidbody.interpolation = RigidbodyInterpolation.None;
                characterBody.transform.SetPositionAndRotation(newPos, characterBody.transform.rotation);
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            stopwatch += Time.fixedDeltaTime;
            if ((bool)characterMotor && (bool)characterDirection)
                characterMotor.velocity = Vector3.zero;
            SetPosition(Vector3.Lerp(blinkStart, blinkDestination, stopwatch / duration));
            if (stopwatch >= duration && base.isAuthority)
            {
                outer.SetNextStateToMain();
            }
        }

        public override void OnExit()
        {
            Util.PlaySound(endSoundString, gameObject);
            modelTransform = GetModelTransform();
            if ((bool)characterDirection)
                characterDirection.forward = GetAimRay().direction;
            if ((bool)modelTransform && (bool)destealthMaterial)
            {
                TemporaryOverlay temporaryOverlay = modelTransform.gameObject.AddComponent<TemporaryOverlay>();
                temporaryOverlay.duration = destealthDuration;
                temporaryOverlay.destroyComponentOnEnd = true;
                temporaryOverlay.originalMaterial = destealthMaterial;
                temporaryOverlay.inspectorCharacterModel = modelTransform.gameObject.GetComponent<CharacterModel>();
                temporaryOverlay.alphaCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
                temporaryOverlay.animateShaderAlpha = true;
            }
            if ((bool)characterModel)
                characterModel.invisibilityCount--;
            if ((bool)hurtboxGroup)
                hurtboxGroup.hurtBoxesDeactivatorCounter--;
            if ((bool)characterMotor)
                characterMotor.enabled = true;

            base.OnExit();
        }
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Pain;
        }
    }
}
