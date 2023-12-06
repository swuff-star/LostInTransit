using RoR2;
using RoR2.Orbs;
using UnityEngine;

namespace LostInTransit.Orbs
{
    public class ToxinOrb : Orb
    {
        private GameObject orbEffect = LITAssets.LoadAsset<GameObject>("ToxinOrbEffect", LITBundle.Items);
        private const float speed = 50f;

        private CharacterBody targetBody;
        public override void Begin()
        {
            duration = distanceToTarget / speed;
            EffectData effectData = new EffectData
            {
                origin = origin,
                genericFloat = duration
            };
            effectData.SetHurtBoxReference(target);

            EffectManager.SpawnEffect(orbEffect, effectData, true);

            HurtBox hurtBox = target.GetComponent<HurtBox>();
            if (hurtBox)
                targetBody = hurtBox.healthComponent.body;
        }

        public override void OnArrival()
        {
            if (targetBody)
                targetBody.AddTimedBuff(LITContent.Buffs.bdToxin, Items.TheToxin.toxinDur);
        }
    }
}
