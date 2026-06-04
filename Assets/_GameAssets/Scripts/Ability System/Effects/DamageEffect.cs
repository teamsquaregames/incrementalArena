using Stats;
using UnityEngine;
using Utils;

[CreateAssetMenu(menuName = "Abilities/Effects/Damage")]
public class DamageEffect : AbilityEffect
{
    public override void Execute(AbilityContext ctx, Entity target)
    {
        if (!target.TryGetModule(out EntityHealthModule healthModule)) return;

        double damage = ctx.value;

        if (ctx.caster.TryGetModule(out EntityStatModule statModule))
            damage += statModule.GetValue(StatType.AttackDamage);

        // this.Log($"Base damage: {damage}, crit: {ctx.isCrit}, crit multiplier: {StatManager.Instance.GetInstanceValue(ctx.caster.gameObject, StatType.CriticalDamage)}%");
        if (ctx.isCrit)
            damage *= StatManager.Instance.GetInstanceValue(ctx.caster.gameObject, StatType.CriticalDamage)/100;

        double damageDealth = healthModule.TakeDamage(damage, ctx.isCrit);
        ctx.module?.NotifyDamageDealt(damageDealth);
    }
}