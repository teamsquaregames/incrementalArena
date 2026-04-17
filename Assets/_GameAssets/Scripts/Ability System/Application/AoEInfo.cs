using UnityEngine;
using Sirenix.OdinInspector;
using Stats;


[System.Serializable]
public class AoEInfo
{
    #region MEMBERS
    public enum Shape
    {
        Circle,
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
    [ShowIf("@this.effectShape == Shape.Circle")]
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
    public Direction effectDirection;
    [EnumToggleButtons]
    public Position effectPosition;

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
    public static AoEInfo AOEFromProjectileTrajectory(float _range, float _width, float _frequency)
    {
        AoEInfo result = new();
        result.effectShape = Shape.Rect;
        // result.range = new() { value = _range };
        // result.width = _width;
        result.effectDirection = Direction.Alined;
        result.effectPosition = Position.Mid;
        result.time = Time.OverTime;
        result.frequency = _frequency;

        //FX constructor

        return result;
    }

    public float Radius(AbilityConfig ability = null)
    {
        return StatManager.Instance.GetDefinitionStat(ability, StatType.Radius).GetSpecificModifierValue(ModifierType.Percentage) * radius / 100f + radius;
    }
    #endregion
}