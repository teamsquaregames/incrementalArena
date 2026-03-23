using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Effects/Movement")]
public class ImpulseEffect : AbilityEffect
{
    public override void Execute(AbilityContext ctx, Entity target)
    {
        if (!target.TryGetModule(out EntityMovementModule movementModule)) return;

        Vector3 direction = ctx.Caster.transform.forward;

        movementModule.AddImpulse(direction * ctx.Value);
    }
}