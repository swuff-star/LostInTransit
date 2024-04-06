using MSU;
using RoR2;
using RoR2.ContentManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace LostInTransit.Items
{
    public class LockedJewel : LITItem
    {
        public static float barrierGain = 20;
        public static int moneyGain = 8;

        public override NullableRef<GameObject> ItemDisplayPrefab => null;

        public override ItemDef ItemDef => _itemDef;
        private ItemDef _itemDef;

        public override void Initialize()
        {
            GlobalEventManager.OnInteractionsGlobal += GlobalEventManager_OnInteractionsGlobal;
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public override IEnumerator LoadContentAsync()
        {
            /*
             * ItemDef - "LockedJewel" - Items
             */
            yield break;
        }

        private void GlobalEventManager_OnInteractionsGlobal(Interactor arg1, IInteractable arg2, UnityEngine.GameObject arg3)
        {
            if (!NetworkServer.active)
                return;

            if (!MSUtil.IsInteractableValidForSpawns(arg3))
                return;

            if (!arg1.TryGetComponent<CharacterBody>(out var body))
                return;

            var itemCount = body.GetItemCount(ItemDef);
            if (itemCount == 0)
                return;

            var healthComponent = body.healthComponent;
            if (!healthComponent)
                return;

            var maxBarrier = healthComponent.fullBarrier;
            var barrierPercentage = (barrierGain + ((barrierGain / 2) * (itemCount - 1))) / 100;
            healthComponent.AddBarrier(Mathf.Min(maxBarrier, maxBarrier * barrierPercentage));

            if (body.master)
                body.master.GiveMoney((uint)Run.instance.GetDifficultyScaledCost(moneyGain));
        }
    }
}