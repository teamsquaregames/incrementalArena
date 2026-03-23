using System;

[Flags]
public enum EntityType
{
    Player   = 1 << 0,
    Orc = 1 << 1,
}