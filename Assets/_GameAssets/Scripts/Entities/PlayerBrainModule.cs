using MyBox;
using UnityEngine;

/// <summary>
/// Player brain:
///   • Always moves toward the cursor world position.
///   • If there are enemies inside the cursor area, moves toward the closest one instead.
///   • Once within auto-attack range of the target, stops and fires the auto-attack.
/// </summary>
public class PlayerBrainModule : EntityBrainModule
{
    [Header("Movement")]
    [SerializeField, Min(0f)] private float m_stopRadius = 0.2f;

    protected override void Think()
    {
        if (CursorManager.Instance == null) return;
        if (!Owner.TryGetModule(out EntityAbilityModule abilityModule)) return;

        Entity targetEnemy = GetClosestEnemyInCursor();
        Vector3 targetPosition = targetEnemy != null
            ? targetEnemy.transform.position
            : CursorManager.Instance.MouseWorldPosition;

        float distanceToTarget = Vector3.Distance(
            new Vector3(Owner.transform.position.x, 0f, Owner.transform.position.z),
            new Vector3(targetPosition.x, 0f, targetPosition.z));

        bool inAttackRange = targetEnemy != null &&
                             distanceToTarget <= abilityModule.AutoAttack.range;

        if (inAttackRange)
        {
            StopMovement();
            TryAutoAttack(targetEnemy.transform.position.OffsetY(0.75f));
        }
        else
        {
            print("CancelAbility");
            abilityModule.CancelAbility();
            MoveToward(targetPosition);
        }
    }

    // ─── Private helpers ──────────────────────────────────────────────────────

    /// <summary>Returns the enemy inside the cursor that is closest to the player, or null if none.</summary>
    private Entity GetClosestEnemyInCursor()
    {
        Entity closest = null;
        float closestSqrDist = float.MaxValue;

        foreach (Entity entity in CursorManager.Instance.EntitiesInCursor)
        {
            if (!entity.TryGetModule(out EntityTeamModule tm) || tm.Team != Team.Enemy)
                continue;

            float sqrDist = (entity.transform.position - Owner.transform.position).sqrMagnitude;
            if (sqrDist < closestSqrDist)
            {
                closestSqrDist = sqrDist;
                closest = entity;
            }
        }

        return closest;
    }

    /// <summary>Drives the movement module toward a world-space position.</summary>
    private void MoveToward(Vector3 worldTarget)
    {
        Vector3 delta = worldTarget - Owner.transform.position;
        Vector2 flatDelta = new Vector2(delta.x, delta.z);

        SetMoveInput(flatDelta.sqrMagnitude > m_stopRadius * m_stopRadius
            ? flatDelta.normalized
            : Vector2.zero);
    }
}