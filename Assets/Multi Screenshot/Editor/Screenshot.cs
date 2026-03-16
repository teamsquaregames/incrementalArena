using System.Collections;
using System.IO;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;

namespace Dalak.Screenshot
{
    [System.Serializable]
    public class ScreenshotSettings
    {
        public int resolutionX;
        public int resolutionY;
        public string path;
        public bool captureSceneView;

        public ScreenshotSettings(int resolutionX, int resolutionY, bool captureSceneView,string path)
        {
            this.resolutionX = resolutionX;
            this.resolutionY = resolutionY;
            this.captureSceneView = captureSceneView;
            this.path = path;
        }
    }
    
    public static class Screenshot
    {
        public static string GetAvailableFilePath(string directoryPath, string fileName)
        {
            var path = Path.Combine(directoryPath, $"{fileName}.png");
            bool fileExist = File.Exists(path);
            for (int i = 1; i <= 50 && fileExist; i++)
            {
                path = Path.Combine(directoryPath, $"{fileName}_{i}.png");
                fileExist = File.Exists(path);
            }

            if (fileExist)
            {
                path = Path.Combine(directoryPath, $"{fileName}_{Path.GetRandomFileName()}.png");
            }
            return path;
        }
        
        public static void Capture(params ScreenshotSettings[] settingsArray)
        {
            EditorCoroutineUtility.StartCoroutineOwnerless(CCapture(settingsArray));
        }

        static IEnumerator CCapture(ScreenshotSettings[] settingsArray)
        {
            bool destroyCamera = false;
            Camera camera = Camera.main;
            if (camera == null) camera = Object.FindObjectOfType<Camera>();
            if (camera == null)
            {
                camera = new GameObject("Dalak Screenshot Camera").AddComponent<Camera>();
                destroyCamera = true;
            }

            void FixUI()
            {
                foreach (var cam in GameObject.FindObjectsOfType<Camera>())
                {
                    cam.Render();
                }
            }

            var cameraTransform = camera.transform;
            Vector3 camSavedPos = cameraTransform.position;
            Quaternion camSavedRot = cameraTransform.rotation;
            float camSavedFov = camera.fieldOfView;

            var size = GameViewUtils.GetCurrentSize();
            foreach (var settings in settingsArray)
            {
                if (string.IsNullOrEmpty(settings.path))
                {
                    Debug.LogError("Setting has no path");
                    continue;
                }
                int x = settings.resolutionX;
                int y = settings.resolutionY;
                string resName = $"DalakSS [{x},{y}]";

                if (x > 0 && y > 0)
                {
                    GameViewUtils.SelectSize(GameViewUtils.AddCustomSize(x,y,resName));
                }
                
                FixUI();
                yield return null;
                
                Directory.CreateDirectory(Path.GetDirectoryName(settings.path));
                
                if (!settings.captureSceneView)
                {
                    GameViewUtils.GetMainGameView().Focus();
                    yield return null;
                    ScreenCapture.CaptureScreenshot(settings.path);
                    while (!File.Exists(settings.path))
                    {
                        GameViewUtils.GetMainGameView().Focus();
                        yield return null;
                    }
                    Debug.Log($"[Dalak Screenshot] SS saved to {settings.path}");
                }
                else
                {
                    SceneViewCameraUtils.GetPositionAndRotation(out var pos, out var rot);
                    camera.transform.SetPositionAndRotation(pos, rot);
                    camera.fieldOfView = SceneViewCameraUtils.FOV();
                    GameViewUtils.GetMainGameView().Focus();
                    yield return null;
                    
                    ScreenCapture.CaptureScreenshot(settings.path);
                    while (!File.Exists(settings.path))
                    {
                        GameViewUtils.GetMainGameView().Focus();
                        yield return null;
                    }
                    
                    // Revert Camera
                    cameraTransform.SetPositionAndRotation(camSavedPos,camSavedRot);
                    camera.fieldOfView = camSavedFov;
                }
                yield return null;
            }

            while (true)
            {
                int validationCounter = 0;
                foreach (var settings in settingsArray)
                {
                    if (File.Exists(settings.path))
                    {
                        validationCounter++;
                    }
                }

                if (validationCounter == settingsArray.Length)
                {
                    foreach (var settings in settingsArray)
                    {
                        var fileData = File.ReadAllBytes(settings.path);
                        var tex = new Texture2D(2, 2);
                        tex.LoadImage(fileData);
                        if (tex.width != settings.resolutionX || tex.height != settings.resolutionY)
                        {
                            Debug.LogError($"Screenshot resolution is not correct \n{Path.GetFileName(settings.path)}\n" +
                                           $"Settings: {settings.resolutionX},{settings.resolutionY}\n" + 
                                           $"Captured: {tex.width},{tex.height}\n");
                        }
                    }
                    Debug.Log("Screenshot capture completed");
                    break;
                }
                yield return null;
            }
            
            if (destroyCamera)
            {
                Object.DestroyImmediate(camera.gameObject);
            }
            
            GameViewUtils.SelectSize(size);
        }
        
    }
}