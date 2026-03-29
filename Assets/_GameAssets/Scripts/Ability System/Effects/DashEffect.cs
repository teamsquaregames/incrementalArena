using UnityEngine;
using Utils;

[CreateAssetMenu(menuName = "Abilities/Effects/Dash")]
public class DashEffect : AbilityEffect
{
    public override void Execute(AbilityContext ctx, Entity target)
    {
        if (!target.TryGetModule(out EntityMovementModule movementModule)) return;

        Vector3 direction = ctx.AimPosition - ctx.Caster.transform.position;
        // Vector3 targetPosition = ctx.Caster.transform.position + direction * ctx.Value;
        movementModule.DashToPosition(ctx.AimPosition, ctx.Value);
    }
}