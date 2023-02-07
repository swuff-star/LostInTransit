using System.Collections.Generic;
using System;
using UnityEngine;

/// <summary>
/// Creator : Luke Cho (Novike)
/// Date : 15-11-2022
/// 
/// The world most simple motion trail component. 
/// Just attach this component to a GameObject with a SkinnedMeshRenderer on it. 
/// That's it. 
/// Would you like to toggle it ? Just call the On() & Off() methods.
/// </summary>
[RequireComponent(typeof(SkinnedMeshRenderer))]
public class MotionTrailGenerator : MonoBehaviour
{
    private SkinnedMeshRenderer _skinedMeshRenderer;
    [RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
    public class DummyMesh : MonoBehaviour
    {
        public MotionTrailGenerator Generator;
        public MeshRenderer Renderer;
        public MeshFilter Filter;
        public float LifeTime;
        private float _timer;

        public void On()
        {
            transform.localPosition = Vector3.zero; 
            transform.localRotation = Quaternion.identity;
            _timer = LifeTime;
            transform.parent = null;
            gameObject.SetActive(true);
        }
        public void Off()
        {            
            transform.parent = Generator.transform;
            Generator.Return(this);
            gameObject.SetActive(false);
        }

        private void Awake()
        {
            Renderer = GetComponent<MeshRenderer>();
            Filter = GetComponent<MeshFilter>();
        }

        private void Update()
        {
            if (_timer < 0)
                Off();
            else
                _timer -= Time.deltaTime;
        }
    }
    private Queue<DummyMesh> _queue = new Queue<DummyMesh>();
    [SerializeField] private Material _material;
    [Range(1.0f, 30.0f)]
    [SerializeField] private float _rateOverTime;
    [Range(0.01f, 10f)]
    [SerializeField] private float _lifeTime;
    [Range(1, 100)]
    [SerializeField] private int _capacity;
    private bool _on;
    private float _timer;
    private DummyMesh _tmp;


    //=======================================================================================
    //********************************** Public Methods *************************************
    //=======================================================================================

    public void On() => _on = true;

    public void Off() => _on = false;

    public void Return(DummyMesh child)
    {
        _queue.Enqueue(child);
    }


    //=======================================================================================
    //********************************** Private Methods ************************************
    //=======================================================================================

    private void Spawn()
    {
        _tmp = _queue.Dequeue();
        _skinedMeshRenderer.BakeMesh(_tmp.Filter.mesh);
        _tmp.On();
    }

    private void Awake()
    {
        _skinedMeshRenderer = GetComponent<SkinnedMeshRenderer>();

        for (int i = 0; i < _capacity; i++)
        {
            _tmp = new GameObject("MotionTrailDummyMesh").AddComponent<DummyMesh>();
            _tmp.Generator = this;
            _tmp.LifeTime = _lifeTime;
            _tmp.Renderer.sharedMaterial = _material;
            _tmp.Off();
        }
    }

    private void Start()
    {
        On();
    }

    private void Update()
    {
        if (_on)
        {
            if (_timer < 0)
            {
                if (_queue.Count > 0)
                {
                    Spawn();
                    _timer = 1.0f  / _rateOverTime;
                }
            }
            else
            {
                _timer -= Time.deltaTime;
            }
        }        
    }
}
