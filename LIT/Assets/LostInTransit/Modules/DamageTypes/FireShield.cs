using Moonstorm;
using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using static R2API.DamageAPI;

namespace LostInTransit.DamageTypes
{
    public sealed class FireShield : DamageTypeBase
    {
        public override ModdedDamageType ModdedDamageType { get; protected set; }

        public static ModdedDamageType fireShield;

        public override void Initialize()
        {
            base.Initialize();
            fireShield = ModdedDamageType;
        }

        public override void Delegates()
        {
            GlobalEventManager.onServerDamageDealt += Ignite;
        }

        private void Ignite(DamageReport report)
        {
            CharacterBody attackerBody = report.attackerBody;
            DamageInfo damageInfo = report.damageInfo;
            if (DamageAPI.HasModdedDamageType(damageInfo, ModdedDamageType))
            {
                var dotInfo = new InflictDotInfo()
                {
                    attackerObject = attackerBody.gameObject,
                    victimObject = report.victim.gameObject,
                    dotIndex = DotController.DotIndex.Burn,
                    duration = 2f,
                    damageMultiplier = attackerBody.GetItemCount(LITContent.Items.FireShield) * Items.FireShield.burnCoef
                };
                StrengthenBurnUtils.CheckDotForUpgrade(report.attackerBody.inventory, ref dotInfo);
                DotController.InflictDot(ref dotInfo);
            }
        }
    }
}
