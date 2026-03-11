using UnityEngine;
using UnityEngine.Serialization;

public class EntityHealthUIModule : EntityModule
{
    [SerializeField] private GenericGaugePoolRef m_genericGaugePoolRef;
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
        healthModule.OnDeathStart += HandleDeathStart;

        SpawnHealthBar(m_healthBarTarget, healthModule.MaxHealth);
    }

    private void SpawnHealthBar(Transform target, float maxHealth)
    {
        Transform canvasTransform = UIManager.Instance.GetCanvas<GameCanvas>().transform;
        m_genericGauge = m_genericGaugePoolRef.pool.Spawn(canvasTransform);
        m_genericGauge.Setup(target, maxHealth, maxHealth);
    }

    private void DespawnHealthBar()
    {
        if (m_genericGauge == null) return;
        m_genericGaugePoolRef.pool.Despawn(m_genericGauge);
        m_genericGauge = null;
    }

    private void HandleHealthChanged(float currentHealth, float maxHealth, float delta)
    {
        if (m_genericGauge == null) return;
        m_genericGauge.SetValue(currentHealth, maxHealth);
        
        FloatingTextManager.Instance.SpawnWorldText(m_healthBarTarget.position, delta.ToString("N0"), m_floatingTextConfig);
    }

    private void HandleDeathStart()
    {
        if (Owner.TryGetModule(out EntityHealthModule healthModule))
        {
            healthModule.OnHealthChanged -= HandleHealthChanged;
            healthModule.OnDeathStart -= HandleDeathStart;
        }

        if (m_genericGauge != null)
            m_genericGauge.HideGauge(true, DespawnHealthBar);
    }
}
