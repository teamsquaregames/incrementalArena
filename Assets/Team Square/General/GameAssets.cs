using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using Utils;
using Stats;

[CreateAssetMenu(menuName = "Config/GameAssets")]
public class GameAssets : ScriptableObject
{
    private static GameAssets _instance;
    public static GameAssets Instance => _instance ?? Load();

    private static GameAssets Load()
    {
        _instance = Resources.Load<GameAssets>("GameAssets");
        return _instance;
    }


    // ----------------------------------------------------------

    [AssetList(Path = "Team Square/Currency/Objects", AutoPopulate = true)]
    public CurrencyAsset[] currencyAssets;
    public FloatingTextConfig critDamageTextConfig;
    public List<ArenaConfig> arenaConfigs;

    public SerializableDictionary<string, Entity> enemies;
    [AssetList(Path = "_GameAssets/Objects/Abilities", AutoPopulate = true)]
    public List<AbilityConfig> abilities;

    public Entity GetEnemy(string id)
    {
        return enemies != null && enemies.TryGetValue(id, out var prefab) ? prefab : null;
    }

    public AbilityConfig GetAbility(string id)
    {
        return abilities?.Find(a => a.id == id);
    }
}   