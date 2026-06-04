using Sirenix.OdinInspector;
using UnityEngine;

public class EntityHealthUIModule : EntityModule
{
    [SerializeField, Required] private FloatingTextConfig m_floatingTextConfig;
    [SerializeField, Required] private FloatingTextConfig m_blockFloatingTextConfig;
    [SerializeField, Required] private FloatingTextConfig m_criticalFloatingTextConfig;
    [SerializeField, Required] protected Transform m_healthBarTarget;

    protected GenericGauge m_genericGauge;

    public override void OnAllModuleInitialized()
    {
        if (!Owner.TryGetModule(out EntityHealthModule healthModule))
        {
            Debug.LogWarning($"[EntityHealthUIModule] No EntityHealthModule found on {Owner.name}. Health bar will not function.");
            return;
        }

        healthModule.OnDamageTaken += OnTakeDamage;
        healthModule.OnHealthChanged += HandleHealthChanged;
        healthModule.OnDeath += OnDeathStart;
        healthModule.OnDeathAnimEnd += OnDeath;

        SpawnHealthBar(healthModule.MaxHealth);
    }

    protected virtual void SpawnHealthBar(double maxHealth) { }

    protected virtual void DespawnHealthBar() { }


    protected void HandleHealthChanged(double currentHealth, double maxHealth, bool suppressFeedback)
    {
        if (m_genericGauge == null) return;
        m_genericGauge.SetValue(currentHealth, maxHealth, instant: false, showChunks: !suppressFeedback);
    }

    protected void OnTakeDamage(double amount, bool isCrit)
    {
        OnFloatingTextRequested((float)amount, isCrit);
    }

    protected void OnFloatingTextRequested(float amount, bool isCrit)
    {
        Vector3 spawnPos = m_healthBarTarget != null ? m_healthBarTarget.position : Owner.transform.position;

        if (amount == 0f)
            FloatingTextManager.Instance.SpawnWorldText(spawnPos, "0", m_blockFloatingTextConfig);
        else if (isCrit)
            FloatingTextManager.Instance.SpawnWorldText(spawnPos, $"{amount:N0}", m_criticalFloatingTextConfig);
        else
            FloatingTextManager.Instance.SpawnWorldText(spawnPos, $"{amount:N0}", m_floatingTextConfig);
    }

    protected virtual void OnDeathStart()
    {
        if (Owner.TryGetModule(out EntityHealthModule healthModule))
        {
            healthModule.OnDeath -= OnDeathStart;
            healthModule.OnDeathAnimEnd -= OnDeath;
        }

        DespawnHealthBar();
    }

    protected virtual void OnDeath()
    {
        if (Owner.TryGetModule(out EntityHealthModule healthModule))
        {
            healthModule.OnDeath -= OnDeathStart;
            healthModule.OnDeathAnimEnd -= OnDeath;
        }

        DespawnHealthBar();
    }
}
