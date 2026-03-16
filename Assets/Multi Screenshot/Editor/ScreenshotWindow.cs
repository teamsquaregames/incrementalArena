using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text.RegularExpressions;
using Debug = UnityEngine.Debug;

namespace Dalak.Screenshot
{
    public class ScreenshotWindow : EditorWindow
    {
        [MenuItem("Tools/Multi Screen Shot/Screen Shot Window")]
        public static void ShowWindow()
        {
            EditorWindow window = GetWindow(typeof(ScreenshotWindow));
            window.Show();
        }

        string fileName = "ss_";
        bool captureSceneView = false;
        bool useGameViewResolution = true;
        int resolutionX;
        int resolutionY;
        
        static string ScreenshotDirectory
        {
            get => EditorPrefs.GetString("com.dalakgames.screenshot.directory", "Screenshots");
            set => EditorPrefs.SetString("com.dalakgames.screenshot.directory", value);
        }

        readonly List<ScreenshotConfiguration> configurations = new List<ScreenshotConfiguration>();
        void OnEnable()
        {
            configurations.Clear();
            var guids = AssetDatabase.FindAssets("t:ScreenshotConfiguration");
            foreach (var guid in guids)
            {
                var screenshotConfiguration = AssetDatabase.LoadAssetAtPath<ScreenshotConfiguration>
                    (AssetDatabase.GUIDToAssetPath(guid));
                configurations.Add(screenshotConfiguration);
            }
            
            string defConfigurationsPath = Path.Combine("Packages", "com.dalakgames.screenshot", "Editor",
                "Configurations");
            var objects = AssetDatabase.LoadAllAssetsAtPath(defConfigurationsPath);
            foreach (var c in objects)
            {
                var config = (ScreenshotConfiguration) c;
                configurations.Add(config);
            }
        }

        Vector2 scrollPos;
        void OnGUI()
        {
            EditorGUILayout.LabelField(ScreenshotDirectory);
            if (GUILayout.Button($"Change Directory"))
            {
                string path = EditorUtility.OpenFolderPanel("Select screenshot folder", "","");
                Debug.Log(path);
                if (!string.IsNullOrEmpty(path) && Directory.Exists(path))
                {
                    ScreenshotDirectory = path;
                    Debug.Log($"[Dalak Screenshot] directory changed {path}");
                }
            }
            
            if (GUILayout.Button("Open Folder"))
            {
                Process.Start(ScreenshotDirectory);
            }
            
            GUILayout.Space(10);

            EditorGUILayout.TextField($"ScreenShotName:", fileName);
            captureSceneView = EditorGUILayout.Toggle("Capture Scene View", captureSceneView);
            useGameViewResolution = EditorGUILayout.Toggle("Use GameView Resolution", useGameViewResolution);
            
            GUI.enabled = !useGameViewResolution;
            resolutionX = EditorGUILayout.IntField("Resolution X", resolutionX);
            resolutionY = EditorGUILayout.IntField("Resolution Y", resolutionY);
            GUI.enabled = true;

            if (GUILayout.Button("Capture"))
            {
                string path = Screenshot.GetAvailableFilePath(ScreenshotDirectory, fileName);
                int x = useGameViewResolution ? 0 : resolutionX;
                int y = useGameViewResolution ? 0 : resolutionY;
                Screenshot.Capture(new ScreenshotSettings(x,y,captureSceneView,path));
            }
            
            GUILayout.Space(20);
            
            EditorGUILayout.LabelField("Configurations");
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            EditorGUILayout.BeginVertical();
            foreach (var configuration in configurations)
            {
                EditorGUILayout.LabelField(configuration.name);
                if (GUILayout.Button("Capture"))
                {
                    configuration.Capture();
                }
                if (GUILayout.Button("Open Directory"))
                {
                    Process.Start(Path.GetFullPath(configuration.directoryPath));
                }
                
                if (GUILayout.Button("Select"))
                {
                    Selection.activeObject = configuration;
                }
                GUILayout.Space(5);
            }
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
        }
    }
}
