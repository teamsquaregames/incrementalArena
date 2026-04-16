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
    [SerializeField, Min(0f)] private float m_stopRadius = 0.2f;
    [SerializeField, Min(0f)] private float m_minMoveThreshold = 0.2f;

    [ReadOnly]
    [SerializeField] private bool m_isMoving;

    protected override void Think()
    {
        // this.Log("Thinking...");

        if (CursorManager.Instance == null) return;
        if (!Owner.TryGetModule(out EntityAbilityModule abilityModule)) return;


        Entity targetEnemy = GetClosestEnemyInCursor(Owner);

        // ── 2. Try to use a regular ability automatically if an enemy is present
        if (targetEnemy != null && !abilityModule.IsUsingAbility)
        {
            Vector3 aimPoint = targetEnemy.transform.position.OffsetY(0f);

            // Iterate over available abilities and fire the first one that is off cooldown
            for (int i = 0; i < abilityModule.Abilities.Count; i++)
            {
                // this.Log($"Trying to use ability {abilityModule.Abilities[i].name} toward {aimPoint}");
                if (TryUseAbility(i, aimPoint))
                    return; // ability fired — skip auto-attack this frame
            }
        }

        // ── 3. Normal auto-attack + movement logic ────────────────────────────
        Vector3 targetPosition = targetEnemy != null //|| abilityModule.IsUsingAbility
            ? targetEnemy.transform.position
            : CursorManager.Instance.MouseWorldPosition;

        if (!abilityModule.IsUsingAbility)
        {
            float distanceToTarget = Vector3.Distance(
                new Vector3(Owner.transform.position.x, 0f, Owner.transform.position.z),
                new Vector3(targetPosition.x, 0f, targetPosition.z));

            bool inAARange = targetEnemy != null
                                 && distanceToTarget <= abilityModule.AutoAttack.Range(Owner).y
                                 && distanceToTarget >= abilityModule.AutoAttack.Range(Owner).x;

            if (inAARange)
            {
                StopMovement();
                TryAutoAttack(targetEnemy.transform.position.OffsetY(0f));
            }
            else
            {
                MoveToward(targetPosition);
            }
        }
        else if (!abilityModule.IsCastRooted)
        {
            MoveToward(CursorManager.Instance.MouseWorldPosition, true);
        }
    }

    // ─── Private helpers ──────────────────────────────────────────────────────

    /// <summary>Returns the enemy inside the cursor closest to the player, or null if none.</summary>
    public static Entity GetClosestEnemyInCursor(Entity owner)
    {
        Entity closest = null;
        float closestSqrDist = float.MaxValue;

        foreach (Entity entity in CursorManager.Instance.EntitiesInCursor)
        {
            if (!entity.TryGetModule(out EntityTeamModule tm) || tm.Team != Team.Enemy)
                continue;

            float sqrDist = (entity.transform.position - owner.transform.position).sqrMagnitude;
            if (sqrDist < closestSqrDist)
            {
                closestSqrDist = sqrDist;
                closest = entity;
            }
        }

        return closest;
    }

    /// <summary>Drives the movement module toward a world-space position.</summary>
    private void MoveToward(Vector3 worldTarget, bool ignoreStopRadius = false)
    {
        Vector3 delta = worldTarget - Owner.transform.position;
        Vector2 flatDelta = new Vector2(delta.x, delta.z);

        float stopRadiusSqr = m_stopRadius * m_stopRadius;
        if (ignoreStopRadius)
            stopRadiusSqr = 0f;

        if (m_isMoving || flatDelta.sqrMagnitude - stopRadiusSqr > m_minMoveThreshold * m_minMoveThreshold)
        {
            m_isMoving = true;
            SetMoveInput(flatDelta.sqrMagnitude > stopRadiusSqr
                ? flatDelta.normalized
                : Vector2.zero);
        }

        if (flatDelta.sqrMagnitude <= stopRadiusSqr)
        {
            m_isMoving = false;
        }

        // this.Log($"Moving toward distance: {flatDelta.sqrMagnitude - stopRadiusSqr}");
    }
}