using System.Collections;
using DG.Tweening;
using Lean.Pool;
using UnityEngine;

public class EntitySpawnModule : EntityModule
{
    [SerializeField] private ParticleSystem m_spawnVFXPrefab;
    [SerializeField] private string m_animationTrigger = "Spawn";
    [SerializeField] private float m_preSpawnDelay = 0.5f;
    [SerializeField] private float m_spawnDuration = 1f;
    [SerializeField] private float m_vfxShrinkDuration = 0.3f;

    private ParticleSystem m_spawnVFXInstance;
    private Coroutine m_spawnCR;

    public ParticleSystem SpawnVFXPrefab => m_spawnVFXPrefab;
    public float PreSpawnDelay => m_preSpawnDelay;

    public override void OnAllModuleInitialized()
    {
        if (Owner.TryGetModule(out EntityHealthModule healthModule))
            healthModule.OnDeath += OnDeathStart;

        Owner.SetSpawning(true);
        Owner.Animator.SetTrigger(m_animationTrigger);
        m_spawnCR = Owner.StartCoroutine(SpawnTimerCR());
    }

    private IEnumerator SpawnTimerCR()
    {
        yield return new WaitForSeconds(m_spawnDuration);
        m_spawnCR = null;
        HandleSpawnEnd();
    }

    public void SetSpawnVFXInstance(ParticleSystem instance)
    {
        m_spawnVFXInstance = instance;
    }

    public void HandleSpawnEnd()
    {
        if (m_spawnCR != null)
        {
            Owner.StopCoroutine(m_spawnCR);
            m_spawnCR = null;
        }

        if (Owner.TryGetModule(out EntityHealthModule healthModule))
            healthModule.OnDeath -= OnDeathStart;

        Owner.SetSpawning(false);
        ShrinkAndDespawnVFX();
    }

    private void OnDeathStart()
    {
        if (m_spawnCR != null)
        {
            Owner.StopCoroutine(m_spawnCR);
            m_spawnCR = null;
        }

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
        if (m_spawnCR != null)
        {
            Owner.StopCoroutine(m_spawnCR);
            m_spawnCR = null;
        }

        if (Owner.TryGetModule(out EntityHealthModule healthModule))
            healthModule.OnDeath -= OnDeathStart;

        Owner.SetSpawning(false);

        if (m_spawnVFXInstance != null)
        {
            m_spawnVFXInstance.transform.DOKill();
            LeanPool.Despawn(m_spawnVFXInstance);
            m_spawnVFXInstance = null;
        }
    }
}
