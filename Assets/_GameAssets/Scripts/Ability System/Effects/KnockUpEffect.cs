using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Effects/KnockUp")]
public class KnockUpEffect : AbilityEffect
{
    public override double Execute(AbilityContext ctx, Entity target)
    {
        if (target == null) return 0;

        target.KnockUp(ctx.value);
        return ctx.value;
    }
}