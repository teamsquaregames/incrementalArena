using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Effects/Damage")]
public class DamageEffect : AbilityEffect
{
    public override void Execute(AbilityContext ctx)
    {
        if (EntityManager.Instance == null) return;

        var hits = Physics.OverlapSphere(ctx.TargetPosition, ctx.AbilityData.aoeRadius);
        foreach (var hit in hits)
        {
            if (hit == ctx.Caster.GetComponent<Collider>()) return;

            if (EntityManager.Instance.EntitiesByCollider.TryGetValue(hit, out Entity entity))
            {
                if (entity.TryGetModule(out EntityHealthModule healthModule))
                    healthModule.TakeDamage(ctx.Value, false);
            }
        }
    }
}