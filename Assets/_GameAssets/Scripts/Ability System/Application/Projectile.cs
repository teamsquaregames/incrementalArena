using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Lean.Pool;

public class Projectile : MonoBehaviour
{
    public int instanceID;
    private bool m_excludeHitEntity;

    private EntityAbilityModule m_abilityModule;
    private ProjectileInfo m_projectileInfo;
    private AbilityContext m_abilityCtz;
    private AbilityApplicationInfo m_applicationInfo;
    private List<AbilityEffectEntry> m_abilityEffects;

    private Vector3 m_originPosition;
    private Vector3 m_finalPosition;
    private float m_duration;
    private float m_distance;
    private float m_width;

    private bool m_isTraveling;
    private Vector3 m_lastPosition;
    private Tweener m_tweener;
    private List<Entity> m_hitEntities = new();

    private ParticleSystem[] m_vfxs;

    public void Spawn(AbilityContext _abilityCtx, AbilityApplicationInfo _abilityAppInfo, int _instanceIndex)
    {
        m_abilityCtz = _abilityCtx;
        m_applicationInfo = _abilityAppInfo;
        m_projectileInfo = _abilityAppInfo.projectileInfo;
        m_excludeHitEntity = _abilityAppInfo.excludeHitEntity;
        m_abilityModule = _abilityCtx.module;
        m_abilityEffects = _abilityCtx.abilityConfig.steps[_instanceIndex].effects;
        instanceID = _instanceIndex;

        m_hitEntities.Clear();

        // m_width = m_projectileInfo.width.ComputeValue(m_ability.abilityInstanceData.currentSkills);

        InitTransform();
        InitVisual();
        InterpolateTrajectory();
    }

