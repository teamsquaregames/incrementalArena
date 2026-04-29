using Lean.Pool;
using UnityEngine;

public class EnemyHealthUIModule : EntityHealthUIModule
{
    [SerializeField] private GenericGauge m_genericGaugePrefab;

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

}
