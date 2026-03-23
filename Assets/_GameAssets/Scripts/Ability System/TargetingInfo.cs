using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;


[System.Serializable]
public class TargetingInfo
{
    [Tooltip("If Quick Target is set to none, will take in account the Target")]
    [EnumToggleButtons]
    public QuickTarget quickTarget;
    [Space]
    [ShowIf("@quickTarget == QuickTarget.None")]
    public Target target;
    [ShowIf("@quickTarget == QuickTarget.None")]
    public float delayInterval;
    [ShowIf("@quickTarget == QuickTarget.None")]
    public bool retriggerAnimation = false;

    //public AdditionalOffset additionalOffset;
    //[ShowIf("@additionalOffset != AdditionalOffset.None"), MinMaxSlider(-10, 10, true)]
    //public Vector2 minMaxOffset;
    public enum QuickTarget
    {
        None,
        Self,
        Current,
        Cursor,
    }

    public enum Target
    {
        Closest,
        Random,
        All,
    }
}