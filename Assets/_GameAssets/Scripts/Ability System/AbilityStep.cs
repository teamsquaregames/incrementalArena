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
    public List<AbilityApplicationInfo> applicationInfos = new List<AbilityApplicationInfo>();

    [TitleGroup("VFXs")]
    public ParticleSystem mainVfx;
    public VisualEffect mainVfxGraph;
    public VFXPosition mainVFXPosition;
    public ParticleSystem hitVfx;
    public VisualEffect hitVfxGraph;

    [TitleGroup("SFXs")]
    public SoundKeys activeSfx;
    
    [TitleGroup("Animation")]
    public AnimationClip abilityClip;
    
    [TitleGroup("Effects")]
    public List<AbilityEffectEntry> effects = new List<AbilityEffectEntry>();
}