using MyBox;
using UnityEngine;

/// <summary>
/// Abstract base for all entity brains. Subclasses implement Think() to issue
/// orders to movement and ability modules — those modules never act on their own.
/// </summary>
public abstract class EntityBrainModule : EntityModule
{
    private void Update()
    {
        Think();
    }

    /// <summary>Override to define this entity's decision-making each frame.</summary>
    protected abstract void Think();

    // ─── Protected helpers ────────────────────────────────────────────────────

    /// <summary>Set the flat 2-D move input on the owner's movement module (if present).</summary>
    protected void SetMoveInput(Vector2 input)
    {
        if (Owner.TryGetModule(out EntityMovementModule movementModule))
            movementModule.SetMoveInput(input);
    }

    /// <summary>Stop the owner in place.</summary>
    protected void StopMovement() => SetMoveInput(Vector2.zero);

    /// <summary>
    /// Instantly rotate the owner to face a world-space position (flat, Y-ignored).
    /// Does nothing if the target is essentially on top of the owner.
    /// </summary>
    protected void FacePosition(Vector3 worldPosition)
    {
        Vector3 dir = (worldPosition - Owner.transform.position).SetY(0f);
        if (dir.sqrMagnitude < 0.001f) return;
        Owner.transform.rotation = Quaternion.LookRotation(dir);
    }

    /// <summary>Try to fire an ability toward a world-space target position.</summary>
    protected bool TryUseAbility(int abilityIndex, Vector3 targetPosition)
    {
        if (abilityIndex < 0) return false;
        
        if (Owner.TryGetModule(out EntityAbilityModule abilityModule))
        {
            if (abilityIndex > abilityModule.Abilities.Count - 1) return false;
            return abilityModule.TryUseAbility(abilityModule.Abilities[abilityIndex], targetPosition);
        }

        return false;
    }

    /// <summary>Try to fire the owner's auto-attack toward a world-space target position.</summary>
    protected bool TryAutoAttack(Vector3 targetPosition)
    {
        if (Owner.TryGetModule(out EntityAbilityModule abilityModule))
            return abilityModule.TryUseAutoAttack(targetPosition);
        return false;
    }

    /// <summary>
    /// Drives the upper-body animator layer weight via <see cref="OmniDirectionalMovementAnimation"/>.
    /// Pass 1f when attacking or casting, 0f otherwise.
    /// Does nothing if the owner has no <see cref="OmniDirectionalMovementAnimation"/>.
    /// </summary>
    protected void SetUpperBodyWeight(float target)
    {
        if (Owner.TryGetModule(out OmniDirectionalMovementAnimation animModule))
            animModule.SetUpperBodyWeight(target);
    }
}