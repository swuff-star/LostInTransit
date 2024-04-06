using MSU;
using RoR2;
using RoR2.Items;
using UnityEngine;
using R2API;
using System;
using UnityEngine.AddressableAssets;
using RoR2.ContentManagement;
using System.Collections;
using R2API;
using static R2API.DamageAPI;
using MSU.Config;

namespace LostInTransit.Items
{
    public sealed class FireShield : LITItem
    {
        private const string TOKEN = "LIT_ITEM_FIRESHIELD_DESC";
        public static DamageAPI.ModdedDamageType FireShieldDamageType { get; private set; }
        public override NullableRef<GameObject> ItemDisplayPrefab => null;
        public override ItemDef ItemDef => _itemDef;
        private ItemDef _itemDef;

        private static GameObject _explosionVFX;

        [RiskOfOptionsConfigureField(LITConfig.ITEMS, ConfigDescOverride = "Base damage dealt by Fire Shield.")]
        [FormatToken(TOKEN)]
        public static float baseDamageCoefficient = 3f;

        [RiskOfOptionsConfigureField(LITConfig.ITEMS, ConfigDescOverride = "Added burn damage per stack.")]
        [FormatToken(TOKEN, 1)]
        public static float burnDamageCoefficient = 1f;
        public override void Initialize()
        {
            FireShieldDamageType = DamageAPI.ReserveDamageType();

            GlobalEventManager.onServerDamageDealt += Ignite;
        }

        private void Ignite(DamageReport report)
        {
            CharacterBody attackerBody = report.attackerBody;
            DamageInfo damageInfo = report.damageInfo;
            if (DamageAPI.HasModdedDamageType(damageInfo, FireShieldDamageType))
            {
                var dotInfo = new InflictDotInfo()
                {
                    attackerObject = attackerBody.gameObject,
                    victimObject = report.victim.gameObject,
                    dotIndex = DotController.DotIndex.Burn,
                    duration = 2f,
                    damageMultiplier = attackerBody.GetItemCount(LITContent.Items.FireShield) * Items.FireShield.burnDamageCoefficient
                };
                StrengthenBurnUtils.CheckDotForUpgrade(report.attackerBody.inventory, ref dotInfo);
                DotController.InflictDot(ref dotInfo);
            }
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return false;
        }

        public override IEnumerator LoadContentAsync()
        {
            // ItemDef - "FireShield" - Items

            var request = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/VFX/ExplosionVFX.prefab");
            while(!request.IsDone)
                yield return null;

            _explosionVFX = request.Result;
        }

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
                        baseDamage = body.damage * baseDamageCoefficient,
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
                    DamageAPI.AddModdedDamageType(blastAttack, FireShieldDamageType);
                    blastAttack.Fire();

                    EffectData effectData = new EffectData
                    {
                        origin = body.corePosition,
                        scale = 4.5f,
                        rotation = new Quaternion(90, 0, 0, 0)
                    };
                    EffectManager.SpawnEffect(_explosionVFX, effectData, true);
                }
            }
        }
    }
}