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

    /// <summary>Try to fire an ability toward a world-space target position.</summary>
    protected bool TryUseAbility(AbilitySO ability, Vector3 targetPosition)
    {
        if (ability == null) return false;
        if (Owner.TryGetModule(out EntityAbilityModule abilityModule))
            return abilityModule.TryUseAbility(ability, targetPosition);
        return false;
    }

    /// <summary>Try to fire the owner's auto-attack toward a world-space target position.</summary>
    protected bool TryAutoAttack(Vector3 targetPosition)
    {
        if (Owner.TryGetModule(out EntityAbilityModule abilityModule))
            return TryUseAbility(abilityModule.AutoAttack, targetPosition);
        return false;
    }
}