    public void OnDespawn()
    {
        gameObject.SetActive(false);
        transform.localScale = Vector3.one;
        transform.position = Vector3.zero;
        m_isTraveling = false;

        foreach (ParticleSystem ps in m_vfxs)
        {
            // ps.Clear(true);
            // ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        m_tweener?.Kill();
    }

    private void InitTransform()
    {
        switch (m_projectileInfo.origin)
        {
            case ProjectileInfo.Origin.Caster:
                m_originPosition = m_abilityCtz.caster.transform.position;
                break;
            case ProjectileInfo.Origin.Target:
                m_originPosition = m_abilityCtz.closestEntity != null ? m_abilityCtz.closestEntity.transform.position : m_abilityCtz.aimPosition;
                break;
        }

        if (m_projectileInfo.origin == ProjectileInfo.Origin.Target)
        {
            m_originPosition = m_abilityCtz.closestEntity != null ? m_abilityCtz.closestEntity.transform.position : m_abilityCtz.aimPosition;
        }

        Vector3 dirTowardAimedPosition = m_abilityCtz.aimPosition - m_abilityCtz.caster.transform.position;
        Quaternion rot = dirTowardAimedPosition == Vector3.zero ? Quaternion.identity : Quaternion.LookRotation(dirTowardAimedPosition, Vector3.up);

        // We apply the offset in local space to make sure the projectile spawns at the correct position relative to the caster, even if the caster is rotated
        Matrix4x4 casterEntityLocalMatrix = Matrix4x4.TRS(m_abilityCtz.caster.transform.localPosition, rot, m_abilityCtz.caster.transform.localScale);
        m_originPosition += casterEntityLocalMatrix.MultiplyVector(m_projectileInfo.startPositionOffset + (m_projectileInfo.autoOffsetWithWidth ? (m_width * 0.5f * Vector3.forward) : Vector3.zero));

        Vector3 direction = m_abilityCtz.caster.transform.forward;

        if (m_projectileInfo.spreadAngle > 0)
        {
            if (m_applicationInfo.repeatCount > 1 && !m_projectileInfo.randomRepartition)
            {
                float angle = Mathf.Lerp(-m_projectileInfo.spreadAngle / 2, m_projectileInfo.spreadAngle / 2, (float)instanceID / (m_applicationInfo.repeatCount - 1));
                direction = Quaternion.AngleAxis(angle, Vector3.up) * direction;
            }
            else
            {
                direction = Quaternion.AngleAxis(Random.Range(-m_projectileInfo.spreadAngle / 2, m_projectileInfo.spreadAngle / 2), Vector3.up) * direction;
            }
        }

        switch (m_projectileInfo.type)
        {
            case ProjectileInfo.Type.Directional:
                m_finalPosition = m_originPosition + (direction * m_projectileInfo.distance);
                m_distance = Vector3.Distance(m_originPosition, m_finalPosition);
                m_duration = m_distance / m_projectileInfo.speed;
                break;
                // case ProjectileInfo.Type.Targeted:
                //     m_finalPosition = effectZoneInstance.aimedPosition;
                //     m_distance = Vector3.Distance(m_originPosition, m_finalPosition);
                //     m_duration = m_distance / m_projectileInfo.speed;
                //     break;
                // case ProjectileInfo.Type.Other:
                //     m_duration = m_projectileInfo.lifeTime;
                //     break;
        }

        transform.position = m_originPosition;
        transform.forward = direction;
    }

    public void InitVisual()
    {
        transform.localScale = m_width * Vector3.one;

        if (m_vfxs == null)
        {
            m_vfxs = GetComponentsInChildren<ParticleSystem>();
        }

        foreach (ParticleSystem ps in m_vfxs)
        {
            // ps.Clear(true);
            ps.Play();
        }
    }

    private void ClearChildren()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);
            if (Application.isPlaying)
                Destroy(child.gameObject);
            else
                DestroyImmediate(child.gameObject);
        }

        m_vfxs = null;
    }

    protected virtual void InterpolateTrajectory()
    {
        m_isTraveling = true;
        m_tweener?.Kill();
        m_tweener = DOVirtual.Float(0f, 1f, m_duration, OnTrajectroryInterpolation).SetEase(Ease.Linear).OnComplete(OnTravelEnd).SetTarget(transform);
    }

    private void OnTrajectroryInterpolation(float _t)
    {
        m_lastPosition = transform.position;

        switch (m_projectileInfo.type)
        {
            case ProjectileInfo.Type.Directional:
                transform.position = Vector3.Lerp(m_originPosition, m_finalPosition, _t);
                break;
                // case ProjectileInfo.Type.Targeted:
                //     if (effectZoneInstance.targetEntity != null)
                //         m_finalPosition = effectZoneInstance.targetEntity.transform.position;
                //     transform.position = Vector3.Lerp(m_originPosition, m_finalPosition, _t);
                //     break;
                // case ProjectileInfo.Type.Other:
                //     transform.position = m_projectileInfo.projectileBehavior.GetNextProjectilePosition(this, m_projectileInfo);
                //     break;
        }


        if (m_projectileInfo.projectileTrajectory == ProjectileInfo.ProjectileTrajectory.Curve)
        {
            float offsetYMultiplier = m_projectileInfo.curveTopHeight * Mathf.Clamp01(m_distance / 15f);
            transform.position += Vector3.up * offsetYMultiplier * m_projectileInfo.trajectoryCurve.Evaluate(_t);
        }

        if (m_projectileInfo.applyEffectThroughTrajectory)
        {
            List<Entity> hitEntities = ResolveApplication.AoeSwitch(m_applicationInfo, transform.position, m_abilityCtz);

            foreach (Entity e in hitEntities)
            {
                if (!m_hitEntities.Contains(e))
                {
                    m_abilityModule.HandleEffects(m_abilityCtz, e, m_abilityEffects);
                    m_hitEntities.Add(e);
                }
            }
        }

        if (m_projectileInfo.piercingAmount > 0 && m_hitEntities.Count >= m_projectileInfo.piercingAmount)
        {
            OnTravelEnd();
        }
    }

    private void OnTravelEnd()
    {
        if (!m_isTraveling)
            return;

        LeanPool.Despawn(this);
    }
}