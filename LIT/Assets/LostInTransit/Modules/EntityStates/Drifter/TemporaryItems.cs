using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoR2;
using UnityEngine;
using LostInTransit.Components;
using UnityEngine.Networking;
using LostInTransit;

namespace EntityStates.Drifter
{
    public class TemporaryItems : BaseSkillState
    {
        public static float baseEntryDuration = 1f;
        public static float baseDropDuration = 1f;
        public static float dropletSpreadAngle = 30f;
        public static float dropletUpVelocity = 20f;
        public static float dropletForwardVelocity = 8f;

        public static float itemDuration = 80f;

        [NonSerialized]
        // drop tables dont serialize i guess ?
        public static PickupDropTable dropTable = LITAssets.LoadAsset<PickupDropTable>("DrifterDropTable", LITBundle.Characters);

        public static int numItems = 4;

        public static int scrapcost = 10;

        private DrifterScrapComponent drifterScrapComponent;

        private float duration;
        private float itemDropInterval;
        private float itemDropStopwatch;
        private int itemsDropped;
        private Xoroshiro128Plus rng;
        public override void OnEnter()
        {
            base.OnEnter();

            if (NetworkServer.active)
            {
                this.rng = new Xoroshiro128Plus(Run.instance.treasureRng.nextUlong);
            }

            drifterScrapComponent = base.GetComponent<DrifterScrapComponent>();
            if (!drifterScrapComponent)
            {
                Debug.Log("dsc is null!");
                outer.SetNextStateToMain();
                return;
            }

            //anim
            //sound

            drifterScrapComponent.AddScrap(-scrapcost);

            this.duration = baseDropDuration;
            this.itemDropInterval = this.duration / numItems;
        }


        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (base.fixedAge <= baseEntryDuration)
                return;

            this.itemDropStopwatch -= Time.fixedDeltaTime;
            if(this.itemDropStopwatch <= 0)
            {
                float angle = (-numItems / 2 + itemsDropped) * dropletSpreadAngle + dropletSpreadAngle / 2;
                itemDropStopwatch += itemDropInterval;
                itemsDropped++;

                if(NetworkServer.active)
                    this.DropItem(angle);
            }

            if(this.itemsDropped >= numItems)
            {
                this.outer.SetNextStateToMain();
            }
        }

        private void DropItem(float angle)
        {
            PickupIndex pickupIndex = dropTable.GenerateDrop(this.rng);
            pickupIndex = RoR2.Items.RandomlyLunarUtils.CheckForLunarReplacement(pickupIndex, this.rng);

            ItemIndex itemIndex = PickupCatalog.GetPickupDef(pickupIndex).itemIndex;
            itemIndex = LITTempItems.CheckForTemporaryReplacement(itemIndex);

            if(itemIndex == ItemIndex.None)
            {
                LITLog.Error(pickupIndex.GetPickupNameToken() + " did not have a suitable temporary item replacement!");
                return;
            }
            pickupIndex = PickupCatalog.FindPickupIndex(itemIndex);

            Vector3 direction = Quaternion.Euler(0, angle, 0) * base.inputBank.aimDirection;
            Vector3 velocity = Vector3.up * TemporaryItems.dropletUpVelocity + direction * TemporaryItems.dropletForwardVelocity;
            Transform origin = base.transform; // BAG MUZZLEEEEEEEEEEEEEEEEEEEEEEEEE

            LITTempItems.CreateTemporaryItemDroplet(pickupIndex, origin.position, velocity, itemDuration);
        }


    }
}
