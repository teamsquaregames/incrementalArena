using Sirenix.OdinInspector;
using Stats;
using UnityEngine;

public class EnemyScaler : MonoBehaviour
{
    [TitleGroup("Health Scaling  |  f = b(x-1) + a·c^(x-1)")]
    public float healthA = 1f;
    public float healthB = 0f;
    public float healthC = 1f;

    [TitleGroup("Damage Scaling  |  f = b(x-1) + a·c^(x-1)")]
    public float damageA = 1f;
    public float damageB = 0f;
    public float damageC = 1f;

    public void ApplyScaling(Entity enemy, int roundIndex)
    {
        if (!enemy.TryGetModule(out EntityStatModule statModule)) return;

        float healthMultiplier = Compute(healthA, healthB, healthC, roundIndex);
        float damageMultiplier = Compute(damageA, damageB, damageC, roundIndex);
        
        Debug.Log($"{enemy} damage multiplier by {damageMultiplier}", enemy);

        statModule.AddModifier(new StatModifier(enemy.EntityType, StatType.MaxHealth,     healthMultiplier, ModifierType.Multiplier));
        statModule.AddModifier(new StatModifier(enemy.EntityType, StatType.AttackDamage,  damageMultiplier, ModifierType.Multiplier));

        if (enemy.TryGetModule(out EntityHealthModule healthModule))
            healthModule.UpdateCurrentHealth();
    }

    private float Compute(float a, float b, float c, int x)
    {
        return b * (x - 1) + a * Mathf.Pow(c, x - 1);
    }
}
