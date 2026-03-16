using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Dalak.Screenshot
{
    [CustomEditor(typeof(ScreenshotConfiguration))]
    public class ScreenShotConfigurationEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("Capture"))
            {
                ((ScreenshotConfiguration)target).Capture();
            }
            if (GUILayout.Button("Open Directory"))
            {
                var path = ((ScreenshotConfiguration) target).directoryPath;
                path = Path.GetFullPath(path);
                Directory.CreateDirectory(path);
                Process.Start(path);
            }
        }
    }
}