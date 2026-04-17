using System.Collections.Generic;
using Sirenix.OdinInspector;
using Stats;
using UnityEngine;
using Utils;

[CreateAssetMenu(menuName = "Abilities/Ability", fileName = "NewAbility")]
public class AbilityConfig : ScriptableObject
{
    [TitleGroup("Identity")]
    public string abilityName;
    public Sprite icon;
    [TextArea] public string description;

    [TitleGroup("General")]
    [SerializeField] private Vector2 range = new Vector2(0f, 2f);
    [SerializeField] private float cooldown = 1f;
    
    [TitleGroup("SFXs")]
    public SoundKeys sfx;
    public float sfxDelay;

    [TitleGroup("Steps")]
    public List<AbilityStep> steps = new List<AbilityStep>();


    #region Getters
    public Vector2 Range(Entity owner = null)
    {
        Vector2 currentRange = range;
        currentRange.y = (StatManager.Instance.GetDefinitionStat(this, StatType.MaxRange).GetSpecificModifierValue(ModifierType.Percentage) / 100f + 1) * range.y;
        return currentRange;
    }

    public float Cooldown()
    {
        // this.Log($"Calculating cooldown for ability '{abilityName}'. Base cooldown: {cooldown}, cooldown modifiers: {StatManager.Instance.GetDefinitionStat(this, StatType.CD).GetSpecificModifierValue(ModifierType.Percentage)}% = {cooldown - (StatManager.Instance.GetDefinitionStat(this, StatType.CD).GetSpecificModifierValue(ModifierType.Percentage) / 100f * cooldown)} ");
        return cooldown - (StatManager.Instance.GetDefinitionStat(this, StatType.CD).GetSpecificModifierValue(ModifierType.Percentage) / 100f * cooldown);
    }
    #endregion
}

public enum VFXPosition
{
    Caster,
    Target
}