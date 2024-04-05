using LostInTransit.Items;
using MSU;
using MSU.Config;
using R2API;
using RoR2;
using RoR2.ContentManagement;
using RoR2.Projectile;
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace LostInTransit.Equipments
{
    //[DisabledContent]
    public class Thqwib : LITEquipment, IContentPackModifier
    {
        private const string token = "LIT_EQUIP_THQWIB_DESC";

        [RiskOfOptionsConfigureField(LITConfig.EQUIPS, ConfigDescOverride = "Number of Thqwibs tossed in a single bloom.")]
        [FormatToken(token)]
        public static int numberOfThqwibs = 20;

        [RiskOfOptionsConfigureField(LITConfig.EQUIPS, ConfigDescOverride = "Amount of damage each Thqwib deals on explosion, as a %.")]
        [FormatToken(token, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 1)]
        public static float damagePerThqwib = 2;

        [RiskOfOptionsConfigureField(LITConfig.EQUIPS, ConfigDescOverride = "Chance, per Thqwib, to activate On-Kill effects when exploding.\nDefault Average: 30x * 10% = 3 average On-Kill activations per bloom. Fun (OP) with Soulbound Catalyst.")]
        [FormatToken(token, 2)]
        public static float onKillProcChance = 0f;

        public override NullableRef<GameObject> ItemDisplayPrefab => null;
        public override EquipmentDef EquipmentDef => _equipmentDef;
        private EquipmentDef _equipmentDef;

        private GameObject _thqwibProjectile;
        private ProjectileChanceForOnKillOnDestroy _prefabComponentChanceForOnKillOnDestroy;


        public override bool Execute(EquipmentSlot slot)
        {
            _prefabComponentChanceForOnKillOnDestroy.chance = onKillProcChance;

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
                for (int i = 0; i < numberOfThqwibs; i++)
                {
                    Quaternion rotation = Util.QuaternionSafeLookRotation(Util.ApplySpread(ray2.direction, 0, 25, 1f, 1f));
                    ProjectileManager.instance.FireProjectile(Projectiles.ThqwibProjectile.ThqwibProj, ray2.origin, rotation, slot.gameObject, charBody.damage * damagePerThqwib * damageMult, 0f, Util.CheckRoll(charBody.crit, charBody?.master), DamageColorIndex.Default, null, magnitude);
                }
                return true;
            }
            return false;
        }

        public override void Initialize()
        {
            if(_thqwibProjectile.TryGetComponent<ProjectileGrantOnKillOnDestroy>(out var component))
            {
                GameObject.Destroy(component);
            }

            _thqwibProjectile.AddComponent<ProjectileChanceForOnKillOnDestroy>();
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public override IEnumerator LoadContentAsync()
        {
            /*
             * EquipmentDef - "Thqwib" - Equips
             */

            var addressablesRequest = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Scav/ScavSackProjectile.prefab");

            while (!addressablesRequest.IsDone)
                yield return null;

            _thqwibProjectile = PrefabAPI.InstantiateClone(addressablesRequest.Result, "LIT_Thqwib", true);
        }

        public void ModifyContentPack(ContentPack contentPack)
        {
            contentPack.projectilePrefabs.AddSingle(_thqwibProjectile);
        }

        public override void OnEquipmentLost(CharacterBody body)
        {
        }

        public override void OnEquipmentObtained(CharacterBody body)
        {
        }
    }
}
