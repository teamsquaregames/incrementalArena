using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.VFX;

[Serializable]
public class AbilityStep
{
    [TitleGroup("Behaviour")]
    public bool isRooting;
    public TargetingInfo targetingInfo;
    [PropertySpace(SpaceAfter = 10)]
    public List<AbilityApplicationInfo> applicationInfos = new List<AbilityApplicationInfo>();

    [FoldoutGroup("Visuals")]
    [Title("VFXs")]
    public ParticleSystem mainVfx;
    [FoldoutGroup("Visuals")]
    public VisualEffect mainVfxGraph;
    [FoldoutGroup("Visuals")]
    public VFXPosition mainVFXPosition;
    [FoldoutGroup("Visuals")]
    public Vector3 mainVFXOffset;
    [FoldoutGroup("Visuals")]
    public ParticleSystem hitVfx;
    [FoldoutGroup("Visuals")]
    public VisualEffect hitVfxGraph;

    [FoldoutGroup("Visuals")]
    [Title("SFXs")]
    public SoundKeys activeSfx;
    
    [FoldoutGroup("Visuals")]
    [Title("Animation")]
    public AnimationClip abilityClip;
    
    [TitleGroup("Effects")]
    [TableList]
    public List<AbilityEffectEntry> effects = new List<AbilityEffectEntry>();
}