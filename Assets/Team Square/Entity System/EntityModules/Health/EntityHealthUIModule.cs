using UnityEngine;

public class EntityHealthUIModule : EntityModule
{
    protected GenericGauge m_genericGauge;

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

        SpawnHealthBar(healthModule.MaxHealth);
    }

    protected virtual void SpawnHealthBar(float maxHealth) { }

    protected virtual void DespawnHealthBar() { }

    protected virtual void OnDamageTextRequested(float amount, bool isCrit) { }

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
            healthModule.OnDeathStart -= OnDeathStart;
            healthModule.OnDeath -= OnDeath;
        }

        DespawnHealthBar();
    }

    protected virtual void OnDeath()
    {
        if (Owner.TryGetModule(out EntityHealthModule healthModule))
        {
            healthModule.OnHealthChanged -= HandleHealthChanged;
            healthModule.OnDeathStart -= OnDeathStart;
            healthModule.OnDeath -= OnDeath;
        }

        DespawnHealthBar();
    }
}
