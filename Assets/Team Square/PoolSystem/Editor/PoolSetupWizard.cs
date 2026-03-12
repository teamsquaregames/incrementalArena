#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

public class PoolSetupWizard : EditorWindow
{
    // ── Fixed paths ───────────────────────────────────────────────────────
    private const string kImplementationsPath =
        "Assets/Team Square/PoolSystem/Implementations";
    private const string kParentPrefabPath =
        "Assets/Team Square/PoolSystem/PoolsParent.prefab";

    // ── Pending state keys (persisted across recompile) ───────────────────
    private const string kPendingType = "PoolWizard_PendingType";
    private const string kPendingDir  = "PoolWizard_PendingDir";

    // ── Section 1 state ───────────────────────────────────────────────────
    private MonoScript _newTypeScript;
    private string     _newTypeName = "";

    // ── Section 2 state ───────────────────────────────────────────────────
    private MonoScript _poolTypeScript;
    private string     _poolTypeName = "";
    private GameObject _pooledPrefab;
    private string     _poolRefName  = "";

    // ─────────────────────────────────────────────────────────────────────
    [MenuItem("Tools/Pool Setup Wizard")]
    public static void Open() =>
        GetWindow<PoolSetupWizard>("Pool Setup Wizard").minSize = new Vector2(400, 440);

    // ── Phase 2 : runs after every recompile ──────────────────────────────
    [UnityEditor.Callbacks.DidReloadScripts]
    private static void OnScriptsReloaded()
    {
        string pendingType = EditorPrefs.GetString(kPendingType, "");
        if (string.IsNullOrEmpty(pendingType)) return;

        string dir = EditorPrefs.GetString(kPendingDir, "");
        ClearPending();
        CreatePoolTypePrefab(pendingType, dir);
    }

    // ── GUI ───────────────────────────────────────────────────────────────
    private void OnGUI()
    {
        // ── Section 1 : Create new pool type ─────────────────────────────
        GUILayout.Label("Create New Pool Type", EditorStyles.boldLabel);
        DrawSeparator();

        EditorGUI.BeginChangeCheck();
        _newTypeScript = (MonoScript)EditorGUILayout.ObjectField(
            "Component Script", _newTypeScript, typeof(MonoScript), false);
        if (EditorGUI.EndChangeCheck())
            _newTypeName = _newTypeScript != null
                ? _newTypeScript.GetClass()?.Name ?? "" : "";

        if (_newTypeScript != null && string.IsNullOrEmpty(_newTypeName))
            EditorGUILayout.HelpBox("Could not resolve class name from this script.", MessageType.Warning);

        string d1 = string.IsNullOrEmpty(_newTypeName) ? "<Type>" : _newTypeName;

        EditorGUILayout.Space(4);
        EditorGUI.BeginDisabledGroup(string.IsNullOrWhiteSpace(_newTypeName));
        if (GUILayout.Button("Generate Pool Type", GUILayout.Height(28)))
            RunPhaseOne(_newTypeName);
        EditorGUI.EndDisabledGroup();

        // ── Section 2 : Create new pool instance ─────────────────────────
        EditorGUILayout.Space(12);
        GUILayout.Label("Create New Pool", EditorStyles.boldLabel);
        DrawSeparator();

        EditorGUI.BeginChangeCheck();
        _poolTypeScript = (MonoScript)EditorGUILayout.ObjectField(
            "Pool Type Script", _poolTypeScript, typeof(MonoScript), false);
        if (EditorGUI.EndChangeCheck())
        {
            _poolTypeName = _poolTypeScript != null
                ? _poolTypeScript.GetClass()?.Name ?? "" : "";
            if (_poolTypeName.EndsWith("PoolRefSetter"))
                _poolTypeName = _poolTypeName.Replace("PoolRefSetter", "");
            else if (_poolTypeName.EndsWith("PoolRef"))
                _poolTypeName = _poolTypeName.Replace("PoolRef", "");

            if (string.IsNullOrEmpty(_poolRefName))
                _poolRefName = $"{_poolTypeName}PoolRef";
        }

        _pooledPrefab = (GameObject)EditorGUILayout.ObjectField(
            "Prefab to Pool", _pooledPrefab, typeof(GameObject), false);

        _poolRefName = EditorGUILayout.TextField("Pool Ref SO Name", _poolRefName);

        // Show resolved parent prefab path as read-only info
        EditorGUILayout.HelpBox($"Parent prefab: {kParentPrefabPath}", MessageType.None);

        bool parentExists = File.Exists(
            Path.Combine(Application.dataPath.Replace("Assets", ""), kParentPrefabPath));
        if (!parentExists)
            EditorGUILayout.HelpBox(
                "PoolsParent.prefab not found at the hardcoded path.", MessageType.Error);

        EditorGUILayout.Space(4);

        bool section2Ready =
            !string.IsNullOrWhiteSpace(_poolTypeName) &&
            _pooledPrefab != null                     &&
            !string.IsNullOrWhiteSpace(_poolRefName)  &&
            parentExists;

        if (!section2Ready && _poolTypeScript != null)
            EditorGUILayout.HelpBox("Fill in all fields above to create a pool instance.", MessageType.Info);

        EditorGUI.BeginDisabledGroup(!section2Ready);
        if (GUILayout.Button("Create Pool", GUILayout.Height(28)))
            CreatePoolInstance();
        EditorGUI.EndDisabledGroup();
    }

