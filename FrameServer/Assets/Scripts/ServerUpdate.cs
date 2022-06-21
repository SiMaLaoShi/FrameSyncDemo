using UnityEngine;

public class ServerUpdate : MonoBehaviour
{
    private void Update()
    {
        ServerGlobal.Instance.DoForAction();
    }

    private void OnApplicationQuit()
    {
        LogManage.Instance.Destory();
        ServerTcp.Instance.EndServer();
        UdpManager.Instance.Destory();
        BattleManage.Instance.Destroy();
    }
}