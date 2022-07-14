using Moonstorm;
using RoR2;
using RoR2.Items;
using UnityEngine;
using R2API;
using System;

namespace LostInTransit.Items
{
    [DisabledContent]
    public class FireShield : ItemBase
    {
        private const string token = "LIT_ITEM_FIRESHIELD_DESC";
        public override ItemDef ItemDef { get; } = LITAssets.LoadAsset<ItemDef>("FireShield");

        //to-do: stacking, config

        public class FireShieldBehavior : BaseItemBodyBehavior, IOnIncomingDamageServerReceiver
        {
            [ItemDefAssociation(useOnClient = true, useOnServer = true, behaviorTypeOverride = typeof(FireShieldBehavior))]
            public static ItemDef GetItemDef() => LITContent.Items.FireShield;

            public void OnIncomingDamageServer(DamageInfo damageInfo)
            {
                if (damageInfo.damage >= (body.maxHealth / 10))
                {
                    new BlastAttack
                    {
                        position = body.corePosition,
                        baseDamage = body.damage * 3f,
                        baseForce = 3000f,
                        bonusForce = Vector3.up * 1500f,
                        radius = 8f,
                        attacker = body.gameObject,
                        inflictor = body.gameObject,
                        crit = Util.CheckRoll(body.crit, body.master),
                        damageColorIndex = DamageColorIndex.Item,
                        falloffModel = BlastAttack.FalloffModel.Linear,
                        attackerFiltering = AttackerFiltering.NeverHitSelf,
                        teamIndex = body.teamComponent.teamIndex,
                        damageType = DamageType.IgniteOnHit,
                        procCoefficient = 1.0f,
                    }.Fire();
                }
            }
        }
    }
}
