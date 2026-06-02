using UnityEngine;
using Utils;

[CreateAssetMenu(menuName = "Abilities/Effects/Dash")]
public class DashEffect : AbilityEffect
{
    public override double Execute(AbilityContext ctx, Entity target)
    {
        // this.Log($"Executing DashEffect on target {target.name} towards position {ctx.aimPosition} with value {ctx.value}");
        if (!target.TryGetModule(out EntityMovementModule movementModule)) return 0;

        movementModule.DashToPosition(ctx.aimPosition, ctx.value);
        return ctx.value;
    }
}