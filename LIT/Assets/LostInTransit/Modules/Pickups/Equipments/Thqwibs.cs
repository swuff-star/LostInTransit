using LostInTransit.Items;
using Moonstorm;
using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace LostInTransit.Equipments
{
    //[DisabledContent]
    public class Thqwib : EquipmentBase
    {
        private const string token = "LIT_EQUIP_THQWIB_DESC";
        public override EquipmentDef EquipmentDef { get; } = LITAssets.LoadAsset<EquipmentDef>("Thqwib", LITBundle.Equips);

        [ConfigurableField(LITConfig.equips, ConfigName = "Damage per Thqwib", ConfigDesc = "Amount of damage each Thqwib deals on explosion, as a %.")]
        [TokenModifier(token, StatTypes.Default, 1)]
        public static float damage = 200;

        [ConfigurableField(LITConfig.equips, ConfigName = "Number of Thqwibs", ConfigDesc = "Number of Thqwibs tossed in a single bloom.")]
        [TokenModifier(token, StatTypes.Default, 0)]
        public static int thqwibAmount = 30;

        [ConfigurableField(LITConfig.equips, ConfigName = "Chance to Proc On-Kill Effects", ConfigDesc = "Chance, per Thqwib, to activate On-Kill effects when exploding.\nDefault Average: 30x * 10% = 3 average On-Kill activations per bloom. Fun (OP) with Soulbound Catalyst.")]
        [TokenModifier(token, StatTypes.Default, 2)]
        public static float chance = 0f;

        public override bool FireAction(EquipmentSlot slot)
        {
            Projectiles.ThqwibProjectile.ThqwibProj.GetComponent<ProjectileChanceForOnKillOnDestroy>().chance = chance;

            InputBankTest inputBank = slot.inputBank;
            CharacterBody charBody = slot.characterBody;
            if (inputBank && charBody)
            {
                Ray aimRay = new Ray(inputBank.aimOrigin, inputBank.aimDirection);
                Ray ray = aimRay;
                Ray ray2 = aimRay;
                Vector3 point = aimRay.GetPoint(10);
                bool flag = false;
                if (Util.CharacterRaycast(slot.gameObject, ray, out var hitInfo, 500f, (int)LayerIndex.world.mask | (int)LayerIndex.entityPrecise.mask, QueryTriggerInteraction.Ignore))
                {
                    point = hitInfo.point;
                    flag = true;
                }
                float magnitude = 40;
                if (flag)
                {
                    Vector3 vector = point - ray2.origin;
                    Vector2 vector2 = new Vector2(vector.x, vector.z);
                    float magnitude2 = vector2.magnitude;
                    Vector2 vector3 = vector2 / magnitude2;
                    if (magnitude2 < 10)
                    {
                        magnitude2 = 10;
                    }
                    float y = Trajectory.CalculateInitialYSpeed(1, vector.y);
                    float num = magnitude2 / 1;
                    Vector3 direction = new Vector3(vector3.x * num, y, vector3.y * num);
                    magnitude = direction.magnitude;
                    ray2.direction = direction;
                }
                float damageMult = BeatingEmbryoManager.Procs(slot) ? 2 : 1;
                for (int i = 0; i < thqwibAmount; i++)
                {
                    Quaternion rotation = Util.QuaternionSafeLookRotation(Util.ApplySpread(ray2.direction, 0, 25, 1f, 1f));
                    ProjectileManager.instance.FireProjectile(Projectiles.ThqwibProjectile.ThqwibProj, ray2.origin, rotation, slot.gameObject, charBody.damage * (damage / 100) * damageMult, 0f, Util.CheckRoll(charBody.crit, charBody?.master), DamageColorIndex.Default, null, magnitude);
                }
                return true;
            }
            return false;
        }
    }
}
