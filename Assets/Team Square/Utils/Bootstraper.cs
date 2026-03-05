using UnityEngine;
using UnityEngine.SceneManagement;

public class Bootstraper : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Init()
    {
        if (GameConfig.Instance.cheatSettings.disableBootStrapper) return;
        
        Scene _currentScene = SceneManager.GetActiveScene();
    
        if (_currentScene.name != "InitScene")
        {
            foreach (GameObject obj in FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                obj.SetActive(false);
                
            SceneManager.LoadScene("InitScene");
        }
        
        LoadNextScene();
    }

    private static void LoadNextScene()
    {
        if (GameConfig.Instance.cheatSettings.noMenu)
            SceneManager.LoadSceneAsync("MainScene", LoadSceneMode.Single);
        else
            SceneManager.LoadSceneAsync("MenuScene", LoadSceneMode.Single);
    }
}