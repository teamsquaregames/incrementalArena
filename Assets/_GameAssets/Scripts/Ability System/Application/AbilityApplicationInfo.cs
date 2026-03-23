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

    [ShowIf("@this.effectZoneType == Type.Aoe"), HideLabel(), InlineProperty()]
    //[FoldoutGroup("Effect Zone")]
    public AoEInfo aoeInfo;

    [Title("Other Settings")]
    public float delay = 0f;
    public int repeatCount;
    [ShowIf("@this.repeatCount > 1")]
    public float repeatDuration;

    [ShowIf("@this.effectZoneType != Type.Direct")]
    public float additionalRotation;
    [ShowIf("@this.effectZoneType != Type.Direct")]
    public Vector3 additionalPosition;
    public bool excludeHitEntity = true;
}
