using System;
using System.Collections.Generic;
using UnityEngine;

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
    Troll = 1 << 7,
    Squeleton = 1 << 8,
    SqueletonWarrior = 1 << 9,
}

public static class EntityTypeExtensions
{
    public static List<EntityType> GetFlags(this EntityType entityType)
    {
        List<EntityType> flags = new List<EntityType>();
        foreach (EntityType value in Enum.GetValues(typeof(EntityType)))
        {
            if ((entityType & value) == value)
            {
                Debug.Log($"EntityType {entityType} contains flag {value}");
                flags.Add(value);
            }
        }
        return flags;
    }

    // public static bool IsSingleFlag(EntityType entityType)
    // {
    //     return entityType != 0 && (entityType & (entityType - 1)) == 0;
    // }
}

