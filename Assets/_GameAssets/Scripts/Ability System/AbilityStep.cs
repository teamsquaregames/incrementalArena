using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class AbilityStep
{
    [Header("VFXs")]
    public ParticleSystem mainVfx;
    public VFXPosition mainVFXPosition;
    public ParticleSystem hitVfx;

    [Header("Animation")]
    public AnimationClip abilityClip;
    
    [Header("Effects")]
    public List<AbilityEffectEntry> effects = new List<AbilityEffectEntry>();
}