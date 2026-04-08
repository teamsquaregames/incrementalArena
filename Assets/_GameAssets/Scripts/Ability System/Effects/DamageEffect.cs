using Stats;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Effects/Damage")]
public class DamageEffect : AbilityEffect
{
    public override void Execute(AbilityContext ctx, Entity target)
    {
        if (!target.TryGetModule(out EntityHealthModule healthModule)) return;

        float damage = ctx.Value;
        bool isCrit = false;

        if (ctx.Caster.TryGetModule(out EntityStatModule statModule))
        {
            damage += statModule.GetValue(StatType.AttackDamage);
            isCrit = Random.value < statModule.GetValue(StatType.CriticalChance);
        }

        healthModule.TakeDamage(damage, isCrit);
    }
}