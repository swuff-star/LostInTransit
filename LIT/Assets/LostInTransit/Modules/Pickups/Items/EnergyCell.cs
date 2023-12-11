using Moonstorm;
using RoR2;
using System;
using RoR2.Items;
using R2API;
using UnityEngine;

namespace LostInTransit.Items
{
    //[DisabledContent]
    public class EnergyCell : ItemBase
    {
        private const string token = "LIT_ITEM_ENERGYCELL_DESC";
        public override ItemDef ItemDef { get; } = LITAssets.LoadAsset<ItemDef>("EnergyCell", LITBundle.Items);

        [ConfigurableField(ConfigName = "Maximum Attack Speed per Cell", ConfigDesc = "Maximum amount of attack speed per item held.")]
        //[TokenModifier(token, StatTypes.Percentage)]
        public static float bonusAttackSpeed = 0.4f;


        public class EnergyCellBehavior : BaseItemBodyBehavior, IStatItemBehavior, IOnTakeDamageServerReceiver
        {
            //★. ..will look up and shout "stop doing everything in the FixedUpdate method!"... and I'll look down and whisper "no".
            //★ Jokes aside, this makes sense to do inside FixedUpdate, right? I figure doing it in RecalculateStats wouldn't update properly, since... well, it's only when RecalculateStats is called.
            //★ P.S. What do you call "FixedUpdate()"? Like, the name for it? It's a 'method', right? I am adding things inside of the method?

            //1.- Yeah, i think this should be called on fixed update. the other option is to look at what watch metronome does for keeping the speed boost constant.
            //2.- FixedUpdate is a method that gets called automatically by unity, remember that CharacterBody.ItemBehavior inherits from MonoBehavior, and all classes that inherit from MonoBehavior have access to FixedUpdate(), Update() among other methods.
            [ItemDefAssociation(useOnClient = true, useOnServer = true)]
            public static ItemDef GetItemDef() => LITContent.Items.EnergyCell;

            public float healthFraction;
            private HealthComponent _healthComponent;

            private void Start()
            {
                _healthComponent = body.healthComponent;
            }

            public void OnTakeDamageServer(DamageReport _)
            {
                body.MarkAllStatsDirty();
            }

            public void RecalculateStatsEnd()
            {
                body.attackSpeed += body.attackSpeed * (1 - healthFraction) * (float)(Math.Pow(bonusAttackSpeed, 1 / stack));
            }

            public void RecalculateStatsStart()
            {
            }

            private void FixedUpdate()
            {
                float combinedHealthFraction = _healthComponent.combinedHealthFraction;
                healthFraction = combinedHealthFraction - 0.1f;
                if (combinedHealthFraction > 0.9f)
                {
                    healthFraction = 1;
                }
                else if (combinedHealthFraction < 0f)
                {
                    healthFraction = 0;
                }
                //★ Is there a better way to do this? From what I understand, Math.Floor() and Math.Ceil() are used to round numbers, rather than prevent them from exiting a specific range.
            }
        }
    }
}
