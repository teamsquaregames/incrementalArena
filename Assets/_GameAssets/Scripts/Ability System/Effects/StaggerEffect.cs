using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Effects/Stun")]
public class StaggerEffect : AbilityEffect
{
    public override void Execute(AbilityContext ctx, Entity target)
    {
        if (target == null) return;

        target.Stagger(ctx.Value);
    }
}