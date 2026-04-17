using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
using Utils;
using Lean.Pool;


public static class ResolveApplication
{
    public static List<Entity> ResolveApplications(TargetingInfo targetingInfo, AbilityContext context, int applicationIndex = 0)
    {
        //Debug.Log($"Resolving applications for ability {context.AbilityConfig.abilityName}, step {context.CurrentStepIndex}");
        List<Entity> results = new List<Entity>();
        
        List<Entity> preselectedTargets = targetingInfo.quickTarget switch
        {
            TargetingInfo.QuickTarget.Self => new List<Entity> { context.Caster },
            TargetingInfo.QuickTarget.Current => context.ClosestEntity != null ? new List<Entity> { context.ClosestEntity } : new List<Entity>(),
            // TargetingInfo.QuickTarget.Cursor => UtilsClass.GetMouseWorldPosition(),
            _ => null
        };
        //context.Caster.Log($"Preselected targets based on quick target {targetingInfo.quickTarget}: {string.Join(", ", preselectedTargets?.ConvertAll(t => t.name) ?? new List<string> { "None" })}");

        Vector3 position = targetingInfo.quickTarget switch
        {
            TargetingInfo.QuickTarget.Self => context.Caster.transform.position,
            TargetingInfo.QuickTarget.Current => context.ClosestEntity != null ? context.ClosestEntity.transform.position : context.AimPosition,
            // TargetingInfo.QuickTarget.Cursor => UtilsClass.GetMouseWorldPosition(),
            _ => context.AimPosition
        };

        return ApplicationSwitch(context.AbilityConfig.steps[context.CurrentStepIndex].applicationInfos[applicationIndex], position, context, preselectedTargets);
    }

    private static List<Entity> ApplicationSwitch(AbilityApplicationInfo applicationInfo, Vector3 position, AbilityContext context, List<Entity> preselectedTargets)
    {
        // Debug.Log($"Resolving application of type {applicationInfo.effectZoneType} at position {position} with context {context.AbilityConfig.abilityName}, step {context.CurrentStepIndex}");
        
        switch (applicationInfo.effectZoneType)
        {
            case AbilityApplicationInfo.Type.Direct:
                return preselectedTargets ?? new List<Entity>();
            case AbilityApplicationInfo.Type.Aoe:
                return AoeSwitch(applicationInfo, position, context);
            case AbilityApplicationInfo.Type.Projectile:
                return ProjectileApplication(applicationInfo, position, context);
            case AbilityApplicationInfo.Type.Custom:
                // Handle custom effect application
                break;
        }

        //Debug.LogWarning("ApplicationSwitch is not implemented yet. Returning empty entity list.");
        return new List<Entity>();
    }

    public static List<Entity> AoeSwitch(AbilityApplicationInfo applicationInfo, Vector3 position, AbilityContext context)
    {
        //Debug.Log($"Resolving AoE application with shape {applicationInfo.aoeInfo.effectShape} at position {position} for ability {context.AbilityConfig.abilityName}, step {context.CurrentStepIndex}");

        Transform owner = context.Caster != null ? context.Caster.transform : null;
        Quaternion rotation = owner != null ? owner.rotation : Quaternion.identity;
        Vector3 rotatedOffset = rotation * applicationInfo.aoeInfo.offset;
        Collider[] hits = new Collider[0];
        Collider casterCollider = context.Caster.GetComponent<Collider>();
        List<Entity> results = new List<Entity>();

        switch (applicationInfo.aoeInfo.effectShape)
        {
            case AoEInfo.Shape.Circle:
                //Debug.Log($"Performing circular AoE at {position + rotatedOffset} with radius {applicationInfo.aoeInfo.radius}");
                hits = Physics.OverlapSphere(position + rotatedOffset, applicationInfo.aoeInfo.Radius(context.AbilityConfig));
                break;

            case AoEInfo.Shape.Rect:
                hits = Physics.OverlapBox(
                    position + rotatedOffset,
                    applicationInfo.aoeInfo.scale / 2f,
                    rotation  // ← Rotation de l'owner
                );
                break;

            case AoEInfo.Shape.Cone:
                // Implement cone-shaped AoE logic here (this is more complex and may require custom calculations)
                //Debug.LogWarning("Cone-shaped AoE is not implemented yet. Returning empty collider array.");
                hits = new Collider[0];
                break;
            default:
                //Debug.LogWarning("Unknown AoE shape. Returning empty collider array.");
                hits = new Collider[0];
                break;
        }
        

        foreach (var hit in hits)
        {
            if (hit == casterCollider) continue;

            if (EntityManager.Instance != null &&
                EntityManager.Instance.EntitiesByCollider.TryGetValue(hit, out Entity entity))
            {
                results.Add(entity);
            }
        }
        return results;
    }

    private static List<Entity> ProjectileApplication(AbilityApplicationInfo applicationInfo, Vector3 position, AbilityContext context)
    {
        Debug.Log($"Spawning projectile for ability '{context.AbilityConfig.abilityName}' at position {position} with context {context.CurrentStepIndex}");
        LeanPool.Spawn(applicationInfo.projectileInfo.prefab, position, Quaternion.identity).Spawn(context, applicationInfo, context.CurrentStepIndex);
        return new List<Entity>();
    }
}