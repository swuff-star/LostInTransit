using MSU;
using RoR2;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RoR2.ContentManagement;

namespace LostInTransit
{
    public abstract class LITInteractable : IInteractableContentPiece
    {
        IInteractable IGameObjectContentPiece<IInteractable>.component => interactablePrefab.GetComponent<IInteractable>();
        GameObject IContentPiece<GameObject>.asset => interactablePrefab;
        public abstract GameObject interactablePrefab { get; }
        public abstract NullableRef<InteractableCardProvider> cardProvider { get; }

        public abstract void Initialize();
        public abstract bool IsAvailable(ContentPack contentPack);
        public abstract IEnumerator LoadContentAsync();
    }
}