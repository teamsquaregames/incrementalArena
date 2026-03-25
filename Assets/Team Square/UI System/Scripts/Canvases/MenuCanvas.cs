using UnityEngine;

public class MenuCanvas : CanvasHandler
{
    public void StartRun()
    {
        UIManager.Instance.GetCanvas<MenuCanvas>().Close();
        GameManager.Instance.FadeAndEnterRun();
    }
}
