using KinematicCharacterController;
using Moonstorm;
using RoR2;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace LostInTransit.Equipments
{
    public class AffixBlighted : EliteEquipmentBase
    {
        public override List<MSEliteDef> EliteDefs { get; } = new List<MSEliteDef>
        {
            LITAssets.LoadAsset<MSEliteDef>("Blighted", LITBundle.Equips),
        };

        public override EquipmentDef EquipmentDef { get; } = LITAssets.LoadAsset<EquipmentDef>("AffixBlighted", LITBundle.Equips);

        [ConfigurableField(ConfigName = "Boss Blighted", ConfigDesc = "Whether Teleporter Bosses should spawn as Blighted enemies.")]
        public static bool bossBlighted = false;


        public override void Initialize()
        {
            HG.ArrayUtils.ArrayAppend(ref LITContent.Instance.SerializableContentPack.artifactDefs, LITAssets.LoadAsset<ArtifactDef>("Prestige", LITBundle.Artifacts));
        }

        public override void AddBehavior(ref CharacterBody body, int stack)
        {
            body.AddItemBehavior<BlightStatIncrease>(stack);
        }

        public override bool FireAction(EquipmentSlot slot)
        {
            if(slot.TryGetComponent<Buffs.AffixBlighted.AffixBlightedBehaviour>(out var behaviour))
            {
                behaviour.RandomizeElites();
                return true;
            }
            return false;
        }

        //Stat increase is done via having this equipment, to avoid huge stat changes when using wake of vultures
        //plus, having this kind of power is pog, and if you get the drop then i think you've earned the powertrip -N
        public class BlightStatIncrease : CharacterBody.ItemBehavior
        {
            public void Start()
            {
                body.baseMaxHealth *= 7.0f;
                body.baseDamage *= 1.6f;
                body.baseMoveSpeed *= 1.1f;

                body.PerformAutoCalculateLevelStats();

                body.healthComponent.health = body.healthComponent.fullHealth;

                Util.PlaySound("BlightAppear", body.gameObject);

                CharacterModel cm = body.modelLocator.modelTransform.GetComponent<CharacterModel>();

                MotionTrailGenerator mtg = cm.mainSkinnedMeshRenderer.gameObject.AddComponent<MotionTrailGenerator>();

                mtg.On();
            }


            public void OnDestroy()
            {
                if(body.healthComponent.alive)
                {
                    body.baseMaxHealth /= 7.0f;
                    body.baseDamage /= 1.4f;
                    body.baseMoveSpeed /= 1.1f;
                    body.PerformAutoCalculateLevelStats();
                }
            }
        }
    }
}