    // ── Phase 1 : write scripts ───────────────────────────────────────────
    private void RunPhaseOne(string type)
    {
        string dir = $"{kImplementationsPath}/{type}";
        Directory.CreateDirectory(
            Path.Combine(Application.dataPath.Replace("Assets", ""), dir));

        WriteScript(dir, $"Lean{type}Pool.cs",     BuildLeanPool(type));
        WriteScript(dir, $"{type}PoolRef.cs",       BuildPoolRef(type));
        WriteScript(dir, $"{type}PoolRefSetter.cs", BuildPoolRefSetter(type));

        EditorPrefs.SetString(kPendingType, type);
        EditorPrefs.SetString(kPendingDir,  dir);

        AssetDatabase.Refresh();
    }

    // ── Phase 2 : create pool type prefab (called after recompile) ────────
    private static void CreatePoolTypePrefab(string type, string dir)
    {
        var leanPoolType = GetTypeFromAllAssemblies($"Lean.Pool.Lean{type}Pool");
        var setterType   = GetTypeFromAllAssemblies($"{type}PoolRefSetter");

        if (leanPoolType == null || setterType == null)
        {
            Debug.LogError($"[PoolWizard] Could not find pool or setter type. Skipping prefab creation.");
            return;
        }

        var go = new GameObject($"{type}Pool");
        go.AddComponent(leanPoolType);
        go.AddComponent(setterType);

        string prefabPath = $"{dir}/{type}Pool.prefab";
        PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
        Object.DestroyImmediate(go);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.FocusProjectWindow();
        Selection.activeObject = AssetDatabase.LoadAssetAtPath<Object>(prefabPath);
        Debug.Log($"[PoolWizard] Created pool type prefab at '{prefabPath}'.");
    }

