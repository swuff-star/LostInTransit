using MSU;
using RoR2;
using System;
using UnityEngine;
using UnityEngine.Networking;
using RoR2.Items;
using RoR2.ContentManagement;
using System.Collections;
using MSU.Config;

namespace LostInTransit.Items
{
    //It's called Photon Cannon because the Laser Turbine powers a Photon Power Plant (and also because Iron Man in MvC is cool as fuck)
#if DEBUG
    public sealed class PhotonCannon : LITItem
    {
        private const string TOKEN = "LIT_ITEM_PHOTONCANNON_DESC";

        [RiskOfOptionsConfigureField(LITConfig.ITEMS, ConfigDescOverride = "Amount of charge gained every second for each skill on cooldown")]
        [FormatToken(TOKEN)]
        public static float baseCharge = 1f;

        [RiskOfOptionsConfigureField(LITConfig.ITEMS, ConfigDescOverride = "Additional charge per turbine")]
        [FormatToken(TOKEN, 1)]
        public static float stackCharge = 0.5f;

        [RiskOfOptionsConfigureField(LITConfig.ITEMS, ConfigDescOverride = "Amount of damage the laser deals")]
        [FormatToken(TOKEN, 2)]
        public static float laserDamage = 2000f;

        [RiskOfOptionsConfigureField(LITConfig.ITEMS, ConfigNameOverride = "Use static charge timer", ConfigDescOverride = "if true, the turbine will gain charge as if one skill is on cooldown at all times")]
        public static bool skillIssue = false;

        public override NullableRef<GameObject> ItemDisplayPrefab => null;
        public override ItemDef ItemDef => _itemDef;
        private ItemDef _itemDef;

        public override void Initialize()
        {
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return false;
        }

        public override IEnumerator LoadContentAsync()
        {
            var assetRequest = LITAssets.LoadAssetAsync<ItemDef>("PhotonCannon", LITBundle.Items);

            assetRequest.StartLoad();
            while (!assetRequest.IsComplete)
                yield return null;

            _itemDef = assetRequest.Asset;
        }

        public class PhotonCannonBehavior : BaseItemBodyBehavior
        {
            [ItemDefAssociation(useOnClient = true, useOnServer = true)]
            public static ItemDef GetItemDef() => LITContent.Items.PhotonCannon;
            public float storedCharge = 0f;
            private float stopwatch = 0f;
            private float chargeMultiplier;
            private void FixedUpdate()
            {
                if (NetworkServer.active)
                {
                    stopwatch -= Time.fixedDeltaTime;
                    if (stopwatch < 0f)
                    {
                        stopwatch += 0.2f;
                        if (storedCharge >= 100f) FireLaser();
                        CalcCharge();
                    }
                }
            }
            private void CalcCharge()
            {
                if (skillIssue)
                {
                    storedCharge += (baseCharge + (stackCharge * (stack - 1))) * 0.2f;
                }
                else
                {
                    foreach (object obj in Enum.GetValues(typeof(SkillSlot)))
                    {
                        SkillSlot slot = (SkillSlot)obj;
                        GenericSkill skill = body.skillLocator.GetSkill(slot);
                        if (skill != null && skill.cooldownRemaining > 0) storedCharge += (baseCharge + (stackCharge * (stack - 1))) * 0.2f;
                    }
                }
            }
            private void FireLaser()
            {
                storedCharge = 0f;
                LITLog.Info("PEW PEW PEW"); //obviously laser code goes here
            }
        }
    }
#endif
}
