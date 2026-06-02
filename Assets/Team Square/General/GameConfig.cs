using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEditor;
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
        public bool playerImmortality;
        public bool npcImmortality;
        public bool oneHitKill;
        public bool usePrefabAbilities;
        public bool usePrefabEnemies;
    }

    [System.Serializable]
    public class DebuggingSettings
    {
        public bool developmentBuild;
        public bool noMusic;
        public bool customTargetFrameRate;
        [Sirenix.OdinInspector.ShowIf("customTargetFrameRate"), Range(10, 144)] public int targetFrameRate = 144;
    }

    [System.Serializable]
    public class GameSettings
    {
        public bool isDemo = false;

        [Title("Tutorial")]
        public float delayBeforeCanValidateOnClick = 0.2f;

        public List<Entity> defaultEnemies;
        [Title("Run Reset")]
        public List<TrackedValueType> trackedValuesToResetOnRunEnd = new List<TrackedValueType>();

        [Title("Nodes")]
        public float nodeScalingBase = 10f;
        public float nodeScalingLinear = 10f;
        public float nodeScalingExponent = 1.5f;
        public float GetNodeValue(int tier)
        {
            return Mathf.Round(
                nodeScalingBase * Mathf.Pow(nodeScalingExponent, tier) +
                nodeScalingLinear * tier
            );
        }
        [Button]
        public static void UpdateAllNodeCosts()
        {
            string[] guids = AssetDatabase.FindAssets("t:STNodeAsset");

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                STNodeAsset asset = AssetDatabase.LoadAssetAtPath<STNodeAsset>(path);

                if (asset != null)
                {
                    asset.UpdateCosts();
                    EditorUtility.SetDirty(asset);
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"Updated {guids.Length} STNodeAssets.");
        }
    }
    
}