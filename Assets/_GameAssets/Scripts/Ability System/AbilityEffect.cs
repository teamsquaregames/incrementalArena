using UnityEngine;

public abstract class AbilityEffect : ScriptableObject
{
    public abstract void Execute(AbilityContext context);
}