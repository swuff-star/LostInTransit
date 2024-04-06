using LostInTransit.Items;
using MSU;
using RoR2;
using RoR2.ContentManagement;
using System.Collections;
using UnityEngine;

namespace LostInTransit.Equipments
{
#if DEBUG
    public sealed class UnstableWatch : LITEquipment
    {
        public override NullableRef<GameObject> ItemDisplayPrefab => null;
        public override EquipmentDef EquipmentDef => _equipmentDef;
        private EquipmentDef _equipmentDef;

        private static GameObject _buffWard;
        private BuffDef _timeStop;
        private BuffDef _timeStopDebuff;
        public override bool Execute(EquipmentSlot slot)
        {
            float timeMult = BeatingEmbryoManager.BeatingEmbryoProcs(slot) ? 1 : 2;
            slot.characterBody.AddTimedBuffAuthority(LITContent.Buffs.bdTimeStop.buffIndex, 8 * timeMult);
            return true;
        }

        public override void Initialize()
        {
            On.RoR2.CharacterBody.RecalculateStats += DoSlow;
        }

        private void DoSlow(On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody self)
        {
            if (self.HasBuff(LITContent.Buffs.bdTimeStopDebuff))
            {
                self.moveSpeed *= 0f;
                self.attackSpeed *= 0f;
            }
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public override IEnumerator LoadContentAsync()
        {
            /*
             * EquipmentDef - "UnstableWatch" - Equips
             * BuffDef - "bdTimeStop" - Equips
             * GameObject - "TimeStopSphere" - Equips
             * BuffDef - "bdTimeStopDebuff" - Equips
             */
            yield break;
        }

        public override void OnEquipmentLost(CharacterBody body)
        {
        }

        public override void OnEquipmentObtained(CharacterBody body)
        {
        }

        public class TimeStopBehavior : BuffBehaviour
        {
            [BuffDefAssociation()]
            public static BuffDef GetBuffDef() => LITContent.Buffs.bdTimeStop;
            private GameObject wardInstance;

            public void OnEnable()
            {
                wardInstance = Instantiate(_buffWard);
                wardInstance.GetComponent<TeamFilter>().teamIndex = CharacterBody.teamComponent.teamIndex;
                wardInstance.GetComponent<NetworkedBodyAttachment>().AttachToGameObjectAndSpawn(gameObject);
            }

            public void OnDisable()
            {
                if (wardInstance != null)
                    Destroy(wardInstance);
            }
        }
    }
#endif
}                                                                  
