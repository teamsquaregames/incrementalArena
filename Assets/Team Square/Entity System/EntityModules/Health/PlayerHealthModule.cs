using Stats;
using UnityEngine;

public class PlayerHealthModule : EntityHealthModule
{
    private void Update()
    {
        if (m_isDead || m_statModule == null) return;
        if (!LevelManager.Instance.IsWaveActive) return;

        float healthLostPerSecond = m_statModule.GetValue(StatType.PlayerHealthLostPerSecond);
        if (healthLostPerSecond > 0f)
            TakeDamage(healthLostPerSecond * Time.deltaTime, false, suppressFeedback: true);
    }
}
