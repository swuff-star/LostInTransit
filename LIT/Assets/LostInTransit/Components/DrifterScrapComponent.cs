using RoR2.HudOverlay;
using RoR2.UI;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Networking;

namespace LostInTransit.Components
{
    public class DrifterScrapComponent : NetworkBehaviour
    {
        [Header("Scrap Values")]
        public float minScrap;
        public float maxScrap;

        [Header("Scrap UI")]
        [SerializeField]
        public GameObject overlayPrefab;

        [SerializeField]
        public string overlayChildLocatorEntry;
        private ChildLocator overlayInstanceChildlocator;
        private OverlayController overlayController;
        private List<ImageFillController> fillUiList = new List<ImageFillController>();
        private TextAlignment uiScrapText;

        [SyncVar(hook = "OnScrapModified")]
        private float _scrap;

        public float scrap
        {
            get
            {
                return _scrap;
            }
        }

        public float scrapFraction
        {
            get
            {
                return scrap / maxScrap;
            }
        }

        public float scrapPercentage
        {
            get
            {
                return scrapFraction * 100f;
            }
        }

        public bool isFullScrap
        {
            get
            {
                return scrap >= maxScrap;
            }
        }

        public float Network_scrap
        {
            get
            {
                return _scrap;
            }
            [param: In]
            set
            {
                if (NetworkServer.localClientActive && syncVarHookGuard)
                {
                    syncVarHookGuard = true;
                    OnScrapModified(value);
                    syncVarHookGuard = false;
                }
                SetSyncVar<float>(value, ref _scrap, 1U);
            }
        }

        private void OnScrapModified(float newScrap)
        {
            Network_scrap = newScrap;
        }

        [Server]
        public void AddScrap(float amount)
        {
            if (!NetworkServer.active)
            {
                Debug.LogWarning("calling network addscrap on client");
                return;
            }
            Network_scrap = Mathf.Clamp(scrap + amount, minScrap, maxScrap);
        }

        private void OnEnable()
        {
            OverlayCreationParams overlayCreationParams = new OverlayCreationParams
            {
                prefab = overlayPrefab,
                childLocatorEntry = overlayChildLocatorEntry
            };
            overlayController = HudOverlayManager.AddOverlay(gameObject, overlayCreationParams);
            overlayController.onInstanceAdded += OnOverlayInstanceAdded;
            overlayController.onInstanceRemove += OnOverlayInstanceRemoved;
        }

        private void OnDisable()
        {
            if (overlayController != null)
            {
                overlayController.onInstanceAdded -= OnOverlayInstanceAdded;
                overlayController.onInstanceRemove -= OnOverlayInstanceRemoved;
                fillUiList.Clear();
                HudOverlayManager.RemoveOverlay(overlayController);
            }
        }

        private void OnOverlayInstanceAdded(OverlayController controller, GameObject instance)
        {
            fillUiList.Add(instance.GetComponent<ImageFillController>());
            //uiScrapText = instance.GetComponent<Text>();

            overlayInstanceChildlocator = instance.GetComponent<ChildLocator>();
        }

        private void OnOverlayInstanceRemoved(OverlayController controller, GameObject instance)
        {
            fillUiList.Remove(instance.GetComponent<ImageFillController>());
        }

        private void FixedUpdate()
        {
            foreach (ImageFillController imageFillController in fillUiList)
            {
                imageFillController.SetTValue(scrap / maxScrap);
            }
        }

        public override void PreStartClient()
        {
            //base.PreStartClient();
        }

        public override bool OnSerialize(NetworkWriter writer, bool forceAll)
        {
            if (forceAll)
            {
                writer.Write(_scrap);
                return true;
            }
            bool flag = false;
            if ((syncVarDirtyBits & 1U) != 0U)
            {
                if (!flag)
                {
                    writer.WritePackedUInt32(syncVarDirtyBits);
                    flag = true;
                }
                writer.Write(_scrap);
            }
            if (!flag)
            {
                writer.WritePackedUInt32(syncVarDirtyBits);
            }
            return flag;
        }

        public override void OnDeserialize(NetworkReader reader, bool initialState)
        {
            if (initialState)
            {
                _scrap = reader.ReadSingle();
                return;
            }
            int num = (int)reader.ReadPackedUInt32();
            if ((num & 1) != 0)
            {
                OnScrapModified(reader.ReadSingle());
            }
        }
    }
}
