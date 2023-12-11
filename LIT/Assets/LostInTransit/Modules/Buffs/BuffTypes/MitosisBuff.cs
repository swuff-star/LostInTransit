using Moonstorm;
using Moonstorm.Components;
using R2API;
using RoR2;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LostInTransit.Buffs
{
    public class MitosisBuff : BuffBase
    {
        public override BuffDef BuffDef => LITAssets.LoadAsset<BuffDef>("bdMitosisBuff", LITBundle.Items);

        public class MitosisBuffBehavior : BaseBuffBodyBehavior, IStatItemBehavior
        {
            [BuffDefAssociation]
            private static BuffDef GetBuffDef() => LITContent.Buffs.bdMitosisBuff;
            private GameObject effectInstance;
            private List<GameObject> effectInstances;

            public void RecalculateStatsEnd()
            {
                if (body.HasBuff(LITContent.Buffs.bdMitosisBuff))
                {
                    if (body.skillLocator)
                    {
                        if (body.skillLocator.primary)
                            body.skillLocator.primary.cooldownScale *= 1 - Items.RapidMitosis.mitosisSkillCD;
                        if (body.skillLocator.secondary)
                            body.skillLocator.secondary.cooldownScale *= 1 - Items.RapidMitosis.mitosisSkillCD;
                        if (body.skillLocator.utility)
                            body.skillLocator.utility.cooldownScale *= 1 - Items.RapidMitosis.mitosisSkillCD;
                        if (body.skillLocator.special)
                            body.skillLocator.special.cooldownScale *= 1 - Items.RapidMitosis.mitosisSkillCD;
                    }
                }
            }

            public void Start()
            {
                GameObject charModel = body.modelLocator.modelTransform.gameObject;
                if (charModel != null)
                {
                    CharacterModel cm = charModel.GetComponent<CharacterModel>();
                    if (cm != null)
                    {
                        CharacterModel.RendererInfo[] rendererInfos = cm.baseRendererInfos;
                        if (rendererInfos != null)
                        {
                            for (int i = 0; i < rendererInfos.Length; i++)
                            {
                                //pls work
                                if (rendererInfos[i].renderer && !rendererInfos[i].ignoreOverlays)
                                {
                                    GameObject effect = AddParticles(rendererInfos[i].renderer, body.coreTransform);
                                    if (effect != null)
                                    {
                                        //effectInstances.Add(effect);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            private GameObject AddParticles(Renderer modelRenderer, Transform targetParentTransform)
            {
                if (modelRenderer is MeshRenderer || modelRenderer is SkinnedMeshRenderer)
                {
                    GameObject effectPrefab = Instantiate(LITAssets.LoadAsset<GameObject>("MitosisEffect", LITBundle.Items), targetParentTransform);
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

            public void RecalculateStatsStart()
            {

            }
        }
    }
}
