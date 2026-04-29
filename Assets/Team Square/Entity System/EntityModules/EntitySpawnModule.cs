using System.Collections;
using DG.Tweening;
using Lean.Pool;
using Sirenix.OdinInspector;
using UnityEngine;

public class EntitySpawnModule : EntityModule
{
    [SerializeField] private ParticleSystem m_spawnVFXPrefab;
    [SerializeField] private string m_animationTrigger = "Spawn";
    [SerializeField] private float m_delayBeforeVisible = 0.5f;
    [SerializeField] private float m_vfxShrinkDuration = 0.3f;
    
    [ValidateInput(nameof(HasValidModelRenderers), "Assign all model renderers. The array cannot be null, empty, or contain null elements.")]
    [SerializeField, Required] private Renderer[] m_modelRenderers;

    private ParticleSystem m_spawnVFXInstance;
    private Coroutine m_spawnSequenceCR;

    public override void OnAllModuleInitialized()
    {
        if (Owner.TryGetModule(out EntityHealthModule healthModule))
            healthModule.OnDeathStart += OnDeathStart;

        Owner.SetSpawning(true);
        SetRenderersVisible(false);
        m_spawnSequenceCR = StartCoroutine(SpawnSequenceCR());
    }

    private IEnumerator SpawnSequenceCR()
    {
        if (m_spawnVFXPrefab != null)
            m_spawnVFXInstance = LeanPool.Spawn(m_spawnVFXPrefab, Owner.transform.position, Quaternion.identity);

        yield return new WaitForSeconds(m_delayBeforeVisible);

        SetRenderersVisible(true);
        Owner.Animator.SetTrigger(m_animationTrigger);
        m_spawnSequenceCR = null;
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
        StopSpawnSequence();
        Owner.SetSpawning(false);
        SetRenderersVisible(true);
        ShrinkAndDespawnVFX();
    }

    private void StopSpawnSequence()
    {
        if (m_spawnSequenceCR == null) return;

        StopCoroutine(m_spawnSequenceCR);
        m_spawnSequenceCR = null;
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

        StopSpawnSequence();
        Owner.SetSpawning(false);
        SetRenderersVisible(true);

        if (m_spawnVFXInstance != null)
        {
            m_spawnVFXInstance.transform.DOKill();
            LeanPool.Despawn(m_spawnVFXInstance);
            m_spawnVFXInstance = null;
        }
    }

    private void SetRenderersVisible(bool visible)
    {
        foreach (Renderer r in m_modelRenderers)
            r.enabled = visible;
    }

    private bool HasValidModelRenderers(Renderer[] renderers)
    {
        if (renderers == null || renderers.Length == 0)
            return false;

        foreach (Renderer renderer in renderers)
        {
            if (renderer == null)
                return false;
        }

        return true;
    }
}
