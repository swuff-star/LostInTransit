﻿using KinematicCharacterController;
using Moonstorm;
using RoR2;
using UnityEngine;

namespace LostInTransit.Equipments
{
    public class AffixBlighted : LITEliteEquip
    {
        public override MSEliteDef EliteDef { get; set; } = LITAssets.Instance.MainAssetBundle.LoadAsset<MSEliteDef>("Blighted");
        public override EquipmentDef EquipmentDef { get; set; } = LITAssets.Instance.MainAssetBundle.LoadAsset<EquipmentDef>("AffixBlighted");
        public override MSAspectAbilityDataHolder AspectAbilityData { get; set; } = LITAssets.Instance.MainAssetBundle.LoadAsset<MSAspectAbilityDataHolder>("AbilityBlighted");

        public override void AddBehavior(ref CharacterBody body, int stack)
        {
            body.AddItemBehavior<BlightStatIncrease>(stack);
        }

        public override bool FireAction(EquipmentSlot slot)
        {
            if (MSUtil.IsModInstalled("com.TheMysticSword.AspectAbilities"))
            {
                var component = slot.characterBody.GetComponent<Buffs.AffixBlighted.AffixBlightedBehavior>();
                if (component)
                {
                    component.MasterBehavior.Ability();
                    return true;
                }
            }
            return false;
        }

        public class BlightStatIncrease : CharacterBody.ItemBehavior
        {
            public void Start()
            {
                body.baseMaxHealth *= 8f;
                body.baseDamage *= 2;
                body.baseMoveSpeed *= 1.1f;
                body.baseAttackSpeed *= 1.1f;
                body.baseArmor += 33.34f;
                body.PerformAutoCalculateLevelStats();

                body.healthComponent.health = body.healthComponent.fullHealth;
            }

            public void OnDestroy()
            {
                if(body.healthComponent.alive)
                {
                    body.baseMaxHealth /= 7.0f;
                    body.baseDamage /= 2f;
                    body.baseMoveSpeed /= 1.1f;
                    body.baseAttackSpeed /= 1.1f;
                    body.baseArmor -= 33.34f;
                    body.PerformAutoCalculateLevelStats();
                }
            }
        }
    }
}
