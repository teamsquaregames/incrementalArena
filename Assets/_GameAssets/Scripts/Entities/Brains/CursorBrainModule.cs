using MyBox;
using Sirenix.OdinInspector;
using UnityEngine;
using Utils;

/// <summary>
/// Cursor brain — the "point-and-click" feel:
///   • The character moves toward the cursor at all times.
///   • When there are enemies inside the cursor area, attacks and abilities
///     fire automatically (abilities take priority over auto-attacks).
///   • No manual key input for attacks or abilities — everything is automatic
///     when a valid target is present.
/// </summary>
public class CursorBrainModule : EntityBrainModule
{
    [Header("Movement")]
    [SerializeField, Min(0f)] private float m_stopRadius      = 0.2f;
    [SerializeField, Min(0f)] private float m_minMoveThreshold = 0.2f;

    [ReadOnly]
    [SerializeField] private bool m_isMoving;

    protected override void Think()
    {
        if (CursorManager.Instance == null) return;
        if (!Owner.TryGetModule(out EntityAbilityModule abilityModule)) return;

        // ── 1. While a non-auto ability is animating, block all input ─────────
        if (abilityModule.IsUsingAbility) return;

        Entity targetEnemy = GetClosestEnemyInCursor();

        // ── 2. Try to use a regular ability automatically if an enemy is present
        if (targetEnemy != null)
        {
            Vector3 aimPoint = targetEnemy.transform.position.OffsetY(0.75f);

            // Iterate over available abilities and fire the first one that is off cooldown
            for (int i = 0; i < abilityModule.Abilities.Count; i++)
            {
                if (TryUseAbility(i, aimPoint))
                    return; // ability fired — skip auto-attack this frame
            }
        }

        // ── 3. Normal auto-attack + movement logic ────────────────────────────
        Vector3 targetPosition = targetEnemy != null
            ? targetEnemy.transform.position
            : CursorManager.Instance.MouseWorldPosition;

        float distanceToTarget = Vector3.Distance(
            new Vector3(Owner.transform.position.x, 0f, Owner.transform.position.z),
            new Vector3(targetPosition.x,           0f, targetPosition.z));

        bool inAttackRange = targetEnemy != null
                             && distanceToTarget <= abilityModule.AutoAttack.range;

        if (inAttackRange)
        {
            StopMovement();
            TryAutoAttack(targetEnemy.transform.position.OffsetY(0.75f));
        }
        else
        {
            MoveToward(targetPosition);
        }
    }

    // ─── Private helpers ──────────────────────────────────────────────────────

    /// <summary>Returns the enemy inside the cursor closest to the player, or null if none.</summary>
    private Entity GetClosestEnemyInCursor()
    {
        Entity closest        = null;
        float  closestSqrDist = float.MaxValue;

        foreach (Entity entity in CursorManager.Instance.EntitiesInCursor)
        {
            if (!entity.TryGetModule(out EntityTeamModule tm) || tm.Team != Team.Enemy)
                continue;

            float sqrDist = (entity.transform.position - Owner.transform.position).sqrMagnitude;
            if (sqrDist < closestSqrDist)
            {
                closestSqrDist = sqrDist;
                closest        = entity;
            }
        }

        return closest;
    }

    /// <summary>Drives the movement module toward a world-space position.</summary>
    private void MoveToward(Vector3 worldTarget)
    {
        Vector3 delta     = worldTarget - Owner.transform.position;
        Vector2 flatDelta = new Vector2(delta.x, delta.z);

        if (m_isMoving || flatDelta.sqrMagnitude - m_stopRadius * m_stopRadius > m_minMoveThreshold * m_minMoveThreshold)
        {
            m_isMoving = true;
            SetMoveInput(flatDelta.sqrMagnitude > m_stopRadius * m_stopRadius
                ? flatDelta.normalized
                : Vector2.zero);
        }

        if (flatDelta.sqrMagnitude <= m_stopRadius * m_stopRadius)
        {
            m_isMoving = false;
        }

        // this.Log($"Moving toward distance: {flatDelta.sqrMagnitude - m_stopRadius * m_stopRadius}");
    }
}