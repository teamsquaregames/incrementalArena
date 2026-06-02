using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Effects/Movement")]
public class ImpulseEffect : AbilityEffect
{
    public override double Execute(AbilityContext ctx, Entity target)
    {
        if (!target.TryGetModule(out EntityMovementModule movementModule)) return 0;

        Vector3 direction = ctx.caster.transform.forward;

        movementModule.AddImpulse(direction * ctx.value);
        return ctx.value;
    }
}