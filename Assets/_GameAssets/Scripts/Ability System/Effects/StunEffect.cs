using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Effects/Stun")]
public class StunEffect : AbilityEffect
{
    public override void Execute(AbilityContext ctx, Entity target)
    {
        if (target == null) return;

        target.Stun(ctx.Value, true);
    }
}