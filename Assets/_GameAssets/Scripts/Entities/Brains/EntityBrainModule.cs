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
    
    protected abstract void Think();
    
    protected void SetMoveInput(Vector2 input)
    {
        if (Owner.TryGetModule(out EntityMovementModule movementModule))
        {
            movementModule.SetMoveInput(input);
        }
    }
    
    protected void StopMovement() => SetMoveInput(Vector2.zero);
    
    protected void FacePosition(Vector3 worldPosition)
    {
        Vector3 dir = (worldPosition - Owner.transform.position).SetY(0f);
        if (dir.sqrMagnitude < 0.001f) return;
        Owner.transform.rotation = Quaternion.LookRotation(dir);
    }
    
    protected bool TryUseAbility(int abilityIndex, Vector3 aimPosition)
    {
        if (abilityIndex < 0) return false;
        
        if (Owner.TryGetModule(out EntityAbilityModule abilityModule))
        {
            if (abilityIndex > abilityModule.Abilities.Count - 1) return false;
            return abilityModule.TryUseAbility(abilityModule.Abilities[abilityIndex], aimPosition);
        }

        return false;
    }
    
    protected bool TryAutoAttack(Vector3 aimPosition)
    {
        if (Owner.TryGetModule(out EntityAbilityModule abilityModule))
            return abilityModule.TryUseAutoAttack(aimPosition);
        return false;
    }
    
    protected void SetUpperBodyWeight(float target)
    {
        if (Owner.TryGetModule(out OmniDirectionalMovementAnimation animModule))
            animModule.SetUpperBodyWeight(target);
    }
}