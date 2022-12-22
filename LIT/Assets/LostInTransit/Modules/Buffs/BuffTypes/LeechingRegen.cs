using Moonstorm;
using Moonstorm.Components;
using RoR2;
using UnityEngine;
using R2API;
using UnityEngine.Networking;

namespace LostInTransit.Buffs
{
    /*[DisabledContent]
    public class LeechingRegen : BuffBase
    {
        public override BuffDef BuffDef { get; } = LITAssets.LoadAsset<BuffDef>("LeechingRegen");

        /*public override void Initialize()
        {
            var croco = Resources.Load<BuffDef>("buffdefs/CrocoRegen");
            BuffDef.iconSprite = croco.iconSprite;
            BuffDef.startSfx = croco.startSfx;
        }

        public class LeechingRegenBehavior : BaseBuffBodyBehavior, IBodyStatArgModifier
        {
            //[BuffDefAssociation(useOnClient = true, useOnServer = true)]
            //public static BuffDef GetBuffDef() => LITContent.Buffs.LeechingRegen;

            public float duration = 5;
            public float totalRegen = 0;
            public float regenStrength = 10;

            public GameObject VFX = LITAssets.LoadAsset<GameObject>("EffectLeechingRegen");
            public void Start()
            {
                VFX.GetComponent<DestroyOnTimer>().duration = duration;
                if(NetworkServer.active)
                {
                    EffectData effectData = new EffectData
                    {
                        scale = body.bestFitRadius / 10,
                        origin = body.aimOrigin,
                        rootObject = body.gameObject
                    };
                    EffectManager.SpawnEffect(VFX, effectData, true);
                }
                totalRegen = body.maxHealth * (regenStrength / 100) / duration;
                body.RecalculateStats();
            }
            public void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args)
            {
                args.baseRegenAdd += totalRegen;
            }
        }
    }*/
}

