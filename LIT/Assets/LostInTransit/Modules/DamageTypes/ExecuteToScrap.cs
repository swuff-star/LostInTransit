using LostInTransit.Components;
using Moonstorm;
using R2API;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using static R2API.DamageAPI;

namespace LostInTransit.DamageTypes
{
    public sealed class ExecuteToScrap : DamageTypeBase
    {
        public override ModdedDamageType ModdedDamageType { get; protected set; }

        public static ModdedDamageType executeToScrap;

        public static GameObject ScrapPickup = LITAssets.LoadAsset<GameObject>("ScrapPickup", LITBundle.Characters);
        public static float procChance = 20f;

        public override void Initialize()
        {
            base.Initialize();
            executeToScrap = ModdedDamageType;
        }

        public override void Delegates()
        {
            GlobalEventManager.onServerDamageDealt += CheckExecute;
        }

        private void CheckExecute(DamageReport report)
        {
            var victimBody = report.victimBody;
            var attackerBody = report.attackerBody;
            var damageInfo = report.damageInfo;
            if (DamageAPI.HasModdedDamageType(damageInfo, ModdedDamageType))
            {
                if (victimBody.healthComponent.combinedHealth < victimBody.healthComponent.fullCombinedHealth * 0.2f)
                {
                    victimBody.healthComponent.Suicide();
                    DrifterScrapComponent dsc = attackerBody.GetComponent<DrifterScrapComponent>();
                    if (dsc != null)
                    {
                        dsc.AddScrap(3f);
                    }
                }
                else if (Util.CheckRoll(procChance))
                {
                    GameObject scrap = Object.Instantiate(ScrapPickup, victimBody.transform.position, victimBody.transform.rotation);
                    scrap.GetComponent<TeamFilter>().teamIndex = report.attackerTeamIndex;

                    NetworkServer.Spawn(scrap);
                }
            }
        }
    }
}
