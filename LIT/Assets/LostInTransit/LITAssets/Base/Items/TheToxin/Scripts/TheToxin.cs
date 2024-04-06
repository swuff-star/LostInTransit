using MSU;
using RoR2;
using R2API;
using RoR2.Items;
using UnityEngine.Networking;
using UnityEngine;
using RoR2.ContentManagement;
using System.Collections;
using MSU.Config;
using System.Collections.Generic;
using RoR2.Orbs;

namespace LostInTransit.Items
{
    public sealed class TheToxin : LITItem
    {
        private const string TOKEN = "LIT_ITEM_THETOXIN_DESC";

        [RiskOfOptionsConfigureField(LITConfig.ITEMS, ConfigDescOverride = "Time in seconds until The Toxin can re-infect.")]
        [FormatToken(TOKEN, 0)]
        public static float toxinCooldown = 6f;

        [RiskOfOptionsConfigureField(LITConfig.ITEMS, ConfigDescOverride = "Time in seconds that The Toxin infects enemies.")]
        [FormatToken(TOKEN, 1)]
        public static float toxinDuration = 8f;

        [RiskOfOptionsConfigureField(LITConfig.ITEMS, ConfigDescOverride = "Range of which enemies will become infected by The Toxin.")]
        [FormatToken(TOKEN, 2)]
        public static float toxinRadius = 8f;

        [RiskOfOptionsConfigureField(LITConfig.ITEMS, ConfigDescOverride = "Armor removed by the debuff inflicted by The Toxin.")]
        [FormatToken(TOKEN, 3)]
        public static float toxinArmorReduction = 40f;

        public override NullableRef<GameObject> ItemDisplayPrefab => null;
        public override ItemDef ItemDef => _itemDef;
        private ItemDef _itemDef;

        private BuffDef _toxin;

        private static GameObject _toxinRangeIndicator;
        private BuffDef _toxinReady;

        private BuffDef _toxinCooldown;

        public override void Initialize()
        {
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public override IEnumerator LoadContentAsync()
        {
            /*
             * ItemDef - "TheToxin" - Items
             * BuffDef - "bdToxin" - Items;
             * BuffDef - "bdToxinReady" - Items
             * GameObject - "ToxinIndicator" - Items
             * BuffDef - "bdToxinCooldown" - Items
             */
            yield break;
        }

        public class TheToxinBehavior : BaseItemBodyBehavior
        {
            [ItemDefAssociation(useOnClient = true, useOnServer = true)]
            public static ItemDef GetItemDef() => LITContent.Items.TheToxin;

            public void FixedUpdate()
            {
                if (!body.HasBuff(LITContent.Buffs.bdToxinCooldown) && !body.HasBuff(LITContent.Buffs.bdToxinReady))
                {
                    body.SetBuffCount(LITContent.Buffs.bdToxinReady.buffIndex, 1);
                }
            }

            public void OnDestroy()
            {
                if (body.GetItemCount(LITContent.Items.TheToxin) == 0)
                {
                    body.SetBuffCount(LITContent.Buffs.bdToxinReady.buffIndex, 0);
                    body.SetBuffCount(LITContent.Buffs.bdToxinCooldown.buffIndex, 0);
                }
            }
        }

        public class ToxinBehavior : BuffBehaviour, IBodyStatArgModifier
        {
            [BuffDefAssociation()]
            public static BuffDef GetBuffDef() => LITContent.Buffs.bdToxin;
            private GameObject effectInstance;
            private List<GameObject> toxinEffectInstances;

            public void OnEnable()
            {
                //effectInstance = Instantiate(LITAssets.LoadAsset<GameObject>("ToxinEffect", LITBundle.Items), base.transform);
                //ParticleSystem ps = effectInstance.GetComponent<ParticleSystem>();
                GameObject charModel = CharacterBody.modelLocator.modelTransform.gameObject;
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
                                //pls work
                                if (rendererInfos[i].renderer && !rendererInfos[i].ignoreOverlays)
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

            public void OnDisable()
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
                args.armorAdd -= Items.TheToxin.toxinArmorReduction;
            }
        }

        public class ToxinReadyBehavior : BuffBehaviour
        {
            [BuffDefAssociation]
            public static BuffDef GetBuffDef() => LITContent.Buffs.bdToxinReady;
            private GameObject indicatorInstance;
            private WardUtils indicatorUtils;
            private SphereSearch search;
            private List<HurtBox> hits;
            private float checkTime = 0.333f;
            private float checkTimer = 0f;

            public void Awake()
            {
                hits = new List<HurtBox>();
                search = new SphereSearch();
                search.mask = LayerIndex.entityPrecise.mask;
                search.radius = Items.TheToxin.toxinRadius;
            }

            private void OnEnable()
            {
                AttemptInfect();

                indicatorInstance = Instantiate(_toxinRangeIndicator);
                indicatorUtils = indicatorInstance.GetComponent<WardUtils>();
                indicatorUtils.radius = Items.TheToxin.toxinRadius * 2;
                indicatorInstance.GetComponent<NetworkedBodyAttachment>().AttachToGameObjectAndSpawn(gameObject);
            }

            public void FixedUpdate()
            {
                if (NetworkServer.active)
                {
                    checkTimer += Time.fixedDeltaTime;
                    if (checkTimer >= checkTime)
                    {
                        checkTime -= checkTime;
                        AttemptInfect();
                    }
                }
            }

            public void OnDisable()
            {
                if (indicatorUtils != null)
                    indicatorUtils.shouldDestroy = true;
            }

            public void AttemptInfect()
            {
                hits.Clear();
                search.ClearCandidates();
                search.origin = CharacterBody.corePosition;
                search.RefreshCandidates();
                search.FilterCandidatesByDistinctHurtBoxEntities();
                search.FilterCandidatesByHurtBoxTeam(TeamMask.GetUnprotectedTeams(CharacterBody.teamComponent.teamIndex));
                search.GetHurtBoxes(hits);

                CharacterBody closestBody = null;

                foreach (HurtBox h in hits)
                {
                    HealthComponent hp = h.healthComponent;
                    if (hp)
                    {
                        CharacterBody bodyS = hp.body;
                        if (bodyS && bodyS != CharacterBody)
                        {
                            float distance = Vector3.Distance(transform.position, CharacterBody.transform.position);
                            if (closestBody == null || distance < Vector3.Distance(transform.position, closestBody.transform.position))
                            {
                                closestBody = bodyS;
                            }
                        }
                    }
                }

                if (closestBody != null)
                {
                    //closestBody.AddTimedBuff(LITContent.Buffs.bdToxin, Items.TheToxin.toxinDur);
                    Orbs.ToxinOrb toxinOrb = new Orbs.ToxinOrb();
                    toxinOrb.origin = transform.position;
                    toxinOrb.target = closestBody.mainHurtBox;
                    OrbManager.instance.AddOrb(toxinOrb);

                    if (indicatorUtils != null)
                        indicatorUtils.shouldDestroy = true;

                    CharacterBody.RemoveBuff(LITContent.Buffs.bdToxinReady);
                    CharacterBody.AddTimedBuff(LITContent.Buffs.bdToxinCooldown, Items.TheToxin.toxinCooldown);
                }
            }
        }
    }
}
