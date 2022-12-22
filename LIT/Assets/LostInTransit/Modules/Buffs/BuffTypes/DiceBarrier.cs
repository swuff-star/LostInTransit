using Moonstorm;
using RoR2;
using UnityEngine.Networking;
using RoR2.Items;
using Moonstorm.Components;

namespace LostInTransit.Buffs
{
    //[DisabledContent]
    /*public class DiceBarrier : BuffBase
    {
        //public override BuffDef BuffDef { get; } = LITAssets.LoadAsset<BuffDef>("DiceBarrier");

        //Todo: have this use IStatItemBehavior, body.barrierDecayRate is recalculated in recalcstats
        public class DiceBarrierBehavior : BaseBuffBodyBehavior
        {
            //[BuffDefAssociation(useOnClient = true, useOnServer = true)]
            //public static BuffDef GetBuffDef() => LITContent.Buffs.DiceBarrier;

            private float origBarrierDecay;
            public void Start()
            {
                origBarrierDecay = body.barrierDecayRate;
                body.barrierDecayRate *= HGMath.Clamp((Items.BlessedDice.decayMult / 100), 0, 1);

                if(NetworkServer.active)
                    body.healthComponent.AddBarrier(body.maxBarrier * (Items.BlessedDice.barrierAmount / 100));
            }
            public void OnDestroy()
            {
                body.barrierDecayRate = origBarrierDecay;
            }
        }
    }*/
}