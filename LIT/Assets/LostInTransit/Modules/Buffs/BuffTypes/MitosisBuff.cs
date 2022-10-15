using Moonstorm;
using Moonstorm.Components;
using R2API;
using RoR2;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LostInTransit.Buffs
{
    public class MitosisBuff : BuffBase
    {
        public override BuffDef BuffDef => LITAssets.LoadAsset<BuffDef>("MitosisBuff");

        public class MitosisBuffBehavior : BaseBuffBodyBehavior, IBodyStatArgModifier, IStatItemBehavior
        {
            [BuffDefAssociation]
            private static BuffDef GetBuffDef() => LITContent.Buffs.MitosisBuff;

            public void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args)
            {
                if (body.HasBuff(LITContent.Buffs.MitosisBuff))
                {
                    //args.cooldownReductionAdd += Items.RapidMitosis.mitosisSkillCD;
                }
            }

            public void RecalculateStatsEnd()
            {
                if (body.HasBuff(LITContent.Buffs.MitosisBuff))
                {
                    if (body.skillLocator)
                    {
                        if (body.skillLocator.primary)
                            body.skillLocator.primary.cooldownScale *= 1 - Items.RapidMitosis.mitosisSkillCD;
                        if (body.skillLocator.secondary)
                            body.skillLocator.secondary.cooldownScale *= 1 - Items.RapidMitosis.mitosisSkillCD;
                        if (body.skillLocator.utility)
                            body.skillLocator.utility.cooldownScale *= 1 - Items.RapidMitosis.mitosisSkillCD;
                        if (body.skillLocator.special)
                            body.skillLocator.special.cooldownScale *= 1 - Items.RapidMitosis.mitosisSkillCD;
                    }
                }
            }

            public void RecalculateStatsStart()
            {

            }
        }
    }
}
