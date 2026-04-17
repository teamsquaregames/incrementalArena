using UnityEngine;
using Sirenix.OdinInspector;


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

    [ShowIf("@this.effectZoneType == Type.Projectile"), HideLabel(), InlineProperty()]
    public ProjectileInfo projectileInfo;

    [ShowIf("@this.effectZoneType == Type.Aoe || this.effectZoneType == Type.Projectile"), HideLabel(), InlineProperty()]
    //[FoldoutGroup("Effect Zone")]
    public AoEInfo aoeInfo;

    [Title("Other Settings")]
    public float delay = 0f;
    public int repeatCount = 1;
    [ShowIf("@this.repeatCount > 1")]
    public float repeatDuration;
    [ShowIf("@this.repeatCount > 1")]
    public bool repeatFX;

    public bool excludeHitEntity = true;
}
