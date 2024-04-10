using MSU;
using RoR2;
using System;
using UnityEngine;
using R2API;
using RoR2.Items;
using System.Collections.Generic;
using UnityEngine.Networking;
using System.Timers;
using RoR2.ContentManagement;
using System.Collections;
using MSU.Config;

namespace LostInTransit.Items
{
    public sealed class RustyJetpack : LITItem
    {
        private const string TOKEN = "LIT_ITEM_RUSTYJETPACK_DESC";

        [RiskOfOptionsConfigureField(LITConfig.ITEMS, ConfigDescOverride = "Added jump power per Jetpack, as a percentage of normal jump power. Halved after the first stack.")]
        [FormatToken(TOKEN, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100)]
        public static float jumpPower = 2f;

        [RiskOfOptionsConfigureField(LITConfig.ITEMS, ConfigDescOverride = "Amount of gravity removed, as a pecent")]
        [FormatToken(TOKEN, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 2)]
        public static float reducedGravity = 0.35f;

        [RiskOfOptionsConfigureField(LITConfig.ITEMS, ConfigDescOverride = "Maximum amount fall speed can be reduced by, in percent")]
        public static float minGravity = 90f;

        public override NullableRef<GameObject> ItemDisplayPrefab => null;
        public override ItemDef ItemDef => _itemDef;
        private ItemDef _itemDef;

        public override void Initialize()
        {
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public override IEnumerator LoadContentAsync()
        {
            var request = LITAssets.LoadAssetAsync<ItemDef>("RustyJetpack", LITBundle.Items);

            request.StartLoad();
            while (!request.IsComplete)
                yield return null;

            _itemDef = request.Asset;
        }

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
                args.baseJumpPowerAdd += stack * jumpPower;
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
                        if (displayList != null)
                        {
                            displayObject = displayList[0];
                            if (displayObject != null)
                            {
                                if (displayCL != null)
                                {
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