    // ── Create pool instance ──────────────────────────────────────────────
    private void CreatePoolInstance()
    {
        string type  = _poolTypeName;
        string soDir = $"{kImplementationsPath}/{type}";

        Directory.CreateDirectory(
            Path.Combine(Application.dataPath.Replace("Assets", ""), soDir));

        // 1. Create the PoolRef SO
        var soType = GetTypeFromAllAssemblies($"{type}PoolRef");
        if (soType == null)
        {
            Debug.LogError($"[PoolWizard] Could not find type '{type}PoolRef'. " +
                           "Has the pool type been generated and compiled?");
            return;
        }

        var    so     = ScriptableObject.CreateInstance(soType);
        string soPath = $"{soDir}/{_poolRefName}.asset";
        AssetDatabase.CreateAsset(so, soPath);
        AssetDatabase.SaveAssets();

        // 2. Find the pool type prefab
        string poolPrefabPath = $"{soDir}/{type}Pool.prefab";
        var    poolPrefab     = AssetDatabase.LoadAssetAtPath<GameObject>(poolPrefabPath);
        if (poolPrefab == null)
        {
            Debug.LogError($"[PoolWizard] Could not find pool prefab at '{poolPrefabPath}'. " +
                           "Generate the pool type first.");
            return;
        }

        // 3. Edit the parent prefab
        using (var scope = new PrefabUtility.EditPrefabContentsScope(kParentPrefabPath))
        {
            var root = scope.prefabContentsRoot;

            // Instantiate pool prefab as child
            var poolInstance = (GameObject)PrefabUtility.InstantiatePrefab(
                poolPrefab, root.transform);
            poolInstance.name = _poolRefName;

            // Assign pooled prefab on the LeanPool component
            var leanPoolType = GetTypeFromAllAssemblies($"Lean.Pool.Lean{type}Pool");
            if (leanPoolType != null)
            {
                var leanPool      = poolInstance.GetComponent(leanPoolType);
                var serialized    = new SerializedObject(leanPool);
                var prefabProp    = serialized.FindProperty("prefab");
                if (prefabProp != null)
                {
                    var pooledComponent = _pooledPrefab.GetComponent(
                        GetTypeFromAllAssemblies(type));
                    prefabProp.objectReferenceValue =
                        pooledComponent != null ? (Object)pooledComponent : _pooledPrefab;
                    serialized.ApplyModifiedPropertiesWithoutUndo();
                }
            }

            // Assign the SO on the setter
            var setterType = GetTypeFromAllAssemblies($"{type}PoolRefSetter");
            if (setterType != null)
            {
                var setter         = poolInstance.GetComponent(setterType);
                var serialized     = new SerializedObject(setter);
                var refProp        = serialized.FindProperty("poolRef");
                if (refProp != null)
                {
                    refProp.objectReferenceValue = so;
                    serialized.ApplyModifiedPropertiesWithoutUndo();
                }
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.FocusProjectWindow();
        Selection.activeObject = AssetDatabase.LoadAssetAtPath<Object>(soPath);

        Debug.Log($"[PoolWizard] Pool '{_poolRefName}' added to PoolsParent with SO at '{soPath}'.");

        // Reset section 2
        _poolTypeScript = null;
        _poolTypeName   = "";
        _pooledPrefab   = null;
        _poolRefName    = "";
    }

    // ── Script templates ──────────────────────────────────────────────────
    private static string BuildLeanPool(string type) =>
$@"using UnityEngine;

namespace Lean.Pool
{{
    [ExecuteInEditMode]
    [AddComponentMenu(LeanPool.ComponentPathPrefix + ""{type} Pool"")]
    public class Lean{type}Pool : LeanComponentPool<{type}>
    {{
    }}
}}";

    private static string BuildPoolRef(string type) =>
$@"using Lean.Pool;
using UnityEngine;

[CreateAssetMenu(fileName = ""{type}PoolRef"", menuName = ""Pool System/{type}PoolRef"")]
public class {type}PoolRef : ComponentPoolRef<{type}, Lean{type}Pool> {{ }}";

    private static string BuildPoolRefSetter(string type) =>
$@"using Lean.Pool;

public class {type}PoolRefSetter : ComponentPoolRefSetter<{type}, Lean{type}Pool> {{ }}";

    // ── Utility ───────────────────────────────────────────────────────────
    private static void DrawSeparator()
    {
        var rect = EditorGUILayout.GetControlRect(false, 1);
        EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 0.5f));
        EditorGUILayout.Space(2);
    }

    private static void WriteScript(string dir, string fileName, string content)
    {
        string fullPath = Path.Combine(Application.dataPath.Replace("Assets", ""), dir, fileName);
        File.WriteAllText(fullPath, content);
        Debug.Log($"[PoolWizard] Written '{dir}/{fileName}'");
    }

    private static System.Type GetTypeFromAllAssemblies(string typeName)
    {
        foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
        {
            var t = assembly.GetType(typeName);
            if (t != null) return t;
        }
        return null;
    }

    private static void ClearPending()
    {
        EditorPrefs.DeleteKey(kPendingType);
        EditorPrefs.DeleteKey(kPendingDir);
    }
}
#endif