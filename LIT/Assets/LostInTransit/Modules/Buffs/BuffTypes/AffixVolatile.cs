using LostInTransit.Elites;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Moonstorm;
using Moonstorm.Components;
using R2API;
using RoR2;
using RoR2.Artifacts;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace LostInTransit.Buffs
{
    //★ I did something naughty and just... commented out a lot of errors instead of fixing them. I plan on redoing this anyway so... oh well.
    //N - haha i've done it, i've fixed this shit :steam_happy:
    public class AffixVolatile : BuffBase
    {
        public override BuffDef BuffDef { get; } = LITAssets.LoadAsset<BuffDef>("bdAffixVolatile", LITBundle.Equips);
        private static Type _explodingStateType;
        private static Type _postExplosionStateType;

        public override void Initialize()
        {
            IL.RoR2.GlobalEventManager.OnHitAll += VolatileExplosion;
            _explodingStateType = typeof(EntityStates.AffixVolatile.SelfDestruct);
            _postExplosionStateType = typeof(EntityStates.AffixVolatile.PostSelfDestruct);
        }

        private void VolatileExplosion(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);

            bool reachedDestination = cursor.TryGotoNext(MoveType.After, x => x.MatchLdloc(2),
                x => x.MatchLdsfld(typeof(RoR2Content.Items), nameof(RoR2Content.Items.Behemoth)),
                x => x.MatchCallOrCallvirt<Inventory>(nameof(Inventory.GetItemCount)),
                x => x.MatchStloc(3));

            if(!reachedDestination)
            {
                LITLog.Error("Could not reach proper destination for Volatile Explosion.");
                return;
            }

            cursor.Emit(OpCodes.Ldloc_0);
            cursor.Emit(OpCodes.Ldloc_3);
            cursor.EmitDelegate<Func<CharacterBody, int, int>>((body, behemothCount) =>
            {
                if (body && body.HasBuff(BuffDef))
                {
                    return behemothCount + 1;
                }
                return behemothCount;
            });
            cursor.Emit(OpCodes.Stloc_3);
        }

        public class AffixVolatileSelfDetonateBehaviour : BaseBuffBodyBehavior, IOnTakeDamageServerReceiver
        {
            [BuffDefAssociation]
            public static BuffDef GetBuffDef() => LITContent.Buffs.bdAffixVolatile;
            public static GameObject volatileAttachment = LITAssets.LoadAsset<GameObject>("VolatileEquipBodyAttachment", LITBundle.Equips);
            private NetworkedBodyAttachment _attachment;
            private EntityStateMachine _explosionStateMachine;

            private void Start()
            {
                _attachment = Instantiate(volatileAttachment).GetComponent<NetworkedBodyAttachment>();
                _attachment.AttachToGameObjectAndSpawn(body.gameObject);
                _explosionStateMachine = _attachment.GetComponent<EntityStateMachine>();
            }

            public void OnTakeDamageServer(DamageReport damageReport)
            {
                //This makes any AI body self detonate if they have the buff
                var healthComponent = body.healthComponent;
                if(healthComponent && healthComponent.isHealthLow && !body.isPlayerControlled)
                {
                    TryExplode();
                }
            }

            public bool TryExplode()
            {
                if (IsExploding())
                    return false;

                _explosionStateMachine.SetNextState(new EntityStates.AffixVolatile.SelfDestruct());
                return true;
            }

            public bool IsExploding()
            {
                Type type = _explosionStateMachine.state.GetType();
                return type == _explodingStateType || type == _postExplosionStateType;
            }

            private void OnDestroy()
            {
                if (_attachment)
                    Destroy(_attachment.gameObject);
            }
        }
    }
}
