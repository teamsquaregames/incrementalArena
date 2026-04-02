using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Effects/KnockUp")]
public class KnockUpEffect : AbilityEffect
{
    public override void Execute(AbilityContext ctx, Entity target)
    {
        if (target == null) return;

        target.KnockUp(ctx.Value);
    }
}