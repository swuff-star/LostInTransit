using LostInTransit.Buffs;
using LostInTransit.Elites;
using RoR2;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine.Networking;
using UnityEngine;

namespace LostInTransit.Components
{
    //Stores the available elite defs and networks which elites the blighted elite is using.
    [RequireComponent(typeof(NetworkedBodyAttachment))]
    public class BlightedBodyAttachment : NetworkBehaviour, INetworkedBodyAttachmentListener
    {
        public EliteIndex FirstIndex => (EliteIndex)_firstEliteIndex;
        public EliteIndex SecondIndex => (EliteIndex)_secondEliteIndex;
        [SyncVar]
        private int _firstEliteIndex;
        [SyncVar]
        private int _secondEliteIndex;

        private ReadOnlyCollection<EliteDef> _availableEliteDefs;
        private Xoroshiro128Plus _rng;
        private CharacterBody attachedBody;

        private void Awake()
        {
            if (!Run.instance)
                return;

            _rng = new Xoroshiro128Plus(Run.instance.runRNG.nextUlong);
            if (RunArtifactManager.instance)
            {
                _availableEliteDefs = RunArtifactManager.instance.IsArtifactEnabled(RoR2Content.Artifacts.eliteOnlyArtifactDef) ? BlightedElites.ElitesHonorEnabled : BlightedElites.ElitesHonorDisabled;
            }
            else
            {
                _availableEliteDefs = BlightedElites.ElitesHonorDisabled;
            }
        }


        private void OnEnable()
        {
            if(attachedBody)
            {
                DoOnEnable();
            }
        }

        public void OnAttachedBodyDiscovered(NetworkedBodyAttachment networkedBodyAttachment, CharacterBody attachedBody)
        {
            DoOnEnable();
        }

        private void DoOnEnable()
        {
            if (NetworkServer.active)
                RandomizeElites();
        }


        [Server]
        public void RandomizeElites()
        {
            var firstArrayIndex = _rng.RangeInt(0, _availableEliteDefs.Count);
            var first = _availableEliteDefs[firstArrayIndex];

            var secondArrayIndex = firstArrayIndex;
            while(secondArrayIndex == firstArrayIndex)
            {
                secondArrayIndex = _rng.RangeInt(0, _availableEliteDefs.Count);
            }
            var second = _availableEliteDefs[secondArrayIndex];

            _firstEliteIndex = (int)first.eliteIndex;
            _secondEliteIndex = (int)second.eliteIndex;
        }
    }
}
