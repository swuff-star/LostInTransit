using MSU;
using RoR2;
using RoR2.ContentManagement;
using System.Collections;
using UnityEngine;

namespace LostInTransit.Equipments
{
#if DEBUG
    public sealed class FieldGenerator : LITEquipment
    {
        public override NullableRef<GameObject> ItemDisplayPrefab => null;
        public override EquipmentDef EquipmentDef => _equipmentDef;
        private EquipmentDef _equipmentDef;
        private EquipmentDef _consumedEquipmentDef;

        private BuffDef _fieldGeneratorPassive;

        public override bool Execute(EquipmentSlot slot)
        {
            return false;
        }

        public override void Initialize()
        {
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return false;
        }

        public override IEnumerator LoadContentAsync()
        {
            /*
             * EquipmentDef - "FieldGenerator" - Equips
             * EquipmentDef - "FieldGeneratorUsed" - Equips
             * BuffDef - "FieldGeneratorPassive" - Equips
             */
            yield break;
        }

        public override void OnEquipmentLost(CharacterBody body)
        {
        }

        public override void OnEquipmentObtained(CharacterBody body)
        {
        }

        public class FieldGeneratorBehaviour : BuffBehaviour, IOnIncomingDamageOtherServerReciever, IOnTakeDamageServerReceiver
        {
            [BuffDefAssociation]
            public static BuffDef GetBuffDef() => LITContent.Buffs.bdFieldGeneratorPassive;

            public void OnIncomingDamageOther(HealthComponent victimHealthComponent, DamageInfo damageInfo)
            {
                if (damageInfo.damage >= victimHealthComponent.health)
                {
                    damageInfo.damage = victimHealthComponent.health - 1;
                    CharacterMasterNotificationQueue.PushEquipmentTransformNotification(CharacterBody.master, CharacterBody.inventory.currentEquipmentIndex, LITContent.Equipments.FieldGeneratorUsed.equipmentIndex, CharacterMasterNotificationQueue.TransformationType.Default);
                    CharacterBody.inventory.SetEquipmentIndex(LITContent.Equipments.FieldGeneratorUsed.equipmentIndex);
                    CharacterBody.AddTimedBuff(RoR2Content.Buffs.HiddenInvincibility, 8f);

                }
            }

            public void OnTakeDamageServer(DamageReport damageReport)
            {
                if (damageReport.victimBody.healthComponent.health < 1)
                {
                    damageReport.victimBody.healthComponent.health = 1;
                    CharacterMasterNotificationQueue.PushEquipmentTransformNotification(CharacterBody.master, CharacterBody.inventory.currentEquipmentIndex, LITContent.Equipments.FieldGeneratorUsed.equipmentIndex, CharacterMasterNotificationQueue.TransformationType.Default);
                    CharacterBody.inventory.SetEquipmentIndex(LITContent.Equipments.FieldGeneratorUsed.equipmentIndex);
                    CharacterBody.AddTimedBuff(RoR2Content.Buffs.HiddenInvincibility, 8f);
                }
            }
        }
    }
#endif
}
