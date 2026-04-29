using DG.Tweening;
using Lean.Pool;
using UnityEngine;

public class EntitySpawnModule : EntityModule
{
    [SerializeField] private ParticleSystem m_spawnVFXPrefab;
    [SerializeField] private string m_animationTrigger = "Spawn";
    [SerializeField] private float m_preSpawnDelay = 0.5f;
    [SerializeField] private float m_vfxShrinkDuration = 0.3f;

    private ParticleSystem m_spawnVFXInstance;

    public ParticleSystem SpawnVFXPrefab => m_spawnVFXPrefab;
    public float PreSpawnDelay => m_preSpawnDelay;

    public override void OnAllModuleInitialized()
    {
        if (Owner.TryGetModule(out EntityHealthModule healthModule))
            healthModule.OnDeathStart += OnDeathStart;

        Owner.SetSpawning(true);
        Owner.Animator.SetTrigger(m_animationTrigger);
    }

    public void SetSpawnVFXInstance(ParticleSystem instance)
    {
        m_spawnVFXInstance = instance;
    }

    public void HandleSpawnEnd()
    {
        if (Owner.TryGetModule(out EntityHealthModule healthModule))
            healthModule.OnDeathStart -= OnDeathStart;

        Owner.SetSpawning(false);
        ShrinkAndDespawnVFX();
    }

    private void OnDeathStart()
    {
        Owner.SetSpawning(false);
        ShrinkAndDespawnVFX();
    }

    private void ShrinkAndDespawnVFX()
    {
        if (m_spawnVFXInstance == null) return;

        m_spawnVFXInstance.transform
            .DOScale(Vector3.zero, m_vfxShrinkDuration)
            .OnComplete(() =>
            {
                LeanPool.Despawn(m_spawnVFXInstance);
                m_spawnVFXInstance = null;
            });
    }

    public override void Cleanup()
    {
        if (Owner.TryGetModule(out EntityHealthModule healthModule))
            healthModule.OnDeathStart -= OnDeathStart;

        Owner.SetSpawning(false);

        if (m_spawnVFXInstance != null)
        {
            m_spawnVFXInstance.transform.DOKill();
            LeanPool.Despawn(m_spawnVFXInstance);
            m_spawnVFXInstance = null;
        }
    }
}
