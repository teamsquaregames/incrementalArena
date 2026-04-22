using UnityEngine;

public class AbilityContext
{
    public Entity caster;
    public EntityAbilityModule module;
    public Vector3 aimPosition;
    public AbilityConfig abilityConfig;
    public Entity closestEntity;
    public int currentStepIndex;
    public float value;
    public bool isCrit;
}