using MSU;
using MSU.Config;
using R2API;
using RoR2;
using RoR2.ContentManagement;
using RoR2.Items;
using System.Collections;
using UnityEngine;

namespace LostInTransit.Items
{
#if DEBUG
    public sealed class ArmsRace : LITItem, IContentPackModifier
    {
        private const string TOKEN = "LIT_ITEM_ARMSRACE_DESC";

        public override NullableRef<GameObject> ItemDisplayPrefab => null;

        public override ItemDef ItemDef => _itemDef;
        private ItemDef _itemDef;

        [RiskOfOptionsConfigureField(LITConfig.ITEMS, ConfigDescOverride = "Percentage of max health granted to drones as shield, per stack.")]
        public static float shieldAmount = 8f;

        [RiskOfOptionsConfigureField(LITConfig.ITEMS, ConfigDescOverride = "Whether or not drones should be given gated shields.")]
        public static bool shieldGating = true;

        private AssetCollection _assetCollection;

        public override void Initialize()
        {
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return false;
        }

        public override IEnumerator LoadContentAsync()
        {
            var request = LITAssets.LoadAssetAsync<AssetCollection>("acArmsRace", LITBundle.Items);

            request.StartLoad();
            while (!request.IsComplete)
                yield return null;

            _assetCollection = request.Asset;

            _itemDef = _assetCollection.FindAsset<ItemDef>("ArmsRace");
        }

        public void ModifyContentPack(ContentPack contentPack)
        {
            contentPack.itemDefs.AddSingle(_assetCollection.FindAsset<ItemDef>("ArmsRaceDroneModifiers"));
        }

        public class ArmsRaceBehavior : BaseItemBodyBehavior
        {
            public int previousStack = 0;

            [ItemDefAssociation(useOnClient = true, useOnServer = true)]
            public static ItemDef GetItemDef() => LITContent.Items.ArmsRace;

            public MinionOwnership minionOwnership;

            private void OnEnable()
            {
                UpdateAllMinions(stack);
                MasterSummon.onServerMasterSummonGlobal += OnServerMasterSummonGlobal;
                minionOwnership = body.GetComponent<MinionOwnership>();
            }

            public void FixedUpdate()
            {
                if (previousStack != stack)
                {
                    UpdateAllMinions(stack);
                }
            }

            public void UpdateAllMinions(int newStack)
            {
                if (body)
                {
                    CharacterBody body = this.body;
                    if ((body != null) ? body.master : null)
                    {
                        MinionOwnership.MinionGroup minionGroup = MinionOwnership.MinionGroup.FindGroup(this.body.master.netId);
                        if (minionGroup != null)
                        {
                            foreach (MinionOwnership minionOwnership in minionGroup.members)
                            {
                                if (minionOwnership)
                                {
                                    CharacterMaster component = minionOwnership.GetComponent<CharacterMaster>();
                                    if (component && component.inventory)
                                    {
                                        CharacterBody body2 = component.GetBody();
                                        if (body2)
                                        {
                                            UpdateMinionInventory(component.inventory, body2.bodyFlags, newStack);
                                        }
                                    }
                                }
                            }
                            previousStack = newStack;
                        }
                    }
                }
            }

            public void OnServerMasterSummonGlobal(MasterSummon.MasterSummonReport summonReport)
            {
                if (body && body.master && body.master == summonReport.leaderMasterInstance)
                {
                    CharacterMaster summonMasterInstance = summonReport.summonMasterInstance;
                    if (summonMasterInstance)
                    {
                        CharacterBody body = summonMasterInstance.GetBody();
                        if (body)
                        {
                            UpdateMinionInventory(summonMasterInstance.inventory, body.bodyFlags, stack);
                        }
                    }
                }
            }

            public void UpdateMinionInventory(Inventory inventory, CharacterBody.BodyFlags bodyFlags, int stack)
            {
                if (inventory && stack > 0 && (bodyFlags & CharacterBody.BodyFlags.Mechanical) > CharacterBody.BodyFlags.None)
                {
                    int itemCount = inventory.GetItemCount(LITContent.Items.ArmsRaceDroneMods);
                    if (itemCount < stack)
                    {
                        inventory.GiveItem(LITContent.Items.ArmsRaceDroneMods, stack - itemCount);
                    }
                    else if (itemCount > stack)
                    {
                        inventory.RemoveItem(LITContent.Items.ArmsRaceDroneMods, itemCount - stack);
                    }
                }
                else
                {
                    inventory.ResetItem(LITContent.Items.ArmsRaceDroneMods);
                }
            }

            public void OnDisable()
            {
                UpdateAllMinions(0);
                MasterSummon.onServerMasterSummonGlobal -= OnServerMasterSummonGlobal;
            }
        }

        public class ArmsRaceDroneModsBehavior : BaseItemBodyBehavior, IOnIncomingDamageServerReceiver, IBodyStatArgModifier
        {
            [ItemDefAssociation(useOnClient = true, useOnServer = true)]
            public static ItemDef GetItemDef() => LITContent.Items.ArmsRaceDroneMods;

            public void Awake()
            {
                base.Awake();
                body.RecalculateStats();
            }

            public void OnIncomingDamageServer(DamageInfo damageInfo)
            {
                if (body.healthComponent.shield >= 1f && damageInfo.damage >= body.healthComponent.shield + body.healthComponent.barrier)
                {
                    if (ArmsRace.shieldGating == true && !(damageInfo.damageType == DamageType.BypassArmor || damageInfo.damageType == DamageType.BypassOneShotProtection))
                    {
                        damageInfo.damage = body.healthComponent.shield + body.healthComponent.barrier;
                    }
                }
            }

            public void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args)
            {
                args.baseShieldAdd += (body.maxHealth * (0.01f * ArmsRace.shieldAmount * stack));
            }
        }
    }
#endif
}
