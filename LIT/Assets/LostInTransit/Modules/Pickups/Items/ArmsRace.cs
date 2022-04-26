using Moonstorm;
using RoR2;
using RoR2.Items;

namespace LostInTransit.Items
{
    public class ArmsRace : ItemBase
    {
        private const string token = "LIT_ITEM_ARMSRACE_DESC";
        public override ItemDef ItemDef { get; } = LITAssets.Instance.MainAssetBundle.LoadAsset<ItemDef>("ArmsRace");

        [ConfigurableField(ConfigName = "Shield Amount", ConfigDesc = "Percentage of max health granted to drones as shield, per stack.")]
        public static float extraShield = 8f;
        
        [ConfigurableField(ConfigName = "Shield Gating", ConfigDesc = "Whether or not drones should be given gated shields.")]
        public static bool shieldGating = true;

        public class ArmsRaceBehavior : BaseItemBodyBehavior
        {
            public int previousStack;

            [ItemDefAssociation(useOnClient = true, useOnServer = true)]
            public static ItemDef GetItemDef() => LITContent.Items.ArmsRace;

            public void Start()
            {
                UpdateAllMinions(stack);
                MasterSummon.onServerMasterSummonGlobal += OnServerMasterSummonGlobal;
            }

            public void FixedUpdate()
            {
                if (previousStack != stack)
                {
                    UpdateAllMinions(stack);
                }    
            }

            public void UpdateAllMinions(int stack)
            {
                if (body)
                {
                    if ((body != null) ? body.master : null)
                    {
                        MinionOwnership.MinionGroup minionGroup = MinionOwnership.MinionGroup.FindGroup(body.master.netId);
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
                                            UpdateMinionInventory(component.inventory, body2.bodyFlags, stack);
                                        }
                                    }
                                }
                            } 
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
                        inventory.RemoveItem(LITContent.Items.ArmsRaceDroneMods, stack - itemCount);
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
    }
}
