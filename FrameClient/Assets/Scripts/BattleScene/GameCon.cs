using UnityEngine;

public class GameCon : MonoBehaviour
{
    public bool isBattle = true;

    public UIGameOver uiGameOver;
    public GameObject uiReady;
    public Transform mapTranform;

    private void Start()
    {
        if (isBattle)
        {
            uiReady.SetActive(true);
            var battleCon = gameObject.AddComponent<BattleCon>();
            battleCon.delegate_readyOver = ReadyFinish;
            battleCon.delegate_gameOver = GameOver;
            battleCon.InitData(mapTranform);
        }
    }

    private void ReadyFinish()
    {
        uiReady.SetActive(false);
    }

    private void GameOver()
    {
        uiGameOver.ShowSelf();
    }
}