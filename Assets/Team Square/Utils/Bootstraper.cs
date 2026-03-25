using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Bootstraper : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static async Task Init()
    {
        if (GameConfig.Instance.cheatSettings.disableBootStrapper) return;
        
        Scene _currentScene = SceneManager.GetActiveScene();
    
        if (_currentScene.name != "InitScene")
        {
            foreach (GameObject obj in FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                obj.SetActive(false);
                
            await SceneManager.LoadSceneAsync("InitScene");
        }
        
        SceneManager.LoadSceneAsync("MenuScene", LoadSceneMode.Single);
    }
}