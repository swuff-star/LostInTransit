using Moonstorm;
using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using static R2API.DamageAPI;

namespace LostInTransit.DamageTypes
{
    public sealed class ScrapOnHit10 : DamageTypeBase
    {
        public override ModdedDamageType ModdedDamageType { get;  protected set; }

        public static ModdedDamageType scrapOnHit10;

        public static GameObject ScrapPickup = LITAssets.LoadAsset<GameObject>("ScrapPickup", LITBundle.Characters);
        public static float procChance = 10f;

        public override void Initialize()
        {
            base.Initialize();
            scrapOnHit10 = ModdedDamageType;
        }

        public override void Delegates()
        {
            GlobalEventManager.onServerDamageDealt += RollScrap;
        }

        private void RollScrap(DamageReport report)
        {
            var victimBody = report.victimBody;
            var attackerBody = report.attackerBody;
            var damageInfo = report.damageInfo;
            if (DamageAPI.HasModdedDamageType(damageInfo, ModdedDamageType))
            {
                if (Util.CheckRoll(procChance))
                {
                    GameObject scrap = Object.Instantiate(ScrapPickup, victimBody.transform.position, victimBody.transform.rotation);
                    scrap.GetComponent<TeamFilter>().teamIndex = report.attackerTeamIndex;

                    NetworkServer.Spawn(scrap);
                }
            }
        }
    }
}
