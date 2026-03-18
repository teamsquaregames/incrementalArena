using System;

[Flags]
public enum EntityType
{
    Player   = 1 << 0,
    Skeleton = 1 << 1,
    Boss     = 1 << 2,
}