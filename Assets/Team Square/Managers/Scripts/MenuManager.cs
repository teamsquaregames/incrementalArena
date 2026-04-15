using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    private void Awake()
    {
        if (GameConfig.Instance.cheatSettings.noMenu)
        {
            NoMenu();
        }
    }

    private async Task NoMenu()
    {
        UIManager.Instance.GetCanvas<MenuCanvas>().Close();
        await SceneManager.LoadSceneAsync(GameAssets.Instance.arenaConfigs.Find(x => x.arenaID == GameData.Instance.currentArenaID).sceneRef.SceneName);
        GameManager.Instance.EnterRun();
    }
}
