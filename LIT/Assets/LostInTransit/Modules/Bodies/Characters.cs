using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using R2API.ScriptableObjects;
using System.Collections.Generic;
using System.Linq;
using LostInTransit;
using Moonstorm;

namespace LostInTransit.Characters
{
    public sealed class Characters : CharacterModuleBase
    {
        public static Characters Instance { get; set; }
        public override R2APISerializableContentPack SerializableContentPack => LITContent.Instance.SerializableContentPack;

        public override void Initialize()
        {
            Instance = this;
            base.Initialize();
            LITLogger.LogI($"Initializing Bodies.");
            GetCharacterBases();
        }

        protected override IEnumerable<CharacterBase> GetCharacterBases()
        {
            base.GetCharacterBases()
                .ToList()
                .ForEach(character => AddCharacter(character));
            return null;
        }
    }
}
