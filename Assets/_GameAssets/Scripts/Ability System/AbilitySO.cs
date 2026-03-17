using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Abilities/Ability", fileName = "NewAbility")]
public class AbilitySO : ScriptableObject
{
    [Header("Identity")]
    public string abilityName;
    public Sprite icon;
    [TextArea] public string description;
    public ParticleSystem vfx;

    [Header("Animation")]
    public string animatorBoolName;

    [Header("Targeting")]
    public float range = 2f;
    public float aoeRadius = 0f;

    [Header("Cooldown")]
    public float cooldown = 1f;

    [Header("Effects")]
    public List<AbilityEffectEntry> effects = new();
}