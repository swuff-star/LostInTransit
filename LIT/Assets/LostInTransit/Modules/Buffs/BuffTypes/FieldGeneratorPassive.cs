using Moonstorm;
using RoR2;
using R2API;
using RoR2.Items;
using Moonstorm.Components;

namespace LostInTransit.Buffs
{
    //[DisabledContent]
    public class FieldGeneratorPassive : BuffBase
    {
        public override BuffDef BuffDef { get; } = LITAssets.LoadAsset<BuffDef>("bdFieldGeneratorPassive", LITBundle.Equips);

        public class DiceAtkBehavior : BaseBuffBodyBehavior, /*IOnIncomingDamageOtherServerReciever,*/ IOnTakeDamageServerReceiver
        {
            [BuffDefAssociation(useOnServer = true, useOnClient = true)]
            public static BuffDef GetBuffDef() => LITContent.Buffs.bdFieldGeneratorPassive;

            /*public void OnIncomingDamageOther(HealthComponent victimHealthComponent, DamageInfo damageInfo)
            {
                if (damageInfo.damage >= victimHealthComponent.health)
                {
                    damageInfo.damage = victimHealthComponent.health - 1;
                    CharacterMasterNotificationQueue.PushEquipmentTransformNotification(body.master, body.inventory.currentEquipmentIndex, LITContent.Equipments.FieldGeneratorUsed.equipmentIndex, CharacterMasterNotificationQueue.TransformationType.Default);
                    body.inventory.SetEquipmentIndex(LITContent.Equipments.FieldGeneratorUsed.equipmentIndex);
                    body.AddTimedBuff(RoR2Content.Buffs.HiddenInvincibility, 8f);
                    
                }
            }*/

            public void OnTakeDamageServer(DamageReport damageReport)
            {
                if (damageReport.victimBody.healthComponent.health < 1)
                {
                    damageReport.victimBody.healthComponent.health = 1;
                    CharacterMasterNotificationQueue.PushEquipmentTransformNotification(body.master, body.inventory.currentEquipmentIndex, LITContent.Equipments.FieldGeneratorUsed.equipmentIndex, CharacterMasterNotificationQueue.TransformationType.Default);
                    body.inventory.SetEquipmentIndex(LITContent.Equipments.FieldGeneratorUsed.equipmentIndex);
                    body.AddTimedBuff(RoR2Content.Buffs.HiddenInvincibility, 8f);
                }
            }
        }
    }
}