using Lean.Pool;
using UnityEngine;

public class EnemyHealthUIModule : EntityHealthUIModule
{
    [SerializeField] private GenericGauge m_genericGaugePrefab;
    [SerializeField] private Transform m_healthBarTarget;
    [SerializeField] private FloatingTextConfig m_floatingTextConfig;

    protected override void SpawnHealthBar(float maxHealth)
    {
        Transform canvasTransform = UIManager.Instance.GetCanvas<GameCanvas>().transform;
        m_genericGauge = LeanPool.Spawn(m_genericGaugePrefab, canvasTransform);
        m_genericGauge.Setup(m_healthBarTarget, maxHealth, maxHealth);
    }

    protected override void DespawnHealthBar()
    {
        if (m_genericGauge == null) return;

        LeanPool.Despawn(m_genericGauge);
        m_genericGauge = null;
    }

    protected override void OnDamageTextRequested(float amount, bool isCrit)
    {
        string text = isCrit ? $"<sprite=\"crit\" name=\"crit\"> {amount:N0}" : amount.ToString("N0");
        FloatingTextConfig config = isCrit ? GameAssets.Instance.critDamageTextConfig : m_floatingTextConfig;

        FloatingTextManager.Instance.SpawnWorldText(m_healthBarTarget.position, text, config);
    }
}
