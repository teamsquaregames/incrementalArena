using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Ability", fileName = "NewAbility")]
public class AbilityConfig : ScriptableObject
{
    [TitleGroup("Identity")]
    public string abilityName;
    public Sprite icon;
    [TextArea] public string description;

    [TitleGroup("General")]
    public Vector2 range = new Vector2(0f, 2f);
    public float cooldown = 1f;
    
    [TitleGroup("SFXs")]
    public SoundKeys sfx;
    public float sfxDelay;

    [TitleGroup("Steps")]
    public List<AbilityStep> steps = new List<AbilityStep>();
}

public enum VFXPosition
{
    Caster,
    Target
}