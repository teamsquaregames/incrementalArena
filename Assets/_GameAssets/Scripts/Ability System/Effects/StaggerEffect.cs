using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Effects/Stun")]
public class StaggerEffect : AbilityEffect
{
    public override double Execute(AbilityContext ctx, Entity target)
    {
        if (target == null) return 0;

        target.Stagger(ctx.value);
        return ctx.value;
    }
}