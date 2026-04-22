using UnityEngine;
using Sirenix.OdinInspector;

[System.Serializable]
public class ProjectileInfo
{
    public Projectile prefab;
    [Title("Travel Settings")]
    [EnumToggleButtons]
    public Type type;
    public float speed = 1f;
    [ShowIf("@this.type == Type.Directional")]
    public float distance = 10f;
    [ShowIf("@this.type == Type.Other")]
    public float lifeTime;
    [ShowIf("@this.type == Type.Other")]
    // public AProjectileStandaloneBehavior projectileBehavior;

    [ShowIf("@this.type == Type.Directional"), ShowInInspector, Tooltip("Range / Speed")]
    public float travelTime => distance / speed;

    [HideIf("@this.type == Type.Other")]
    public ProjectileTrajectory projectileTrajectory;

    [ShowIf("@this.projectileTrajectory == ProjectileTrajectory.Curve")]
    public float curveTopHeight;
    [ShowIf("@this.projectileTrajectory == ProjectileTrajectory.Curve")]
    public AnimationCurve trajectoryCurve;

    public Origin origin;
    public Vector3 startPositionOffset;
    public bool autoOffsetWithWidth;
    [ShowIf("@this.type == Type.Directional"), Range(0f, 360f)]
    public float spreadAngle;
    [ShowIf("@this.type == Type.Directional")]
    public bool randomRepartition;

    [Title("Effect Application Settings")]
    public bool applyEffectThroughTrajectory;
    [ShowIf("applyEffectThroughTrajectory")] public float frequency;
    [ShowIf("applyEffectThroughTrajectory")]
    public int piercingAmount;
    // public bool computeAoeOnComplete;
    // [ShowIf("computeAoeOnComplete")] public AoEInfo onCompleteAoe;
    // public bool computeAbilityOnComplete;
    // [ShowIf("computeAbilityOnComplete")] public AbilityInfo onCompleteRecursiveAbility;

    //[FoldoutGroup("Projectile FX Settings")] [InlineProperty] public FX.FXModifier fxModifier;

    public enum ProjectileTrajectory
    {
        Linear,
        Curve
    }

    public enum Type
    {
        Directional,
        Missile,
        Other,
    }

    public enum Origin
    {
        Caster,
        Target,
    }
}