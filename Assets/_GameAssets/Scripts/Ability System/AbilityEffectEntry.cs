using Sirenix.OdinInspector;
using UnityEditor.ShaderGraph.Internal;
using System;

[EnumToggleButtons]
public enum TeamApplication
{
    Opponent = 0,
    Allies = 1,
}

[System.Serializable]
public class AbilityEffectEntry
{
    public AbilityEffect effect;
    public float value;
}