using Sirenix.OdinInspector;
using UnityEditor.ShaderGraph.Internal;
using System;

[Flags]
public enum TeamApplication
{
    None = 0,
    Opponent = 1 << 0,   // 1
    Allies = 1 << 1,   // 2
    
    // Combos pratiques (optionnel)
    AllOptions = Opponent | Allies
}

[System.Serializable]
public class AbilityEffectEntry
{
    public AbilityEffect effect;
    public TeamApplication teamApplication = TeamApplication.Opponent;
    public float value;
}