using System.Collections.Generic;
using MyBox;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;
using Utils;

/// <summary>
/// Passive brain — the "Vampire Survivors" feel. Almost everything fires
/// automatically; the player only steers the character.
///
///   Movement    : Configurable — either WASD/arrow keys OR mouse cursor
///                 (toggle via <see cref="m_movementMode"/> in the Inspector).
///   Facing      : Looks at the closest enemy if any exists; falls back to
///                 the mouse cursor position.
///   Auto-attack : Fires automatically at the closest enemy within AutoAttack.range.
///   Abilities   : Each ability checks its own range independently and fires
///                 on the closest enemy within that range when off cooldown.
///                 Ability list order = priority (index 0 fires first).
///
/// No button presses are required for combat — just stay alive and steer.
/// </summary>
public class PassiveBrainModule : EntityBrainModule
{
    // ─── Inspector ────────────────────────────────────────────────────────────

    public enum MovementMode { Keyboard, Mouse }

    [Header("Movement")]
    [SerializeField] private MovementMode m_movementMode = MovementMode.Keyboard;

    // Mouse-follow settings (only used when m_movementMode == Mouse)
    [SerializeField, Min(0f)] private float m_stopRadius       = 0.2f;
    [SerializeField, Min(0f)] private float m_minMoveThreshold = 0.2f;

    [ReadOnly]
    [SerializeField] private bool m_isMoving;

    // ─── Think ────────────────────────────────────────────────────────────────

    protected override void Think()
    {
        if (!Owner.TryGetModule(out EntityAbilityModule abilityModule)) return;

        // ── 1. Movement ───────────────────────────────────────────────────────
        switch (m_movementMode)
        {
            case MovementMode.Keyboard: ThinkKeyboardMovement(); break;
            case MovementMode.Mouse:    ThinkMouseMovement();    break;
        }

        // ── 2. Facing — closest enemy in the world, or mouse as fallback ──────
        Entity closestEnemy = GetClosestEnemy();
        if (closestEnemy != null)
        {
            FacePosition(closestEnemy.transform.position);
        }
        else if (CursorManager.Instance != null)
        {
            FacePosition(CursorManager.Instance.MouseWorldPosition);
        }

        // ── 3. While a non-auto ability is animating, hold off on combat ─────
        if (abilityModule.IsUsingAbility) return;

        // ── 4. Auto-cast abilities — each checks its own range independently ──
        // Abilities take priority; try each in order (index 0 = highest priority).
        for (int i = 0; i < abilityModule.Abilities.Count; i++)
        {
            AbilityConfig ability = abilityModule.Abilities[i];
            Entity target = GetClosestEnemyInRange(ability.range);
            if (target == null) continue;

            if (TryUseAbility(i, target.transform.position.OffsetY(0.75f)))
                return; // ability fired — skip auto-attack this frame
        }

        // ── 5. Auto-attack on the closest in-range enemy ──────────────────────
        if (abilityModule.AutoAttack != null)
        {
            Entity target = GetClosestEnemyInRange(abilityModule.AutoAttack.range);
            if (target != null)
                TryAutoAttack(target.transform.position.OffsetY(0.75f));
        }
    }

    // ─── Movement helpers ─────────────────────────────────────────────────────

    private void ThinkKeyboardMovement()
    {
        Vector2 input = Vector2.zero;
        if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)    input.y += 1f;
        if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)  input.y -= 1f;
        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) input.x += 1f;
        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)  input.x -= 1f;

        SetMoveInput(input.sqrMagnitude > 0f ? input.normalized : Vector2.zero);
    }

    private void ThinkMouseMovement()
    {
        if (CursorManager.Instance == null) return;

        Vector3 worldTarget = CursorManager.Instance.MouseWorldPosition;
        Vector3 delta       = worldTarget - Owner.transform.position;
        Vector2 flatDelta   = new Vector2(delta.x, delta.z);

        if (m_isMoving || flatDelta.sqrMagnitude - m_stopRadius * m_stopRadius > m_minMoveThreshold * m_minMoveThreshold)
        {
            m_isMoving = true;
            SetMoveInput(flatDelta.sqrMagnitude > m_stopRadius * m_stopRadius
                ? flatDelta.normalized
                : Vector2.zero);
        }

        if (flatDelta.sqrMagnitude <= m_stopRadius * m_stopRadius)
            m_isMoving = false;
    }

    // ─── Target resolution ────────────────────────────────────────────────────

    /// <summary>
    /// Returns the closest live enemy regardless of range, or null if none exist.
    /// Used for facing decisions.
    /// </summary>
    private Entity GetClosestEnemy()
    {
        if (EntityManager.Instance == null) return null;

        List<Entity> enemies = EntityManager.Instance.Enemies;
        Entity       closest = null;
        float        bestSqr = float.MaxValue;

        foreach (Entity enemy in enemies)
        {
            if (enemy == null) continue;
            float sqrDist = (enemy.transform.position - Owner.transform.position).sqrMagnitude;
            if (sqrDist < bestSqr)
            {
                bestSqr = sqrDist;
                closest = enemy;
            }
        }

        return closest;
    }

    /// <summary>
    /// Returns the closest live enemy within <paramref name="range"/>,
    /// or null if none qualifies. Used for ability and auto-attack targeting.
    /// </summary>
    private Entity GetClosestEnemyInRange(float range)
    {
        if (EntityManager.Instance == null) return null;

        List<Entity> enemies  = EntityManager.Instance.Enemies;
        Entity       closest  = null;
        float        sqrRange = range * range;
        float        bestSqr  = float.MaxValue;

        foreach (Entity enemy in enemies)
        {
            if (enemy == null) continue;
            float sqrDist = (enemy.transform.position - Owner.transform.position).sqrMagnitude;
            if (sqrDist <= sqrRange && sqrDist < bestSqr)
            {
                bestSqr = sqrDist;
                closest = enemy;
            }
        }

        return closest;
    }
}