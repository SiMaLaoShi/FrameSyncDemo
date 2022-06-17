using UnityEngine;

public class NetUpdate : MonoBehaviour
{
    private void Update()
    {
        NetGlobal.Instance().DoForAction();
    }

    private void OnApplicationQuit()
    {
        MarsTcp.Instance.EndClient();
        UdpPB.Instance().Destory();
    }
}