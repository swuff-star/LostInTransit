using Moonstorm;
using RoR2;
using RoR2.Items;
using UnityEngine;
using R2API;
using System;
using UnityEngine.AddressableAssets;

namespace LostInTransit.Items
{
    //[DisabledContent]
    public class FireShield : ItemBase
    {
        private const string token = "LIT_ITEM_FIRESHIELD_DESC";
        public override ItemDef ItemDef { get; } = LITAssets.LoadAsset<ItemDef>("FireShield", LITBundle.Items);

        [ConfigurableField(ConfigName = "Base Damage Coefficient", ConfigDesc = "Base damage dealt by Fire Shield.")]
        [TokenModifier(token, StatTypes.Default, 1)]
        public static float dmgCoef = 3f;

        [ConfigurableField(ConfigName = "Burn Damage Coefficient", ConfigDesc = "Added burn damage per stack.")]
        [TokenModifier(token, StatTypes.Default, 1)]
        public static float burnCoef = 1f;

        //to-do: stacking, config

        public class FireShieldBehavior : BaseItemBodyBehavior, IOnIncomingDamageServerReceiver
        {
            [ItemDefAssociation(useOnClient = true, useOnServer = true, behaviorTypeOverride = typeof(FireShieldBehavior))]
            public static ItemDef GetItemDef() => LITContent.Items.FireShield;
            private BlastAttack blastAttack;

            public void OnIncomingDamageServer(DamageInfo damageInfo)
            {
                if (damageInfo.damage >= (body.maxHealth / 10))
                {
                    blastAttack = new BlastAttack()
                    {
                        position = body.corePosition,
                        baseDamage = body.damage * dmgCoef,
                        baseForce = 2000f,
                        bonusForce = Vector3.up * 750f,
                        radius = 2f,
                        attacker = body.gameObject,
                        inflictor = body.gameObject,
                        crit = Util.CheckRoll(body.crit, body.master),
                        damageColorIndex = DamageColorIndex.Item,
                        falloffModel = BlastAttack.FalloffModel.Linear,
                        attackerFiltering = AttackerFiltering.NeverHitSelf,
                        teamIndex = body.teamComponent.teamIndex,
                        procCoefficient = 1.0f
                    };
                    DamageAPI.AddModdedDamageType(blastAttack, DamageTypes.FireShield.fireShield);
                    blastAttack.Fire();

                    //EffectManager.SimpleEffect(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/VFX/ExplosionVFX.prefab").WaitForCompletion(), body.transform.position, Quaternion.identity.normalized, true);

                    EffectData effectData = new EffectData
                    {
                        origin = body.corePosition,
                        scale = 4.5f,
                        rotation = new Quaternion(90, 0, 0, 0)
                    };
                    EffectManager.SpawnEffect(Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/VFX/ExplosionVFX.prefab").WaitForCompletion(), effectData, true);
                }
            }
        }
    }
}
