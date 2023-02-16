using Moonstorm;
using RoR2;
using System;
using UnityEngine;
using UnityEngine.Networking;
using RoR2.Items;

namespace LostInTransit.Items
{
    //It's called Photon Cannon because the Laser Turbine powers a Photon Power Plant (and also because Iron Man in MvC is cool as fuck)
    [DisabledContent]
    public class PhotonCannon : ItemBase
    {
        private const string token = "LIT_ITEM_PHOTONCANNON_DESC";
        public override ItemDef ItemDef { get;} = LITAssets.LoadAsset<ItemDef>("PhotonCannon");

        [ConfigurableField(ConfigName = "Charge gained per second", ConfigDesc = "Amount of charge gained every second for each skill on cooldown")]
        [TokenModifier(token, StatTypes.Default, 0)]
        public static float baseCharge = 1f;

        [ConfigurableField(ConfigName = "Bonus charge from stacks", ConfigDesc = "Additional charge per turbine")]
        [TokenModifier(token, StatTypes.Default, 1)]
        public static float stackCharge = 0.5f;

        [ConfigurableField(ConfigName = "Laser damage", ConfigDesc = "Amount of damage the laser deals")]
        [TokenModifier(token, StatTypes.Default, 2)]
        public static float laserDamage = 2000f;

        [ConfigurableField(ConfigName = "Use static charge timer", ConfigDesc = "if true, the turbine will gain charge as if one skill is on cooldown at all times")]
        public static bool skillIssue = false;

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
                    storedCharge += (baseCharge + (stackCharge * (stack - 1)))*0.2f;
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
                Debug.Log("PEW PEW PEW"); //obviously laser code goes here
            }
        }
    }
}
