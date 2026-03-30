using UnityEngine;
using Utils;

[CreateAssetMenu(menuName = "Abilities/Effects/Dash")]
public class DashEffect : AbilityEffect
{
    public override void Execute(AbilityContext ctx, Entity target)
    {
        // this.Log($"Executing DashEffect on target {target.name} towards position {ctx.AimPosition} with value {ctx.Value}");
        if (!target.TryGetModule(out EntityMovementModule movementModule)) return;

        movementModule.DashToPosition(ctx.AimPosition, ctx.Value);
    }
}