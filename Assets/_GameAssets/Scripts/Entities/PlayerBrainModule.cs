using MyBox;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;
using Utils;

/// <summary>
/// Player brain:
///   • Ability keys have highest priority and interrupt any ongoing auto-attack.
///   • While a non-auto ability is playing, movement and auto-attacks are suppressed.
///   • If there are enemies inside the cursor area, moves toward the closest one.
///   • Once within auto-attack range of the target, stops and fires the auto-attack.
/// </summary>
public class PlayerBrainModule : EntityBrainModule
{
    [Header("Movement")]
    [SerializeField, Min(0f)] private float m_stopRadius = 0.2f;
    [SerializeField, Min(0f)] private float m_minMoveThreshold = 0.2f;

    [ReadOnly]
    [SerializeField] private bool m_isMoving;

    protected override void Think()
    {
        if (CursorManager.Instance == null) return;
        if (!Owner.TryGetModule(out EntityAbilityModule abilityModule)) return;

        // ── 1. Ability input — highest priority, interrupts auto-attacks ──────
        if (Keyboard.current.digit1Key.wasPressedThisFrame)
        {
            abilityModule.CancelAbility();
            TryUseAbility(0, CursorManager.Instance.MouseWorldPosition.OffsetY(0.75f));
            return;
        }

        // ── 2. While a non-auto ability is animating, block input ─────────────
        if (abilityModule.IsUsingAbility) return;

        // ── 3. Normal auto-attack + movement logic ────────────────────────────
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

        this.Log($"Moving toward distance: {flatDelta.sqrMagnitude - m_stopRadius * m_stopRadius}");
    }
}