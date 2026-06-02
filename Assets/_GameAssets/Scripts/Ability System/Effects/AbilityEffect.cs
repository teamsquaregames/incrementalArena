using UnityEngine;

public abstract class AbilityEffect : ScriptableObject
{
    public abstract double Execute(AbilityContext ctx, Entity target);
}