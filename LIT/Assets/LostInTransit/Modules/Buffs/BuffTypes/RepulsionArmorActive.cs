using LostInTransit.Items;
using Moonstorm;
using RoR2;
using Moonstorm.Components;
using R2API;
using RoR2.Projectile;
using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.Networking;
using System.Collections;

namespace LostInTransit.Buffs
{
    //[DisabledContent]
    public class RepulsionArmorActive : BuffBase
    {
        public override BuffDef BuffDef { get; } = LITAssets.LoadAsset<BuffDef>("RepulsionArmorActive");
        //private Ray aimRay;

        public class RepulsionArmorActiveBehavior : BaseBuffBodyBehavior, IBodyStatArgModifier
        {
            [BuffDefAssociation(useOnClient = false, useOnServer = true)]
            public static BuffDef GetBuffDef() => LITContent.Buffs.RepulsionArmorActive;

            public void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args)
            {
                args.armorAdd += RepulsionArmor.damageResist;
            }

            public void FixedUpdate()       //★ i think this works because of a bug; working is working!
            {
                Collider[] array = Physics.OverlapSphere(body.corePosition, 2f, LayerIndex.projectile.mask);

                for (int i = 0; i < array.Length; i++)
                {
                    ProjectileController pc = array[i].GetComponentInParent<ProjectileController>();
                    if (pc)
                    {
                        if (pc.owner != gameObject)
                        {
                            pc.owner = gameObject;

                            FireProjectileInfo info = new FireProjectileInfo()
                            {
                                projectilePrefab = pc.gameObject,
                                position = pc.gameObject.transform.position,
                                rotation = Quaternion.Inverse(pc.gameObject.transform.rotation),
                                owner = body.gameObject,
                                damage = body.damage * 5f,
                                force = 200f,
                                crit = true,
                                damageColorIndex = DamageColorIndex.Default,
                                target = null,
                                speedOverride = 120f,
                                fuseOverride = -1
                            };
                            ProjectileManager.instance.FireProjectile(info);

                            Destroy(pc.gameObject);
                        }
                    }
                }
            }

            public void OnDestroy()
            {
                body.SetBuffCount(LITContent.Buffs.RepulsionArmorCD.buffIndex, (int)RepulsionArmor.hitsNeededConfig);
            }

            /*public void OnIncomingDamageServer(DamageInfo damageInfo)
            {
                damageInfo.damage *= ((100f - RepulsionArmor.damageResist) * 0.01f);
            }*/
        }
    }
}