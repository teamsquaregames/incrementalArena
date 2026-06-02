using Unity.Cinemachine;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Effects/ScreenShake")]
public class ScreenShakeEffect : AbilityEffect
{
    public override double Execute(AbilityContext ctx, Entity target)
    {
        if (!ctx.caster.TryGetModule(out EntityAbilityModule abilityModule)) return 0;

        abilityModule.ImpulseSource?.GenerateImpulse(ctx.value);
        return ctx.value;
    }
}
