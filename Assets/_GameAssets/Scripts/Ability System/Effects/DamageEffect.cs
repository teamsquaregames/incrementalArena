using Stats;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Effects/Damage")]
public class DamageEffect : AbilityEffect
{
    public override void Execute(AbilityContext ctx, Entity target)
    {
        if (!target.TryGetModule(out EntityHealthModule healthModule)) return;

        float damage = ctx.value;

        if (ctx.caster.TryGetModule(out EntityStatModule statModule))
            damage += statModule.GetValue(StatType.AttackDamage);

        healthModule.TakeDamage(damage, ctx.isCrit);
    }
}