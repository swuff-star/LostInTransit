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
        public float scrapThreshold;
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
            return (dsc.scrap >= scrapThreshold);
        }

        public override bool CanExecute([NotNull] GenericSkill skillSlot)
        {
            return HasScrap(skillSlot) && CanExecute(skillSlot);
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
