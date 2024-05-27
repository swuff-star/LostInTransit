using BepInEx;
using BepInEx.Configuration;
using LostInTransit.Components;
using MSU;
using R2API;
using RoR2;
using RoR2.ContentManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace LostInTransit.Interactables
{
    public class EquipmentStorageChest : LITInteractable, IContentPackModifier
    {
        public override GameObject InteractablePrefab => _prefab;
        private GameObject _prefab;
        public override NullableRef<InteractableCardProvider> CardProvider => null;

        private static ConfigEntry<string> _equipmentStorageChest;
        public static GameObject EquipmentTakenOrbPrefab { get; private set; }
        public override void Initialize()
        {
            _equipmentStorageChest = LITConfig.ConfigMain.Bind<string>("Main", "Random Characters", string.Empty, "A set of random characters. This config gets overwritten by the mod at runtime.");
            
            On.RoR2.ChestBehavior.ItemDrop += ReplaceBarrelDrop;
            Stage.onStageStartGlobal += PlaceChest;
        }

        private void PlaceChest(Stage obj)
        {
            if (!NetworkServer.active)
                return;

            if (DirectorAPI.GetStageEnumFromSceneDef(obj.sceneDef) == DirectorAPI.Stage.MomentFractured)
            {
                Vector3 pos = new Vector3
                {
                    x = 389,
                    y = -169.3f,
                    z = 213
                };
                Quaternion rotation = Quaternion.Euler(0, 286, 359);
                NetworkServer.Spawn(GameObject.Instantiate(_prefab, pos, rotation));
            }
        }

        private void ReplaceBarrelDrop(On.RoR2.ChestBehavior.orig_ItemDrop orig, ChestBehavior self)
        {
            if (!NetworkServer.active)
            {
                orig(self);
                return;
            }

            if (!LITRunBehaviour.Instance)
            {
                orig(self);
                return;
            }

            LITRunBehaviour instance = LITRunBehaviour.Instance;
            if(instance.HasAnyEquipmentBarrelBeenOpened && !instance.HasEquipmentBeenStoredThisRun && instance.IsGameObjectTheFirstEquipmentBarrelBeingOpen(self.gameObject))
            {
                orig(self);
                return;
            }

            var index = GetEquipmentStored();
            if (index == EquipmentIndex.None)
            {
                orig(self);
                return;
            }

            if (!self.TryGetComponent<PurchaseInteraction>(out var purchaseInteraction))
            {
                orig(self);
                return;
            }

            //N: Bad ending
            if(purchaseInteraction.displayNameToken == "EQUIPMENTBARREL_NAME")
            {
                self.dropPickup = PickupCatalog.FindPickupIndex(index);
                _equipmentStorageChest.SetSerializedValue(string.Empty);
            }
            orig(self);
        }

        internal static void SetEquipmentStored(EquipmentIndex index)
        {
            if (index == EquipmentIndex.None)
            {
                _equipmentStorageChest.SetSerializedValue(string.Empty);
                return;
            }

            EquipmentDef def = EquipmentCatalog.GetEquipmentDef(index);

            if (!def)
            {
                _equipmentStorageChest.SetSerializedValue(string.Empty);
                return;
            }

            string name = def.name;
            _equipmentStorageChest.SetSerializedValue(Encrypt(name));

            string Encrypt(string input)
            {
                byte[] key = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
                byte[] iv = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
                SymmetricAlgorithm algorithm = DES.Create();
                ICryptoTransform cryptoTransform = algorithm.CreateEncryptor(key, iv);
                byte[] inputBuffer = Encoding.Unicode.GetBytes(input);
                byte[] outputBuffer = cryptoTransform.TransformFinalBlock(inputBuffer, 0, inputBuffer.Length);
                return Convert.ToBase64String(outputBuffer);
            }
        }

        internal static EquipmentIndex GetEquipmentStored()
        {
            if(!TryDecrypt(out string decryptedEquipmentName))
            {
                return EquipmentIndex.None;
            }
            if(decryptedEquipmentName.IsNullOrWhiteSpace())
            {
                return EquipmentIndex.None;
            }

            var index = EquipmentCatalog.FindEquipmentIndex(decryptedEquipmentName);
            return index;

            bool TryDecrypt(out string result)
            {
                result = string.Empty;

                try
                {
                    byte[] key = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
                    byte[] iv = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
                    SymmetricAlgorithm algorithm = DES.Create();
                    ICryptoTransform cryptoTransform = algorithm.CreateDecryptor(key, iv);
                    byte[] inputBuffer = Convert.FromBase64String(_equipmentStorageChest.Value);
                    byte[] outputBuffer = cryptoTransform.TransformFinalBlock(inputBuffer, 0, inputBuffer.Length);

                    result =  Encoding.Unicode.GetString(outputBuffer);
                    return true;
                }
                catch(Exception _)
                {
                    //gulp
                }
                return false;
            }
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public override IEnumerator LoadContentAsync()
        {
            var request = LITAssets.LoadAssetAsync<AssetCollection>("acEquipmentStorageChest", LITBundle.Characters);

            request.StartLoad();
            while (!request.IsComplete)
                yield return null;

            var collection = request.Asset;

            _prefab = collection.FindAsset<GameObject>("EquipmentStorageChest");
            EquipmentTakenOrbPrefab = collection.FindAsset<GameObject>("EquipmentTakenOrbEffect");

            yield break;
        }

        public void ModifyContentPack(ContentPack contentPack)
        {
            contentPack.effectDefs.AddSingle(new EffectDef(EquipmentTakenOrbPrefab));
        }
    }
}