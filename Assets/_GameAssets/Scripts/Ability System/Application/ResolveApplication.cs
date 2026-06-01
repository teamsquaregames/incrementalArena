using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
using Utils;
using Lean.Pool;


public static class ResolveApplication
{
    public static List<Entity> ResolveApplications(TargetingInfo targetingInfo, AbilityContext context, int applicationIndex)
    {
        //Debug.Log($"Resolving applications for ability {context.AbilityConfig.abilityName}, step {context.CurrentStepIndex}");
        List<Entity> preselectedTargets = targetingInfo.quickTarget switch
        {
            TargetingInfo.QuickTarget.Self => new List<Entity> { context.caster },
            TargetingInfo.QuickTarget.Current => context.closestEntity != null ? new List<Entity> { context.closestEntity } : new List<Entity>(),
            // TargetingInfo.QuickTarget.Cursor => UtilsClass.GetMouseWorldPosition(),
            _ => null
        };
        //context.Caster.Log($"Preselected targets based on quick target {targetingInfo.quickTarget}: {string.Join(", ", preselectedTargets?.ConvertAll(t => t.name) ?? new List<string> { "None" })}");

        Vector3 position = targetingInfo.quickTarget switch
        {
            TargetingInfo.QuickTarget.Self => context.caster.transform.position,
            TargetingInfo.QuickTarget.Current => context.closestEntity != null ? context.closestEntity.transform.position : context.aimPosition,
            // TargetingInfo.QuickTarget.Cursor => UtilsClass.GetMouseWorldPosition(),
            _ => context.aimPosition
        };

        AbilityApplicationInfo applicationInfo = context.abilityConfig.steps[context.currentStepIndex].applicationInfos[applicationIndex];

        return ApplicationSwitch(applicationInfo, position, context, preselectedTargets, applicationIndex);
    }

    private static List<Entity> ApplicationSwitch(AbilityApplicationInfo applicationInfo, Vector3 position, AbilityContext context, List<Entity> preselectedTargets, int applicationIndex)
    {
        // Debug.Log($"Resolving application of type {applicationInfo.effectZoneType} at position {position} with context {context.AbilityConfig.abilityName}, step {context.CurrentStepIndex}");

        switch (applicationInfo.effectZoneType)
        {
            case AbilityApplicationInfo.Type.Direct:
                return preselectedTargets ?? new List<Entity>();
            case AbilityApplicationInfo.Type.Aoe:
                return AoeSwitch(applicationInfo, position, context);
            case AbilityApplicationInfo.Type.Projectile:
                return ProjectileApplication(applicationInfo, context, applicationIndex);
            case AbilityApplicationInfo.Type.Custom:
                // Handle custom effect application
                break;
        }

        //Debug.LogWarning("ApplicationSwitch is not implemented yet. Returning empty entity list.");
        return new List<Entity>();
    }

    public static List<Entity> AoeSwitch(AbilityApplicationInfo applicationInfo, Vector3 position, AbilityContext context)
    {
        // Debug.Log($"Resolving AoE application with shape {applicationInfo.aoeInfo.effectShape} at position {position} for ability {context.abilityConfig.abilityName}, step {context.currentStepIndex}");

        Transform owner = context.caster != null ? context.caster.transform : null;
        Quaternion rotation = owner != null ? owner.rotation : Quaternion.identity;
        Vector3 rotatedOffset = rotation * applicationInfo.aoeInfo.offset;
        Collider[] hits = new Collider[0];
        Collider casterCollider = context.caster.GetComponent<Collider>();
        List<Entity> results = new List<Entity>();

        switch (applicationInfo.aoeInfo.effectShape)
        {
            case AoEInfo.Shape.Sphere:
                hits = Physics.OverlapSphere(position + rotatedOffset, applicationInfo.aoeInfo.Radius(context.abilityConfig));
                // Debug.Log($"Performing spherical AoE at {position + rotatedOffset} with radius {applicationInfo.aoeInfo.Radius(context.abilityConfig)}, found {hits.Length} colliders");
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

        GizmoDrawData gizmoData = new GizmoDrawData
        {
            abilityConfig = context.abilityConfig,
            applicationInfo = applicationInfo,
            // targetingInfo = null, // You can pass the actual TargetingInfo if needed
            position = position + rotatedOffset,
            hit = results.Count > 0,
            duration = 1f
        };
        if (context.caster != null && context.caster.TryGetModule(out EntityAbilityModule abilityModule))
        {
            abilityModule.SetApplicationGizmo(gizmoData);
        }

        return ResolveTeamApplication(results, applicationInfo.teamApplication, context.closestEntity, context.caster);
    }

    private static List<Entity> ResolveTeamApplication(List<Entity> entitiesTested, TeamApplication application, Entity target, Entity owner)
    {
        owner.TryGetModule(out EntityTeamModule ownerTeamModule);
        for (int i = entitiesTested.Count - 1; i >= 0; i--)
        {
            Entity entityTested = entitiesTested[i];
            entityTested.TryGetModule(out EntityTeamModule entityTeamModule);

            if (application.HasFlag(TeamApplication.Allies) && entityTeamModule.Team != ownerTeamModule.Team)
                entitiesTested.Remove(entityTested);
            else if (application.HasFlag(TeamApplication.Opponent) && entityTeamModule.Team == ownerTeamModule.Team)
                entitiesTested.Remove(entityTested);

            // Debug.Log($"Entity {entityTested.name} is on team {entityTeamModule.Team}, owner is on team {ownerTeamModule.Team}, application is {application}. Entity {(entitiesTested.Contains(entityTested) ? "is" : "is not")} included in final target list.");
        }

        return entitiesTested;
    }


    private static List<Entity> ProjectileApplication(AbilityApplicationInfo applicationInfo, AbilityContext context, int applicationIndex)
    {
        // Debug.Log($"Spawning projectile for ability '{context.abilityConfig.abilityName}' with context {context.currentStepIndex}");
        LeanPool.Spawn(applicationInfo.projectileInfo.prefab, new Vector3(), Quaternion.identity).Spawn(context, applicationIndex, context.currentStepIndex);
        return new List<Entity>();
    }

}