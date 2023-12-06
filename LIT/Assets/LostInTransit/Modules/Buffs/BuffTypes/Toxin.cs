using Moonstorm;
using RoR2;
using R2API;
using Moonstorm.Components;
using UnityEngine;
using System.Collections.Generic;

namespace LostInTransit.Buffs
{
    public class Toxin : BuffBase
    {
        public override BuffDef BuffDef { get; } = LITAssets.LoadAsset<BuffDef>("bdToxin", LITBundle.Items);

        public class ToxinBehavior : BaseBuffBodyBehavior, IBodyStatArgModifier
        {
            [BuffDefAssociation(useOnClient = true, useOnServer = true)]
            public static BuffDef GetBuffDef() => LITContent.Buffs.bdToxin;
            private GameObject effectInstance;
            private List<GameObject> toxinEffectInstances;

            public void Start()
            {
                //effectInstance = Instantiate(LITAssets.LoadAsset<GameObject>("ToxinEffect", LITBundle.Items), base.transform);
                //ParticleSystem ps = effectInstance.GetComponent<ParticleSystem>();
                GameObject charModel = body.modelLocator.modelTransform.gameObject;
                if (charModel != null)
                {
                    CharacterModel cm = charModel.GetComponent<CharacterModel>();
                    if (cm != null)
                    {
                        CharacterModel.RendererInfo[] rendererInfos = cm.baseRendererInfos;
                        CharacterBody wtf = cm.body;
                        if (wtf != null && rendererInfos != null)
                        {
                            for (int i = 0; i < rendererInfos.Length; i++)
                            {
                                if (!rendererInfos[i].ignoreOverlays)
                                {
                                    GameObject effect = AddToxinParticles(rendererInfos[i].renderer, wtf.coreTransform);
                                    if (effect != null)
                                    {
                                        toxinEffectInstances.Add(effect);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            private GameObject AddToxinParticles(Renderer modelRenderer, Transform targetParentTransform)
            {
                if (modelRenderer is MeshRenderer || modelRenderer is SkinnedMeshRenderer)
                {
                    GameObject effectPrefab = Instantiate(LITAssets.LoadAsset<GameObject>("ToxinEffect", LITBundle.Items), targetParentTransform);
                    ParticleSystem ps = effectPrefab.GetComponent<ParticleSystem>();
                    ParticleSystem.ShapeModule shape = ps.shape;
                    if (modelRenderer != null)
                    {
                        if (modelRenderer is MeshRenderer)
                        {
                            shape.shapeType = ParticleSystemShapeType.MeshRenderer;
                            shape.meshRenderer = (MeshRenderer)modelRenderer;
                        }
                        else if (modelRenderer is SkinnedMeshRenderer)
                        {
                            shape.shapeType = ParticleSystemShapeType.SkinnedMeshRenderer;
                            shape.skinnedMeshRenderer = (SkinnedMeshRenderer)modelRenderer;
                        }
                    }
                    ParticleSystem.MainModule main = ps.main;
                    ps.gameObject.SetActive(true);
                    BoneParticleController bpc = effectPrefab.GetComponent<BoneParticleController>();
                    if (bpc != null && modelRenderer is SkinnedMeshRenderer)
                    {
                        bpc.skinnedMeshRenderer = (SkinnedMeshRenderer)modelRenderer;
                    }
                    return effectPrefab;
                }
                return null;
            }

            public void OnDestroy()
            {
                //why dont you work..

                /*for (int i = 0; i < toxinEffectInstances.Count; i++)
                {
                    if (toxinEffectInstances[i] != null)
                    {
                        toxinEffectInstances[i].GetComponent<ParticleSystem>().enableEmission = false;
                        DestroyOnTimer dot = toxinEffectInstances[i].GetComponent<DestroyOnTimer>();
                        if (dot != null)
                            dot.enabled = true;
                    }
                }*/
            }

            public void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args)
            {
                args.armorAdd -= Items.TheToxin.toxinArmorDebuff;
            }
        }
    }
}