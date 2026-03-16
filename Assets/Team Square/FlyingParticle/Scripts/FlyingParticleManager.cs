using System;
using Lean.Pool;
using MyBox;
using Pinpin;
using UnityEngine;
using UnityEngine.Serialization;

public class FlyingParticleManager : Singleton<FlyingParticleManager>
{
    [SerializeField] private UIFlyingParticle m_flyingParticlePrefab;
    public UIFlyingParticle Spawn(Vector3 startScreenPos, Transform target, Sprite sprite, float duration, Action callback, double burstCount = 1)
    {
        UIFlyingParticle _spawnedParticle = LeanPool.Spawn(m_flyingParticlePrefab, startScreenPos, Quaternion.identity);
        _spawnedParticle.Initialize(target, sprite, duration, callback, burstCount);
        
        return _spawnedParticle;
    }
}