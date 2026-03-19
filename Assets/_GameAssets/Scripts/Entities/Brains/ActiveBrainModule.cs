using MyBox;
using UnityEngine;
using UnityEngine.InputSystem;
using Utils;

/// <summary>
/// Active brain — the "action game" feel. Nothing is automatic; the player
/// explicitly controls everything:
///
///   Movement  : WASD / arrow keys (flat 2-D input).
///   Aim       : Mouse world position (via CursorManager).
///   Auto-attack : Left mouse button — fires at MAX range in the aim direction
///                 so the character never has to walk into melee to attack.
///   Ability 0 : Q key.
///   Ability 1 : E key.
///
/// Notes
/// ─────
/// • Abilities interrupt an ongoing auto-attack (same as the original PlayerBrainModule).
/// • While a non-auto ability is animating, movement and attacks are suppressed.
/// • The auto-attack target point is placed exactly at AutoAttack.range along
///   the aim direction so that the AnimationEvent resolves hits at the right spot.
/// </summary>
public class ActiveBrainModule : EntityBrainModule
{
    protected override void Think()
    {
        if (CursorManager.Instance == null) return;
        if (!Owner.TryGetModule(out EntityAbilityModule abilityModule)) return;

        Vector3 mouseWorld = CursorManager.Instance.MouseWorldPosition;

        // ── 1. Ability input — highest priority, cancel any auto-attack ───────
        if (Keyboard.current.qKey.wasPressedThisFrame)
        {
            abilityModule.CancelAbility();
            TryUseAbility(0, mouseWorld.OffsetY(0.75f));
            return;
        }

        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            abilityModule.CancelAbility();
            TryUseAbility(1, mouseWorld.OffsetY(0.75f));
            return;
        }

        // ── 2. While a non-auto ability is animating, block everything else ───
        if (abilityModule.IsUsingAbility) return;

        // ── 3. Movement — raw WASD/arrow key input ────────────────────────────
        Vector2 moveInput = Vector2.zero;
        if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)    moveInput.y += 1f;
        if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)  moveInput.y -= 1f;
        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) moveInput.x += 1f;
        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)  moveInput.x -= 1f;

        SetMoveInput(moveInput.sqrMagnitude > 0f ? moveInput.normalized : Vector2.zero);

        // ── 4. Auto-attack on left mouse button ───────────────────────────────
        if (Mouse.current.leftButton.wasPressedThisFrame && abilityModule.AutoAttack != null)
        {
            // Place the target at exactly AutoAttack.range in the aim direction
            // so the attack resolves at max range regardless of cursor distance.
            Vector3 aimDir = (mouseWorld - Owner.transform.position).SetY(0).normalized;
            Vector3 attackTargetPos = Owner.transform.position
                                      + aimDir * abilityModule.AutoAttack.range;

            TryAutoAttack(attackTargetPos.OffsetY(0.75f));
        }
    }
}