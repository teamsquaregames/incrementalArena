using Lean.Pool;
using UnityEngine;

public class EntityHealthUIModule : EntityModule
{
    [SerializeField] private GenericGauge m_genericGaugePrefab;
    [SerializeField] private Transform m_healthBarTarget;
    [SerializeField] private FloatingTextConfig m_floatingTextConfig;

    private GenericGauge m_genericGauge;

    public override void OnAllModuleInitialized()
    {
        if (!Owner.TryGetModule(out EntityHealthModule healthModule))
        {
            Debug.LogWarning($"[EntityHealthUIModule] No EntityHealthModule found on {Owner.name}. Health bar will not function.");
            return;
        }

        healthModule.OnHealthChanged += HandleHealthChanged;
        healthModule.OnDeathStart += OnDeathStart;
        healthModule.OnDeath += OnDeath;

        SpawnHealthBar(m_healthBarTarget, healthModule.MaxHealth);
    }

    private void SpawnHealthBar(Transform target, float maxHealth)
    {
        Transform canvasTransform = UIManager.Instance.GetCanvas<GameCanvas>().transform;
        m_genericGauge = LeanPool.Spawn(m_genericGaugePrefab, canvasTransform);
        m_genericGauge.Setup(target, maxHealth, maxHealth);
    }

    private void DespawnHealthBar()
    {
        if (m_genericGauge == null) return;
        
        LeanPool.Despawn(m_genericGaugePrefab);
        m_genericGauge = null;
    }

    private void HandleHealthChanged(float currentHealth, float maxHealth, float delta)
    {
        if (m_genericGauge == null) return;
        m_genericGauge.SetValue(currentHealth, maxHealth);
        
        FloatingTextManager.Instance.SpawnWorldText(m_healthBarTarget.position, delta.ToString("N0"), m_floatingTextConfig);
    }

    private void OnDeathStart()
    {
        if (Owner.TryGetModule(out EntityHealthModule healthModule))
        {
            healthModule.OnHealthChanged -= HandleHealthChanged;
            healthModule.OnDeathStart -= OnDeathStart;
            healthModule.OnDeath -= OnDeath;
        }

        if (m_genericGauge != null)
        { 
            LeanPool.Despawn(m_genericGauge);   
        }
    }
    
    private void OnDeath()
    {
        if (Owner.TryGetModule(out EntityHealthModule healthModule))
        {
            healthModule.OnHealthChanged -= HandleHealthChanged;
            healthModule.OnDeathStart -= OnDeathStart;
            healthModule.OnDeath -= OnDeath;
        }

        if (m_genericGauge != null)
        { 
            LeanPool.Despawn(m_genericGauge);   
        }
    }
}
