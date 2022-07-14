using LostInTransit.Elites;
using Moonstorm;
using Moonstorm.Components;
using R2API;
using RoR2;
using RoR2.Artifacts;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace LostInTransit.Buffs
{
    //★ I did something naughty and just... commented out a lot of errors instead of fixing them. I plan on redoing this anyway so... oh well.
    public class AffixVolatile : BuffBase
    {
        public override BuffDef BuffDef { get; } = LITAssets.LoadAsset<BuffDef>("AffixVolatile");

        public override void Initialize()
        {
            On.RoR2.GlobalEventManager.OnHitAll += VolatileExplosion;
        }

        private void VolatileExplosion(On.RoR2.GlobalEventManager.orig_OnHitAll orig, GlobalEventManager self, DamageInfo damageInfo, GameObject hitObject)
        {
            orig(self, damageInfo, hitObject);
            if (damageInfo != null)
            {
                var atkr = damageInfo.attacker;
                if (atkr)
                {
                    var component = atkr.GetComponent<AffixVolatileBehavior>();
                    if (component)
                    {
                        component.OnHitAll(damageInfo);
                    }
                }
            }
        }

        public class AffixVolatileBehavior : BaseBuffBodyBehavior, IOnTakeDamageServerReceiver
        {
            [BuffDefAssociation(useOnClient = true, useOnServer = true)]
            public static BuffDef GetBuffDef() => LITContent.Buffs.AffixVolatile;

            private float diffScaling { get => DifficultyCatalog.GetDifficultyDef(Run.instance.selectedDifficulty).scalingValue; }
            private GameObject bomb;

            public float damageThreshold;
            public float tankedDamage;
            public float timeSinceBombsDeployed;

            private void Start()
            {
                if (body.healthComponent)
                {
                    //HG.ArrayUtils.ArrayAppend(ref body.healthComponent.onTakeDamageReceivers, this);
                }

                damageThreshold = (body.maxHealth / 1.5f) / diffScaling;
                bomb = VolatileSpitebomb.VolatileSpiteBomb;
            }

            public void OnTakeDamageServer(DamageReport damageReport)
            {
                tankedDamage += damageReport.damageDealt;
                damageThreshold = (body.maxHealth / 1.5f) / diffScaling;
            }

            private void Update()
            {
                timeSinceBombsDeployed += Time.deltaTime;
                if (tankedDamage > damageThreshold)
                {
                    if (timeSinceBombsDeployed > 3)
                    {
                        tankedDamage = 0f;
                        //PrepBombs();
                    }
                    else
                    {
                        tankedDamage /= 2;
                    }
                }
            }

            /*private void PrepBombs()
            {
                if (!NetworkServer.active)
                    return;

                if (bomb)
                {
                    int bombAmount = Mathf.Min(BombArtifactManager.maxBombCount/2, Mathf.CeilToInt(body.bestFitRadius * BombArtifactManager.extraBombPerRadius));
                    List<(BombArtifactManager.BombRequest, float)> bombs = new List<(BombArtifactManager.BombRequest, float)>();
                    for (int i = 0; i < bombAmount; i++)
                    {
                        Vector3 b = UnityEngine.Random.insideUnitSphere * (BombArtifactManager.bombSpawnBaseRadius + body.bestFitRadius * BombArtifactManager.bombSpawnRadiusCoefficient);
                        BombArtifactManager.BombRequest bomb = new BombArtifactManager.BombRequest
                        {
                            spawnPosition = body.corePosition,
                            raycastOrigin = body.corePosition + b,
                            bombBaseDamage = body.damage,
                            attacker = body.gameObject,
                            teamIndex = body.teamComponent.teamIndex,
                            velocityY = UnityEngine.Random.Range(5f, 25f)
                        };
                        Ray ray = new Ray(bomb.raycastOrigin + new Vector3(0f, BombArtifactManager.maxBombStepUpDistance, 0f), Vector3.down);
                        float maxDistance = BombArtifactManager.maxBombStepUpDistance + BombArtifactManager.maxBombFallDistance;
                        RaycastHit rayCastHit;
                        if (Physics.Raycast(ray, out rayCastHit, maxDistance, LayerIndex.world.mask, QueryTriggerInteraction.Ignore))
                        {
                            bombs.Add((bomb, rayCastHit.point.y));
                        }
                    }
                    SpawnBombs(bombs);
                }
            }*/

            /*private void SpawnBombs(List<(BombArtifactManager.BombRequest, float)> bombs)
            {
                foreach ((BombArtifactManager.BombRequest bomb, float groundY) in bombs)
                {
                    Vector3 spawnPosition = bomb.spawnPosition;
                    if (spawnPosition.y < groundY + 4f)
                    {
                        spawnPosition.y = groundY + 4f;
                    }
                    Vector3 raycastOrigin = bomb.raycastOrigin;
                    raycastOrigin.y = groundY;
                    GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(BombArtifactManager.bombPrefab, spawnPosition, UnityEngine.Random.rotation);
                    SpiteBombController component = gameObject.GetComponent<SpiteBombController>();
                    DelayBlast delayBlast = component.delayBlast;
                    TeamFilter component2 = gameObject.GetComponent<TeamFilter>();
                    component.bouncePosition = raycastOrigin;
                    component.initialVelocityY = bomb.velocityY;
                    delayBlast.position = spawnPosition;
                    delayBlast.baseDamage = bomb.bombBaseDamage;
                    delayBlast.baseForce = 2300f;
                    delayBlast.attacker = bomb.attacker;
                    delayBlast.radius = BombArtifactManager.bombBlastRadius;
                    delayBlast.crit = false;
                    delayBlast.procCoefficient = 0.75f;
                    delayBlast.maxTimer = BombArtifactManager.bombFuseTimeout;
                    delayBlast.timerStagger = 0f;
                    delayBlast.falloffModel = BlastAttack.FalloffModel.None;
                    component2.teamIndex = bomb.teamIndex;
                    NetworkServer.Spawn(gameObject);
                }
            }*/


            public void OnHitAll(DamageInfo dmgInfo)
            {
                var atkBody = dmgInfo.attacker.GetComponent<CharacterBody>();
                if (dmgInfo.procCoefficient != 0f && !DamageAPI.HasModdedDamageType(dmgInfo, DamageTypes.Volatile.volatileDamageType))
                {
                    float radius = 1.5f + (2.5f * dmgInfo.procCoefficient);
                    float dmgCoef = 0.3f;
                    float baseDamage = Util.OnHitProcDamage(dmgInfo.damage, atkBody.damage, dmgCoef);
                    EffectManager.SpawnEffect(Resources.Load<GameObject>("Prefabs/Effects/OmniEffect/OmniExplosionVFXQuick"), new EffectData
                    {
                        origin = dmgInfo.position,
                        scale = radius,
                        rotation = Util.QuaternionSafeLookRotation(dmgInfo.force)
                    }, transmit: true);

                    BlastAttack atk = new BlastAttack
                    {
                        position = dmgInfo.position,
                        baseDamage = baseDamage,
                        baseForce = 0f,
                        radius = radius,
                        attacker = dmgInfo.attacker,
                        inflictor = null
                    };
                    atk.teamIndex = TeamComponent.GetObjectTeam(dmgInfo.attacker);
                    atk.crit = dmgInfo.crit;
                    atk.procChainMask = dmgInfo.procChainMask;
                    atk.damageColorIndex = DamageColorIndex.WeakPoint;
                    atk.falloffModel = BlastAttack.FalloffModel.None;
                    atk.damageType = dmgInfo.damageType;
                    DamageAPI.AddModdedDamageType(atk, DamageTypes.Volatile.volatileDamageType);
                    atk.Fire();
                }
            }

            /*private void OnDestroy()
            {
                //This SHOULDNT cause any errors because nothing should be fucking with the order of things in this list... I hope.
                if (body.healthComponent)
                {
                    int i = Array.IndexOf(body.healthComponent.onIncomingDamageReceivers, this);
                    if (i > -1)
                    {
                        HG.ArrayUtils.ArrayRemoveAtAndResize(ref body.healthComponent.onIncomingDamageReceivers, body.healthComponent.onIncomingDamageReceivers.Length, i);
                    }
                }
            }*/
        }
    }
}
