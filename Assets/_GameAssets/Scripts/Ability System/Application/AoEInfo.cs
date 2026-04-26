using UnityEngine;
using Sirenix.OdinInspector;
using Stats;


[System.Serializable]
public class AoEInfo
{
    #region MEMBERS
    public enum Shape
    {
        Sphere,
        Rect,
        Cone
    }

    public enum Time
    {
        Instant,
        OverTime,
    }

    public enum Direction
    {
        None,
        Alined,
        Targeted
    }

    public enum Position
    {
        Start,
        Mid,
        End
    }
    [Title("Aoe Settings")]

    [EnumToggleButtons]
    public Shape effectShape;
    [ShowIf("@this.effectShape == Shape.Sphere")]
    [SerializeField] private float radius;
    [ShowIf("@this.effectShape == Shape.Rect")]
    public Vector3 scale;
    [ShowIf("@this.effectShape == Shape.Cone")]
    public float angle;
    [ShowIf("@this.effectShape == Shape.Cone")]
    public float range;
    public Vector3 offset;

    [Space]
    [EnumToggleButtons]
    public Time time;
    [ShowIfGroup("time", Condition = "@this.time == Time.OverTime")]
    public float duration; //Temps pendant lequel la zone fait des d�gats 
    [ShowIfGroup("time")]
    public float frequency; //Temps apr�s lequel les d�gats sont appliqu�s � r�p�tition 
    [ShowIfGroup("time")]
    public bool stackEffect;

    // public AbilityVFX AOEVisualEffect;
    #endregion


    #region Getters
    public float Radius(AbilityConfig ability, int step = 0, int application = 0)
    {
        return StatManager.Instance.GetDefinitionStat(ability, StatType.Size, step, application).GetSpecificModifierValue(ModifierType.Percentage) * radius / 100f + radius;
    }
    #endregion
}