using UnityEngine;
using Sirenix.OdinInspector;
using Stats;


[System.Serializable]
public class AbilityApplicationInfo
{
    public enum Type
    {
        Direct,
        Aoe,
        Projectile,
        Custom
    }

    [EnumToggleButtons]
    public Type effectZoneType;    
    public TeamApplication teamApplication = TeamApplication.Opponent;

    [ShowIf("@this.effectZoneType == Type.Projectile"), HideLabel(), InlineProperty()]
    public ProjectileInfo projectileInfo;

    [ShowIf("@this.effectZoneType == Type.Aoe || this.effectZoneType == Type.Projectile"), HideLabel(), InlineProperty()]
    //[FoldoutGroup("Effect Zone")]
    public AoEInfo aoeInfo;

    [Title("Other Settings")]
    public float delay = 0f;
    [SerializeField]
    private int repeatCount = 1;
    [ShowIf("@this.repeatCount > 1")]
    public float repeatDuration;
    [ShowIf("@this.repeatCount > 1")]
    public bool repeatFX;

    public bool excludeHitEntity = true;

    public int RepeatCount(AbilityConfig ability = null, int step = 0, int application = 0)
    {
        // Debug.Log($"Getting repeat count for ability '{ability?.abilityName}', step {step}, application {application}. Base repeat count: {repeatCount}. Return: {(int)StatManager.Instance.GetDefinitionStat(ability, StatType.Repetition, step, application).GetSpecificModifierValue(ModifierType.Flat) + repeatCount}");
        return (int)StatManager.Instance.GetDefinitionStat(ability, StatType.Repetition, step, application).GetSpecificModifierValue(ModifierType.Flat) + repeatCount;
    }
}
