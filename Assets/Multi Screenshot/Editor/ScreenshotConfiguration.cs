using System.Collections.Generic;
using UnityEngine;

namespace Dalak.Screenshot
{
    [CreateAssetMenu(menuName = "Dalak/Screenshot/Configuration",fileName = "ScreenshotConfiguration")]
    public class ScreenshotConfiguration : ScriptableObject
    {
        [System.Serializable]
        public class Item
        {
            public int resolutionX;
            public int resolutionY;
            public string fileName;
            public bool captureSceneView;
        }

        public string directoryPath;
        public Item[] items;

        
        
        public void Capture()
        {
            Dictionary<string,int> fileNameSet = new Dictionary<string,int>();
            for (int i = 0; i < items.Length; i++)
            {
                if (string.IsNullOrEmpty(items[i].fileName) || string.IsNullOrWhiteSpace(items[i].fileName))
                {
                    Debug.LogError($"Empty file name at {i}");
                    return;
                }
                
                if (fileNameSet.TryGetValue(items[i].fileName,out var idx))
                {
                    Debug.LogError($"Conflicting file names at {idx} and {i}");
                    return;
                }
                fileNameSet.Add(items[i].fileName,i);
            }


            var settingsArray = new ScreenshotSettings[items.Length];
            for (int i = 0; i < items.Length; i++)
            {
                var item = items[i];
                var path = Screenshot.GetAvailableFilePath(directoryPath,item.fileName);
                settingsArray[i] = new ScreenshotSettings(item.resolutionX,item.resolutionY,item.captureSceneView,path);
            }
            
            Screenshot.Capture(settingsArray);
        }
    }
}
