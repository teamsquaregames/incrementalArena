using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Effects/Damage")]
public class DamageEffect : AbilityEffect
{
    public override void Execute(AbilityContext ctx, Entity target)
    {
        if (target.TryGetModule(out EntityHealthModule healthModule))
            healthModule.TakeDamage(ctx.Value, false);
    }
}