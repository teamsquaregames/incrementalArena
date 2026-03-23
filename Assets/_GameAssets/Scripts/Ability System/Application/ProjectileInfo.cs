using UnityEngine;
using Sirenix.OdinInspector;

[System.Serializable]
public class ProjectileInfo
{
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
    [ShowIf("@this.type == Type.Directional")]
    public float spreadAngle;

    [Title("Effect Application Settings")]
    public bool applyEffectThroughTrajectory;
    // [ShowIf("applyEffectThroughTrajectory")] public SkillScaling width;
    [ShowIf("applyEffectThroughTrajectory")] public float frequency;
    public bool destroyOnHit;
    [ShowIf("destroyOnHit")]
    // public SkillScaling piercingAmount;
    // public bool computeAoeOnComplete;
    // [ShowIf("computeAoeOnComplete")] public AoEInfo onCompleteAoe;
    // public bool computeAbilityOnComplete;
    // [ShowIf("computeAbilityOnComplete")] public AbilityInfo onCompleteRecursiveAbility;

    [FoldoutGroup("Projectile FX Settings")] public GameObject projectileVisualEffect;
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