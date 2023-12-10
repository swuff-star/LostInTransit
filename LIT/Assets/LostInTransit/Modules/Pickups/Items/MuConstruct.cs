using Moonstorm;
using RoR2;
using R2API;
using RoR2.Items;

namespace LostInTransit.Items
{
    [DisabledContent]
    public class MuConstruct : ItemBase
    {
        private const string token = "LIT_ITEM_MUCONSTRUCT_DESC";
        public override ItemDef ItemDef { get; } = LITAssets.LoadAsset<ItemDef>("MuConstruct", LITBundle.Items);

        public override void Initialize()
        {
            base.Initialize();

            //ty ClassicItemsReturns for specific use cases
            On.RoR2.TeleporterInteraction.ChargingState.OnEnter += ChargingState_OnEnter;
            On.EntityStates.InfiniteTowerSafeWard.Active.OnEnter += Active_OnEnter;
            On.EntityStates.Missions.BrotherEncounter.BrotherEncounterPhaseBaseState.OnEnter += BrotherEncounterPhaseBaseState_OnEnter;
            On.RoR2.VoidRaidEncounterController.Start += VoidRaidEncounterController_Start;

            Stage.onStageStartGlobal += Stage_onStageStartGlobal;
            On.RoR2.TeleporterInteraction.ChargingState.OnExit += ChargingState_OnExit;
            On.EntityStates.InfiniteTowerSafeWard.Travelling.OnEnter += Travelling_OnEnter;
        }


        private void VoidRaidEncounterController_Start(On.RoR2.VoidRaidEncounterController.orig_Start orig, VoidRaidEncounterController self)
        {
            orig(self);
            MuConstructBehavior.constructActive = true;
        }

        private void BrotherEncounterPhaseBaseState_OnEnter(On.EntityStates.Missions.BrotherEncounter.BrotherEncounterPhaseBaseState.orig_OnEnter orig, EntityStates.Missions.BrotherEncounter.BrotherEncounterPhaseBaseState self)
        {
            orig(self);
            MuConstructBehavior.constructActive = true;
        }

        private void ChargingState_OnEnter(On.RoR2.TeleporterInteraction.ChargingState.orig_OnEnter orig, EntityStates.BaseState self)
        {
            orig(self);
            MuConstructBehavior.constructActive = true;
        }

        private void Active_OnEnter(On.EntityStates.InfiniteTowerSafeWard.Active.orig_OnEnter orig, EntityStates.InfiniteTowerSafeWard.Active self)
        {
            orig(self);
            MuConstructBehavior.constructActive = true;
        }

        private void ChargingState_OnExit(On.RoR2.TeleporterInteraction.ChargingState.orig_OnExit orig, EntityStates.BaseState self)
        {
            orig(self);
            MuConstructBehavior.constructActive = false;
        }

        private void Travelling_OnEnter(On.EntityStates.InfiniteTowerSafeWard.Travelling.orig_OnEnter orig, EntityStates.InfiniteTowerSafeWard.Travelling self)
        {
            orig(self);
            MuConstructBehavior.constructActive = false;
        }

        private void Stage_onStageStartGlobal(Stage obj)
        {
            MuConstructBehavior.constructActive = false;
        }
    }

    public class MuConstructBehavior : BaseItemBodyBehavior
    {
        [ItemDefAssociation(useOnClient = true, useOnServer = true)]
        public static ItemDef GetItemDef() => LITContent.Items.MuConstruct;
        public static bool constructActive = false;
        public void FixedUpdate()
        {
            if (constructActive)
            {

            }
        }

        public void ActivateMu()
        {

        }
    }
}

