using UnityEngine;

public class EntityHealthUIModule : EntityModule
{
    [SerializeField] private FloatingTextConfig m_floatingTextConfig;
    [SerializeField] protected Transform m_healthBarTarget;

    protected GenericGauge m_genericGauge;

    public override void OnAllModuleInitialized()
    {
        if (!Owner.TryGetModule(out EntityHealthModule healthModule))
        {
            Debug.LogWarning($"[EntityHealthUIModule] No EntityHealthModule found on {Owner.name}. Health bar will not function.");
            return;
        }

        healthModule.OnHealthChanged += HandleHealthChanged;
        healthModule.OnDeath += OnDeathStart;
        healthModule.OnDeathAnimEnd += OnDeath;

        SpawnHealthBar(healthModule.MaxHealth);
    }

    protected virtual void SpawnHealthBar(float maxHealth) { }

    protected virtual void DespawnHealthBar() { }

    protected void OnDamageTextRequested(float amount, bool isCrit)
    {
        string text = isCrit ? $"<sprite=\"crit\" name=\"crit\"> {amount:N0}" : amount.ToString("N0");
        FloatingTextConfig config = isCrit ? GameAssets.Instance.critDamageTextConfig : m_floatingTextConfig;

        Vector3 spawnPos = m_healthBarTarget != null ? m_healthBarTarget.position : Owner.transform.position;
        FloatingTextManager.Instance.SpawnWorldText(spawnPos, text, config);
    }

    protected void HandleHealthChanged(float currentHealth, float maxHealth, float delta, bool isCrit, bool suppressFeedback)
    {
        if (m_genericGauge == null) return;
        m_genericGauge.SetValue(currentHealth, maxHealth, instant: false, showChunks: !suppressFeedback);

        if (delta < 0f && !suppressFeedback)
            OnDamageTextRequested(Mathf.Abs(delta), isCrit);
    }

    protected virtual void OnDeathStart()
    {
        if (Owner.TryGetModule(out EntityHealthModule healthModule))
        {
            healthModule.OnHealthChanged -= HandleHealthChanged;
            healthModule.OnDeath -= OnDeathStart;
            healthModule.OnDeathAnimEnd -= OnDeath;
        }

        DespawnHealthBar();
    }

    protected virtual void OnDeath()
    {
        if (Owner.TryGetModule(out EntityHealthModule healthModule))
        {
            healthModule.OnHealthChanged -= HandleHealthChanged;
            healthModule.OnDeath -= OnDeathStart;
            healthModule.OnDeathAnimEnd -= OnDeath;
        }

        DespawnHealthBar();
    }
}
