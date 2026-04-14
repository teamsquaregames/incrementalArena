using UnityEngine;

public class PlayerHealthUIModule : EntityHealthUIModule
{
    private PlayerHealthBarUIC PlayerHealthBarUIC => UIManager.Instance.GetCanvas<GameCanvas>().GetContainer<PlayerHealthBarUIC>();

    protected override void SpawnHealthBar(float maxHealth)
    {
        m_genericGauge = PlayerHealthBarUIC.PlayerHealthBar;
        PlayerHealthBarUIC.Open();
        m_genericGauge.Setup(null, maxHealth, maxHealth);
    }

    protected override void DespawnHealthBar()
    {
        if (m_genericGauge == null) return;

        PlayerHealthBarUIC.Close();
        m_genericGauge = null;
    }
}
