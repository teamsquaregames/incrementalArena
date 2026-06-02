using Stats;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Effects/Damage")]
public class DamageEffect : AbilityEffect
{
    public override double Execute(AbilityContext ctx, Entity target)
    {
        if (!target.TryGetModule(out EntityHealthModule healthModule)) return 0;

        double damage = ctx.value;

        if (ctx.caster.TryGetModule(out EntityStatModule statModule))
            damage += statModule.GetValue(StatType.AttackDamage);

        healthModule.TakeDamage(damage, ctx.isCrit);
        ctx.module?.NotifyDamageDealt(damage);
        return damage;
    }
}