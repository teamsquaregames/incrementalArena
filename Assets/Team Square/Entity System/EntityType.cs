using System;
using System.Collections.Generic;
using UnityEngine;

[Flags]
public enum EntityType
{
    Player = 1 << 0,
    Orc = 1 << 1,
    Goblin = 1 << 3,
    Ogre = 1 << 5,
    OrcHeavy = 1 << 6,
    Troll = 1 << 7,
    Squeleton = 1 << 100,
    SqueletonWarrior = 1 << 101,
    Banshee = 1 << 102,
    Ghoul = 1 << 103,
    Horror = 1 << 104,
    Demon = 1 << 201,
    DarkKnight = 1 << 202,
    WingedDemon = 1 << 205,
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
                //Debug.Log($"EntityType {entityType} contains flag {value}");
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

