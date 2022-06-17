using UnityEngine;

public class UIGameOver : MonoBehaviour
{
    private void Start()
    {
    }

    public void ShowSelf()
    {
        gameObject.SetActive(true);
    }

    public void HideSelf()
    {
        gameObject.SetActive(false);
    }

    public void OnClickExit()
    {
        ClearSceneData.LoadScene(GameConfig.mainScene);
    }
}