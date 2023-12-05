using Moonstorm;
using RoR2;
using R2API;
using Moonstorm.Components;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;

namespace LostInTransit.Buffs
{
    public class ToxinReady : BuffBase
    {
        public override BuffDef BuffDef { get; } = LITAssets.LoadAsset<BuffDef>("bdToxinReady", LITBundle.Items);

        public class ToxinReadyBehavior : BaseBuffBodyBehavior
        {
            [BuffDefAssociation(useOnClient = true, useOnServer = true)]
            public static BuffDef GetBuffDef() => LITContent.Buffs.bdToxinReady;
            //public static GameObject rangeIndicator = LITAssets.LoadAsset<GameObject>("ToxinIndicator", LITBundle.Items);
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
                    closestBody.AddTimedBuff(LITContent.Buffs.bdToxin, Items.TheToxin.toxinDur);
                    body.RemoveBuff(LITContent.Buffs.bdToxinReady);
                    body.AddTimedBuff(LITContent.Buffs.bdToxinCooldown, Items.TheToxin.toxinCD);
                }
            }
        }
    }
}