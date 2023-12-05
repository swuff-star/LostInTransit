using LostInTransit.Components;
using Moonstorm;
using Moonstorm.Components;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace LostInTransit.Buffs
{
    public sealed class AffixBlighted : BuffBase
    {
        public override BuffDef BuffDef { get; } = LITAssets.LoadAsset<BuffDef>("bdAffixBlighted", LITBundle.Equips);
        private static GameObject _blightedBodyAttachment;

        public override void Initialize()
        {
            _blightedBodyAttachment = LITAssets.LoadAsset<GameObject>("BlightedBodyAttachment", LITBundle.Equips);
        }

        public class AffixBlightedBehaviour : BaseBuffBodyBehavior
        {
            [BuffDefAssociation]
            public static BuffDef GetBuffDef() => LITContent.Buffs.bdAffixBlighted;
            public BuffDef FirstEliteBuff => _first ? _second.eliteEquipmentDef.passiveBuffDef : null;
            public BuffDef SecondEliteBuff => _second ? _second.eliteEquipmentDef.passiveBuffDef : null;

            private EliteDef _first;
            private EliteDef _second;
            private NetworkedBodyAttachment _attachment;
            private BlightedBodyAttachment _blightedAttachment;
            private float _aiRandomizeEliteStopwatch;

            private void Start()
            {
                _attachment = Instantiate(_blightedBodyAttachment).GetComponent<NetworkedBodyAttachment>();
                _attachment.AttachToGameObjectAndSpawn(body.gameObject);
                _blightedAttachment = _attachment.GetComponent<BlightedBodyAttachment>();
                _aiRandomizeEliteStopwatch = LITContent.Equipments.AffixBlighted.cooldown;
            }

            public void RandomizeElites()
            {
                if (!NetworkServer.active)
                    return;

                _blightedAttachment.RandomizeElites();
            }
            
            private void FixedUpdate()
            {
                UpdateIndividual(ref _first, _blightedAttachment.FirstIndex);
                UpdateIndividual(ref _second, _blightedAttachment.SecondIndex);

                //Makes ai blighted elites shuffle their elites every 60 seconds.
                if(NetworkServer.active && !body.isPlayerControlled)
                {
                    _aiRandomizeEliteStopwatch -= Time.fixedDeltaTime;
                    if(_aiRandomizeEliteStopwatch < 0)
                    {
                        _aiRandomizeEliteStopwatch = LITContent.Equipments.AffixBlighted.cooldown;
                        RandomizeElites();
                    }
                }
            }

            private void UpdateIndividual(ref EliteDef eliteDef, EliteIndex index)
            {
                //If we dont have an eliteDef, and the incoming index is not none, update our current eliteDef
                if(!eliteDef)
                {
                    if(index != EliteIndex.None)
                    {
                        eliteDef = EliteCatalog.GetEliteDef(index);
                        if(NetworkServer.active && eliteDef.eliteEquipmentDef.passiveBuffDef)
                        {
                            body.AddBuff(eliteDef.eliteEquipmentDef.passiveBuffDef);
                        }
                    }
                }
                else  //However, if we do have an elitedef, and the incoming inddex is different, update buffs.
                {
                    if (eliteDef.eliteIndex == index)
                        return;

                    if(NetworkServer.active)
                        body.RemoveBuff(eliteDef.eliteEquipmentDef.passiveBuffDef);
    
                    eliteDef = EliteCatalog.GetEliteDef(index);
                    
                    if(NetworkServer.active)
                        body.AddBuff(eliteDef.eliteEquipmentDef.passiveBuffDef);
                }
            }

            private void OnDestroy()
            {
                if (_attachment)
                    Destroy(_attachment.gameObject);

                if (FirstEliteBuff)
                    body.RemoveBuff(FirstEliteBuff);
                if (SecondEliteBuff)
                    body.RemoveBuff(SecondEliteBuff);
            }
        }

        //Leaving this for the sake of seeing if we reimplement any of this shit later.
        /*
        public class AffixBlightedBehavior : BaseBuffBodyBehavior, IStatItemBehavior
        {

            private GameObject SmokeEffect = LITAssets.LoadAsset<GameObject>("BlightSmoke", LITBundle.Equips);

            private float stopwatch;
            private static float checkTimer = 0.5f;

            public void Start()
            {
                body.onSkillActivatedServer += RemoveBuff;
            }

            private void RemoveBuff(GenericSkill obj = null)
            {
                if (body.hasCloakBuff)
                    body.RemoveBuff(RoR2Content.Buffs.Cloak);
                SpawnEffect();
                stopwatch = 0f;
            }

            public void FixedUpdate()
            {
                stopwatch += Time.fixedDeltaTime;
                if (stopwatch > checkTimer && !body.HasBuff(RoR2Content.Buffs.Cloak))
                {
                    body.AddBuff(RoR2Content.Buffs.Cloak);
                }
                else if (Util.CheckRoll(1))
                {
                    stopwatch = 0; //Doing this to ensure they're visible for a moment
                    RemoveBuff();
                    SpawnEffect();
                }
            }

            private void SpawnEffect()
            {
                EffectData data = new EffectData
                {
                    scale = body.radius * 0.75f,
                    origin = body.transform.position
                };
                EffectManager.SpawnEffect(SmokeEffect, data, true);
                Util.PlaySound("BlightAppear", body.gameObject); too obnoxious
            }

            public void RecalculateStatsStart() { }

            public void RecalculateStatsEnd()
            {
                //Reduces cooldowns by 50%
                if (body.skillLocator.primary)
                    body.skillLocator.primary.cooldownScale *= 0.7f;
                if (body.skillLocator.secondary)
                    body.skillLocator.secondary.cooldownScale *= 0.7f;
                if (body.skillLocator.utility)
                    body.skillLocator.utility.cooldownScale *= 0.7f;
                if (body.skillLocator.special)
                    body.skillLocator.special.cooldownScale *= 0.7f;
                //Is there a reason you subtract CDR instead of multiply? If two things gave 0.5 CDR like this, then it'd have 0 CDR... Right?
                //Also, if these need nerfed, I say this is the first thing to go.
                //Neb: i supose i can change it to multiply by 0.5 if they need nerfed. we'll see in the future.
            }

            public void OnDestroy()
            {
                if (body)
                {
                    body.onSkillActivatedServer -= RemoveBuff;

                    body?.RemoveBuff(RoR2Content.Buffs.Cloak);
                }
            }
        }*/
    }
}