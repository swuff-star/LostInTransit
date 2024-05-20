using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LostInTransit.Components
{
    public class EquipmentTakenOrbEffect : MonoBehaviour
    {
        public TrailRenderer trailToColor;
        public ParticleSystem[] particlesToColor = Array.Empty<ParticleSystem>();
        public SpriteRenderer[] spritesToColor = Array.Empty<SpriteRenderer>();
        public SpriteRenderer iconSpriteRenderer;

        private void Start()
        {
            EquipmentDef equipmentDef = EquipmentCatalog.GetEquipmentDef((EquipmentIndex)Util.UintToIntMinusOne(GetComponent<EffectComponent>().effectData.genericUInt));
            ColorCatalog.ColorIndex colorIndex = ColorCatalog.ColorIndex.Error;
            Sprite sprite = null;
            if (equipmentDef != null)
            {
                colorIndex = equipmentDef.colorIndex;
                sprite = equipmentDef.pickupIconSprite;
            }
            Color color = ColorCatalog.GetColor(colorIndex);
            trailToColor.startColor *= color;
            trailToColor.endColor *= color;
            for (int i = 0; i < particlesToColor.Length; i++)
            {
                ParticleSystem obj = particlesToColor[i];
                ParticleSystem.MainModule main = obj.main;
                main.startColor = color;
                obj.Play();
            }
            for (int j = 0; j < spritesToColor.Length; j++)
            {
                spritesToColor[j].color = color;
            }
            iconSpriteRenderer.sprite = sprite;
        }
    }
}