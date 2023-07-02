using KinematicCharacterController;
using Moonstorm;
using RoR2;
using System.Collections.Generic;
using UnityEngine;

namespace LostInTransit.Equipments
{
    [DisabledContent]
    public class AffixBlighted : EliteEquipmentBase
    {
        public override List<MSEliteDef> EliteDefs { get; } = new List<MSEliteDef>
        {
            LITAssets.LoadAsset<MSEliteDef>("Blighted", LITBundle.Equips),
        };
        public override EquipmentDef EquipmentDef { get; } = LITAssets.LoadAsset<EquipmentDef>("AffixBlighted", LITBundle.Equips);
       // public override MSAspectAbilityDataHolder AspectAbilityData { get; } = LITAssets.LoadAsset<MSAspectAbilityDataHolder>("AbilityBlighted");

        [ConfigurableField(ConfigName = "Boss Blighted", ConfigDesc = "Whether Teleporter Bosses should spawn as Blighted enemies.")]
        public static bool bossBlighted = false;

        public override void AddBehavior(ref CharacterBody body, int stack)
        {
            body.AddItemBehavior<BlightStatIncrease>(stack);
        }

        public override bool FireAction(EquipmentSlot slot)
        {
            /*if (MSUtil.IsModInstalled("com.TheMysticSword.AspectAbilities"))
            {
                var component = slot.characterBody.GetComponent<Buffs.AffixBlighted.AffixBlightedBehavior>();
                if (component)
                {
                    component.MasterBehavior.Ability();
                    return true;
                }
            }*/
            return false;
        }

        //N- Due to the nature of us usiong "ItemBehavior" for equipments, Equipment behaviors still inherit from CharacterBody.ItemBehavior
        //Chances of this changing are extremely minimal.
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
