using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using Utils;

[CreateAssetMenu(menuName = "Config/GameConfig")]
public class GameConfig : ScriptableObject
{
    private static GameConfig _instance;
    public static GameConfig Instance => _instance ?? Load();

    private static GameConfig Load()
    {
        _instance = Resources.Load<GameConfig>("GameConfig");
#if UNITY_EDITOR
        if (_instance == null)
            UnityEngine.Debug.LogError("GameConfig asset not found in Resources folder!");
#endif
        return _instance;
    }

    //-------------------------------------

    public DebuggingSettings debuggingSettings = new DebuggingSettings();
    public CheatSettings cheatSettings = new CheatSettings();
    public GameSettings gameSettings = new GameSettings();

    //-------------------------------------

    [System.Serializable]
    public class CheatSettings
    {
        public bool preventSave;
        public bool startResetData;
        public bool noFTUE;
        public bool noCurrencyRequired;
        public bool noMenu;
        public bool disableBootStrapper;
        public bool infiniteRunDuration;
        public bool usePrefabAbilities;
        public bool usePrefabEnemies;
    }

    [System.Serializable]
    public class DebuggingSettings
    {
        public bool developmentBuild;
        public bool noMusic;
    }

    [System.Serializable]
    public class GameSettings
    {
        public bool isDemo = false;

        [Space, Header("Tutorial")]
        public float delayBeforeCanValidateOnClick = 0.2f;

        public List<Entity> defaultEnemies;
    }
}