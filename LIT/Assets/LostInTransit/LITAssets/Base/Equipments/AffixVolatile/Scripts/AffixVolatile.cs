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
    public sealed class AffixVolatile : LITEliteEquipment, IContentPackModifier
    {
        public override List<EliteDef> EliteDefs => _eliteDefs;
        private List<EliteDef> _eliteDefs;
        public override NullableRef<List<GameObject>> ItemDisplayPrefabs => null;
        public override EquipmentDef EquipmentDef => _equipmentDef;
        private EquipmentDef _equipmentDef;

        private static GameObject _volatileAttachment;
        private static Type _explodingStateType;
        private static Type _postExplosionType;
        private AssetCollection _assetCollection;


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
                if (body && body.HasBuff(LITContent.Buffs.bdAffixVolatile))
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
            var assetRequest = LITAssets.LoadAssetAsync<AssetCollection>("acAffixVolatile", LITBundle.Equips);

            assetRequest.StartLoad();
            while (!assetRequest.IsComplete)
                yield return null;

            _assetCollection = assetRequest.Asset;

            _eliteDefs = new List<EliteDef>
            {
                _assetCollection.FindAsset<ExtendedEliteDef>("Volatile"),
                _assetCollection.FindAsset<ExtendedEliteDef>("VolatileHonor")
            };
            _equipmentDef = _assetCollection.FindAsset<EquipmentDef>("AffixVolatile");
            _volatileAttachment = _assetCollection.FindAsset<GameObject>("VolatileEquipBodyAttachment");
        }

        public override void OnEquipmentLost(CharacterBody body)
        {
        }

        public override void OnEquipmentObtained(CharacterBody body)
        {
        }

        public void ModifyContentPack(ContentPack contentPack)
        {
            contentPack.AddContentFromAssetCollection(_assetCollection);
        }

        public class AffixVolatileSelfDetonateBehaviour : BaseBuffBehaviour, IOnTakeDamageServerReceiver
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

            protected override void OnFirstStackGained()
            {
                base.OnFirstStackGained();
                if (_attachment.attachedBody != CharacterBody)
                {
                    _attachment.AttachToGameObjectAndSpawn(CharacterBody.gameObject);
                }

                if (_attachment.attached)
                {
                    _attachment.gameObject.SetActive(true);
                }
            }

            protected override void OnAllStacksLost()
            {
                base.OnAllStacksLost();
                    if (_attachment.attached)
                        _attachment.gameObject.SetActive(false);
            }

            public void OnTakeDamageServer(DamageReport damageReport)
            {
                if (!_attachment.gameObject.activeSelf)
                    return;

                if (!enabled)
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

            protected override void OnDestroy()
            {
                if (_attachment)
                    Destroy(_attachment.gameObject);
            }
        }
    }
}
