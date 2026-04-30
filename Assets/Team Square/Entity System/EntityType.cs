using System;
using System.Collections.Generic;

[Flags]
public enum EntityType
{
    Player = 1 << 0,
    Orc = 1 << 1,
    Demon = 1 << 2,
    Goblin = 1 << 3,
    WingedDemon = 1 << 4,
    Ogre = 1 << 5,
    OrcHeavy = 1 << 6,
    Squeleton = 1 << 7,
    SqueletonWarrior = 1 << 8,
}

public static class EntityTypeExtensions
{
    public static IEnumerable<EntityType> GetFlags(this EntityType entityType)
    {
        foreach (EntityType value in Enum.GetValues(typeof(EntityType)))
        {
            if (value == 0) continue;
            if (entityType.HasFlag(value))
                yield return value;
        }
    }

    // public static bool IsSingleFlag(EntityType entityType)
    // {
    //     return entityType != 0 && (entityType & (entityType - 1)) == 0;
    // }
}

