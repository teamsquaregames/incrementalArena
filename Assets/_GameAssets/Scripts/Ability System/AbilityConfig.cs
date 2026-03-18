using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Ability", fileName = "NewAbility")]
public class AbilityConfig : ScriptableObject
{
    [Header("Identity")]
    public string abilityName;
    public Sprite icon;
    [TextArea] public string description;

    [Header("Targeting")]
    public float range = 2f;
    public float aoeRadius = 0f;

    [Header("Cooldown")]
    public float cooldown = 1f;

    [Header("Steps")]
    public List<AbilityStep> steps = new List<AbilityStep>();
}

public enum VFXPosition
{
    Caster,
    Target
}