using Moonstorm;
using RoR2;
using System;
using UnityEngine;
using R2API;
using RoR2.Items;
using System.Collections.Generic;
using UnityEngine.Networking;
using System.Timers;

namespace LostInTransit.Items
{
    public class RustyJetpack : ItemBase
    {
        private const string token = "LIT_ITEM_RUSTYJETPACK_DESC";
        public override ItemDef ItemDef { get; } = LITAssets.LoadAsset<ItemDef>("RustyJetpack", LITBundle.Items);

        [ConfigurableField(ConfigName = "Jump Power", ConfigDesc = "Added jump power per Jetpack, as a percentage of normal jump power. Halved after the first stack.")]
        [TokenModifier(token, StatTypes.Percentage, 0)]
        public static float addedJumpPower = 2f;

        [ConfigurableField(ConfigName = "Fall Speed Reduction", ConfigDesc = "Amount of gravity removed, as a pecent")]
        [TokenModifier(token, StatTypes.Percentage, 2)]
        public static float reducedGravity = 0.35f;

        [ConfigurableField(ConfigName = "Fall Speed Limit", ConfigDesc = "Maximum amount fall speed can be reduced by, in percent")]
        public static float minGrav = 90f;

        public class RustyJetpackBehavior : BaseItemBodyBehavior, IBodyStatArgModifier
        {
            [ItemDefAssociation(useOnServer = true, useOnClient = true)]
            public static ItemDef GetItemDef() => LITContent.Items.RustyJetpack;
            private CharacterModel model;
            private List<GameObject> displayList;
            private GameObject displayObject;
            private ChildLocator displayCL;
            private GameObject jetsSmall;
            private GameObject jetsLarge;
            private GameObject jetR;
            private GameObject jetL;

            int jumpTimes = 0;

            private bool hasTriedToSetPrefab = false;
            public void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args)
            {
                //args.jumpPowerMultAdd += stack * addedJumpPower;
                args.baseJumpPowerAdd += stack * addedJumpPower;
            }
            private void Reset(ref CharacterMotor.HitGroundInfo hitGroundInfo)
            {
                jumpTimes = 0;
            }

            private void Start()
            {
                if (NetworkServer.active)
                    body.characterMotor.onHitGroundServer += Reset;
                else
                    body.characterMotor.onHitGroundAuthority += Reset;

                model = body.modelLocator.modelTransform.GetComponent<CharacterModel>();

                if (model != null)
                {
                    displayList = model.GetItemDisplayObjects(LITContent.Items.RustyJetpack.itemIndex);

                    if (displayList != null)
                    {
                        displayObject = displayList[0];
                        if (displayObject != null)
                        {
                            displayCL = displayObject.GetComponent<ChildLocator>();
                            if (displayCL != null)
                            {
                                jetsSmall = displayCL.FindChild("Jets").gameObject;
                                jetsLarge = displayCL.FindChild("JetsBig").gameObject;
                            }
                        }
                    }
                }
            }

            private void FixedUpdate()
            {
                if (!body.characterMotor || !body)
                    return;

                if (hasTriedToSetPrefab == false)
                {
                    hasTriedToSetPrefab = true;

                    model = body.modelLocator.modelTransform.GetComponent<CharacterModel>();

                    if (model != null)
                    {
                        displayList = model.GetItemDisplayObjects(LITContent.Items.RustyJetpack.itemIndex);
                        //Debug.Log("found model");
                        if (displayList != null)
                        {
                            displayObject = displayList[0];
                            //Debug.Log("found display list");
                            if (displayObject != null)
                            {
                                displayCL = displayObject.GetComponent<ChildLocator>();
                                //Debug.Log("found display object");
                                if (displayCL != null)
                                {
                                    //Debug.Log("found display cl");
                                    jetsSmall = displayCL.FindChild("Jets").gameObject;
                                    jetsLarge = displayCL.FindChild("JetsBig").gameObject;
                                }
                            }
                        }
                    }
                }

                if (body.inputBank.jump.justPressed && jumpTimes < body.maxJumpCount)
                {
                    jumpTimes++;
                    jetsLarge.SetActive(true);
                }

                if (body.characterMotor.isGrounded)
                {
                    return;
                }

                if (body.inputBank.jump.down && !body.characterMotor.isGrounded)
                    jetsSmall.SetActive(true);
                else
                    jetsSmall.SetActive(false);

                if (body.inputBank.jump.down)
                {
                    body.characterMotor.velocity.y -= Time.fixedDeltaTime * Physics.gravity.y * reducedGravity;
                    
                }    
            }
        }
    }
}
