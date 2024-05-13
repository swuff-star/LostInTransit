using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LostInTransit.Components
{
    public class IncreaseParticleNoiseStrengthOverTime : MonoBehaviour
    {
        public new ParticleSystem particleSystem;
        public AnimationCurve strengthCurve;
        public float maxTime;
        public bool loop;

        private ParticleSystem.NoiseModule _noiseModule;
        private float _maxStrength;
        private float _stopWatch;
        private void Awake()
        {
            if(!particleSystem)
            {
                particleSystem = GetComponent<ParticleSystem>();
            }
        }

        private void Start()
        {
            _noiseModule = particleSystem.noise;
            _maxStrength = _noiseModule.strengthMultiplier;

        }

        private void Update()
        {
            _stopWatch += Time.deltaTime;
            _noiseModule.strengthMultiplier = strengthCurve.Evaluate(_stopWatch / maxTime) * _maxStrength;
            if(_stopWatch >= maxTime && loop)
            {
                _stopWatch = 0;
            }
        }
    }
}