using MSU;
using RoR2;
using RoR2.ContentManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using R2API;
using MonoMod.Cil;
using Mono.Cecil.Cil;

namespace LostInTransit.Equipments
{
    public sealed class AffixVolatile : LITEliteEquipment
    {
        public override List<EliteDef> EliteDefs => _eliteDefs;
        private List<EliteDef> _eliteDefs;
        public override NullableRef<GameObject> ItemDisplayPrefab => null;
        public override EquipmentDef EquipmentDef => _equipmentDef;
        private EquipmentDef _equipmentDef;

        private static GameObject _volatileAttachment;
        private BuffDef _buffDef;
        private static Type _explodingStateType;
        private static Type _postExplosionType;

        //ProcTypeAPI when... -N
        public static DamageAPI.ModdedDamageType VolatileExplosionDamageType { get; private set; }

        public override bool Execute(EquipmentSlot slot)
        {
            if (slot.TryGetComponent<AffixVolatileSelfDetonateBehaviour>(out var component))
            {
                return component.TryExplode();
            }
            return false;
        }

        public override void Initialize()
        {
            VolatileExplosionDamageType = DamageAPI.ReserveDamageType();
            IL.RoR2.GlobalEventManager.OnHitAll += VolatileExplosion;
            _explodingStateType = typeof(EntityStates.AffixVolatile.SelfDestruct);
            _postExplosionType = typeof(EntityStates.AffixVolatile.PostSelfDestruct);
        }

        private void VolatileExplosion(MonoMod.Cil.ILContext il)
        {
            ILCursor cursor = new ILCursor(il);

            bool reachedDestination = cursor.TryGotoNext(MoveType.After, x => x.MatchLdloc(2),
                x => x.MatchLdsfld(typeof(RoR2Content.Items), nameof(RoR2Content.Items.Behemoth)),
                x => x.MatchCallOrCallvirt<Inventory>(nameof(Inventory.GetItemCount)),
                x => x.MatchStloc(3));

            if (!reachedDestination)
            {
                LITLog.Fatal("Could not reach proper destination for Volatile Explosion!");
                return;
            }

            cursor.Emit(OpCodes.Ldloc_0);
            cursor.Emit(OpCodes.Ldloc_3);
            cursor.EmitDelegate<Func<CharacterBody, int, int>>((body, behemothCount) =>
            {
                if (body && body.HasBuff(_buffDef))
                {
                    return behemothCount + 2;
                }
                return behemothCount;
            });
            cursor.Emit(OpCodes.Stloc_3);
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public override IEnumerator LoadContentAsync()
        {
            /*
             * ExtendedEliteDef - "Volatile" - Equips
             * ExtendedEliteDef - "VolatileHonor" - Equips
             * EquipmentDef - "AffixVolatile" - Equips
             * BuffDef - "bdAffixVolatile" - Equips
             * GameObject - "VolatileEquipBodyAttachment" - Equips
             */
            yield break;
        }

        public override void OnEquipmentLost(CharacterBody body)
        {
        }

        public override void OnEquipmentObtained(CharacterBody body)
        {
        }

        public class AffixVolatileSelfDetonateBehaviour : BuffBehaviour, IOnTakeDamageServerReceiver
        {
            [BuffDefAssociation]
            public static BuffDef GetBuffDef() => LITContent.Buffs.bdAffixVolatile;

            private NetworkedBodyAttachment _attachment;
            private EntityStateMachine _explosionStateMachine;

            private void Awake()
            {
                _attachment = Instantiate(_volatileAttachment).GetComponent<NetworkedBodyAttachment>();
                _explosionStateMachine = _attachment.GetComponent<EntityStateMachine>();
                _attachment.gameObject.SetActive(false);
            }

            private void OnEnable()
            {
                if(_attachment.attachedBody != CharacterBody)
                {
                    _attachment.AttachToGameObjectAndSpawn(CharacterBody.gameObject);
                }

                if(_attachment.attached)
                {
                    _attachment.gameObject.SetActive(true);
                }
            }

            private void OnDisable()
            {
                if (_attachment.attached)
                    _attachment.gameObject.SetActive(false);
            }

            public void OnTakeDamageServer(DamageReport damageReport)
            {
                if (!_attachment.gameObject.activeSelf)
                    return;

                var healthComponent = CharacterBody.healthComponent;
                //If we're dead, destroy the attachment to avoid detonation post mortem
                if (!healthComponent.alive && _attachment)
                    Destroy(_attachment.gameObject);

                //This makes any AI body self detonate if they have the buff
                if (healthComponent && healthComponent.isHealthLow && !CharacterBody.isPlayerControlled)
                {
                    TryExplode();
                }
            }

            public bool TryExplode()
            {
                if (!_attachment.gameObject.activeSelf)
                    return false;

                if (IsExploding())
                    return false;

                _explosionStateMachine.SetNextState(new EntityStates.AffixVolatile.SelfDestruct());
                return true;
            }

            public bool IsExploding()
            {
                Type type = _explosionStateMachine.state.GetType();
                return type == _explodingStateType || type == _postExplosionType;
            }

            private void OnDestroy()
            {
                if (_attachment)
                    Destroy(_attachment.gameObject);
            }
        }
    }
}
