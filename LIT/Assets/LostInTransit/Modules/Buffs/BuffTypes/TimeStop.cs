using Moonstorm;
using RoR2;
using R2API;
using RoR2.Items;
using Moonstorm.Components;
using UnityEngine;

namespace LostInTransit.Buffs
{
    //[DisabledContent]
    public class TimeStop : BuffBase
    {
        public static GameObject buffWard = LITAssets.LoadAsset<GameObject>("TimeStopSphere", LITBundle.Equips);
        public override BuffDef BuffDef { get; } = LITAssets.LoadAsset<BuffDef>("bdTimeStop", LITBundle.Equips);

        public class TimeStopBehavior : BaseBuffBodyBehavior
        {
            [BuffDefAssociation(useOnServer = true, useOnClient = true)]
            public static BuffDef GetBuffDef() => LITContent.Buffs.bdTimeStop;
            private GameObject wardInstance;

            public void Start()
            {
                wardInstance = Instantiate(buffWard);
                wardInstance.GetComponent<TeamFilter>().teamIndex = body.teamComponent.teamIndex;
                wardInstance.GetComponent<NetworkedBodyAttachment>().AttachToGameObjectAndSpawn(gameObject);
            }

            public void OnDestroy()
            {
                if (wardInstance != null)
                    Destroy(wardInstance);
            }
        }
    }
}