using Stats;
using Unity.Cinemachine;
using UnityEngine;

public class PlayerHealthModule : EntityHealthModule
{
    [SerializeField] private CinemachineImpulseSource m_impulseSource;
    private DamageVignetteUIC m_damageVignette => UIManager.Instance.GetCanvas<GameCanvas>().GetContainer<DamageVignetteUIC>();

    protected override void PlayDamageFeedback(float damagePercentage)
    {
        base.PlayDamageFeedback(damagePercentage);
        m_damageVignette.Flash(damagePercentage);
        float currentHealthPercentage = m_currentHealth / MaxHealth;
        if (currentHealthPercentage <= 0.5f && currentHealthPercentage > 0f)
            m_damageVignette.LowHealthWarning(currentHealthPercentage);
        m_impulseSource?.GenerateImpulse(.08f);
    }

    private void Update()
    {
        if (m_isDead || m_statModule == null) return;
        if (!LevelManager.Instance.IsWaveActive) return;

        float healthLostPerSecond = m_statModule.GetValue(StatType.PlayerHealthLostPerSecond);
        if (healthLostPerSecond > 0f)
        {
            TakeDamage(healthLostPerSecond * Time.deltaTime, false, suppressFeedback: true);
            float currentHealthPercentage = m_currentHealth / MaxHealth;
            if (currentHealthPercentage <= 0.5f && currentHealthPercentage > 0f)
                m_damageVignette.LowHealthWarning(currentHealthPercentage);
        }
    }

    public override void Die()
    {
        if (GameConfig.Instance.cheatSettings.playerImmortality)
            return;
        m_damageVignette.StopLowHealthWarning();

        base.Die();
    }
}