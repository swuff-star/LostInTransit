using JetBrains.Annotations;
using LostInTransit.Components;
using RoR2;
using RoR2.Skills;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LostInTransit.ScriptableObjects
{
    [CreateAssetMenu(menuName = "LostInTransit/ScrapSkillDef/ScrapSkillDef")]
    public class ScrapSkillDef : SkillDef
    {
        [Header("Scrap Parameters")]
        [Tooltip("The required amount of scrap to perform this skill.")]
        public float scrapCost;
        protected class InstanceData : BaseSkillInstanceData
        {
            public DrifterScrapComponent dsc;
        }

        public override bool IsReady([NotNull] GenericSkill skillSlot)
        {
            return base.IsReady(skillSlot) && HasScrap(skillSlot);
        }

        public bool HasScrap([NotNull] GenericSkill skillSlot)
        {
            DrifterScrapComponent dsc = ((InstanceData)skillSlot.skillInstanceData).dsc;
            if (dsc == null)
                return false;
            return (dsc.scrap >= scrapCost);
        }

        public override bool CanExecute([NotNull] GenericSkill skillSlot)
        {
            return HasScrap(skillSlot) && base.CanExecute(skillSlot);
        }

        public override BaseSkillInstanceData OnAssigned([NotNull] GenericSkill skillSlot)
        {
            return new InstanceData
            {
                dsc = skillSlot.GetComponent<DrifterScrapComponent>()
            };
        }
    }
}
