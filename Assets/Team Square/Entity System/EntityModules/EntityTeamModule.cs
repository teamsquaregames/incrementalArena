using UnityEngine;

public enum Team
{
    Player = 0,
    Enemy = 1
}

public class EntityTeamModule : EntityModule
{
    [SerializeField] private Team team;
    [SerializeField] private Team enemyTeam;

    public Team Team => team;
    public Team EnemyTeam => enemyTeam;
}