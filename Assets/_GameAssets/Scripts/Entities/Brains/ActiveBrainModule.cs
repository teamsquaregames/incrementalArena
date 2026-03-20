using MyBox;
using UnityEngine;
using UnityEngine.InputSystem;
using Utils;

/// <summary>
/// Active brain — the "action game" feel. Nothing is automatic; the player
/// explicitly controls everything:
///
///   Movement    : WASD / arrow keys (flat 2-D input).
///   Aim         : Mouse world position (via CursorManager).
///   Auto-attack : Left mouse button — fires at MAX range in the aim direction
///                 so the character never has to walk into melee to attack.
///   Ability 0   : Q key.
///   Ability 1   : E key.
///
/// Notes
/// ─────
/// • Facing and movement are always processed, even during ability animations.
/// • Ability key presses (Q/E) still interrupt everything and return early —
///   but that only skips the auto-attack check, not facing/movement.
/// • While a non-auto ability is animating, only the auto-attack is suppressed.
/// • The auto-attack target point is placed exactly at AutoAttack.range along
///   the aim direction so that the AnimationEvent resolves hits at the right spot.
/// • Upper-body layer weight is set to 1 whenever any attack or ability is active
///   (IsBusy), and 0 otherwise.
/// </summary>
public class ActiveBrainModule : EntityBrainModule
{
    protected override void Think()
    {
        if (CursorManager.Instance == null) return;
        if (!Owner.TryGetModule(out EntityAbilityModule abilityModule)) return;

        Vector3 mouseWorld = CursorManager.Instance.MouseWorldPosition;

        // ── 0. Always face the mouse — never blocked ──────────────────────────
        FacePosition(mouseWorld);

        // ── 1. Always process movement — never blocked ────────────────────────
        Vector2 moveInput = Vector2.zero;
        if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)    moveInput.y += 1f;
        if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)  moveInput.y -= 1f;
        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) moveInput.x += 1f;
        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)  moveInput.x -= 1f;

        SetMoveInput(moveInput.sqrMagnitude > 0f ? moveInput.normalized : Vector2.zero);

        // ── 2. Upper-body layer — weight 1 during any attack or ability, 0 otherwise
        SetUpperBodyWeight(abilityModule.IsBusy ? 1f : 0f);

        // ── 3. Ability input — cancel auto-attack, then skip to next frame ────
        if (Keyboard.current.qKey.wasPressedThisFrame)
        {
            abilityModule.CancelEverything();
            TryUseAbility(0, mouseWorld.OffsetY(0.75f));
            return;
        }

        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            abilityModule.CancelEverything();
            TryUseAbility(1, mouseWorld.OffsetY(0.75f));
            return;
        }

        // ── 4. While a non-auto ability is animating, suppress auto-attack only ─
        if (abilityModule.IsUsingAbility) return;

        // ── 5. Auto-attack on left mouse button ───────────────────────────────
        if (Mouse.current.leftButton.wasPressedThisFrame && abilityModule.AutoAttack != null)
        {
            // Place the target at exactly AutoAttack.range in the aim direction
            // so the attack resolves at max range regardless of cursor distance.
            Vector3 aimDir          = (mouseWorld - Owner.transform.position).SetY(0).normalized;
            Vector3 attackTargetPos = Owner.transform.position + aimDir * abilityModule.AutoAttack.range;

            TryAutoAttack(attackTargetPos.OffsetY(0.75f));
        }
    }
}