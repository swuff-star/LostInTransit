using Moonstorm;
using RoR2;
using R2API;
using Moonstorm.Components;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using RoR2.Orbs;

namespace LostInTransit.Buffs
{
    public class ToxinReady : BuffBase
    {
        public override BuffDef BuffDef { get; } = LITAssets.LoadAsset<BuffDef>("bdToxinReady", LITBundle.Items);

        public class ToxinReadyBehavior : BaseBuffBodyBehavior
        {
            [BuffDefAssociation(useOnClient = true, useOnServer = true)]
            public static BuffDef GetBuffDef() => LITContent.Buffs.bdToxinReady;
            public static GameObject rangeIndicator = LITAssets.LoadAsset<GameObject>("ToxinIndicator", LITBundle.Items);
            private GameObject indicatorInstance;
            private WardUtils indicatorUtils;
            private SphereSearch search;
            private List<HurtBox> hits;
            private float checkTime = 0.333f;
            private float checkTimer = 0f;

            public void Start()
            {
                hits = new List<HurtBox>();
                search = new SphereSearch();
                search.mask = LayerIndex.entityPrecise.mask;
                search.radius = Items.TheToxin.toxinRadius;
                AttemptInfect();

                indicatorInstance = Instantiate(rangeIndicator);
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

            public void OnDestroy()
            {
                if (indicatorUtils != null)
                    indicatorUtils.shouldDestroy = true;
            }

            public void AttemptInfect()
            {
                hits.Clear();
                search.ClearCandidates();
                search.origin = body.corePosition;
                search.RefreshCandidates();
                search.FilterCandidatesByDistinctHurtBoxEntities();
                search.FilterCandidatesByHurtBoxTeam(TeamMask.GetUnprotectedTeams(body.teamComponent.teamIndex));
                search.GetHurtBoxes(hits);

                CharacterBody closestBody = null;

                foreach (HurtBox h in hits)
                {
                    HealthComponent hp = h.healthComponent;
                    if (hp)
                    {
                        CharacterBody bodyS = hp.body;
                        if (bodyS && bodyS != body)
                        {
                            float distance = Vector3.Distance(transform.position, body.transform.position);
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

                    body.RemoveBuff(LITContent.Buffs.bdToxinReady);
                    body.AddTimedBuff(LITContent.Buffs.bdToxinCooldown, Items.TheToxin.toxinCD);
                }
            }
        }
    }
}