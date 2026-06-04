using Unity.Cinemachine;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Effects/ScreenShake")]
public class ScreenShakeEffect : AbilityEffect
{
    public override void Execute(AbilityContext ctx, Entity target)
    {
        if (!ctx.caster.TryGetModule(out EntityAbilityModule abilityModule)) return;

        abilityModule.ImpulseSource?.GenerateImpulse(ctx.value);
    }
}
