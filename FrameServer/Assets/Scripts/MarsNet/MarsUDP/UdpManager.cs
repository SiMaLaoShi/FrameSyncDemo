using System.Net;
using System.Net.Sockets;

public class UdpManager
{
    private static UdpManager singleInstance;
    private static readonly object padlock = new object();

    public UdpClient _udpClient;
    public int recvPort;

    private UdpManager()
    {
        CreatUdp();
    }

    public static UdpManager Instance
    {
        get
        {
            lock (padlock)
            {
                if (singleInstance == null) singleInstance = new UdpManager();
                return singleInstance;
            }
        }
    }

    public void Creat()
    {
    }

    private void CreatUdp()
    {
        _udpClient = new UdpClient(ServerConfig.udpRecvPort);
        var _localip = (IPEndPoint)_udpClient.Client.LocalEndPoint;
        LogManage.Instance.AddLog("udp端口:" + _localip.Port);
        recvPort = _localip.Port;
    }

    public void Destory()
    {
        CloseUdpClient();
        singleInstance = null;
    }

    public void CloseUdpClient()
    {
        if (_udpClient != null)
        {
            _udpClient.Close();
            _udpClient = null;
        }
    }

    public UdpClient GetClient()
    {
        if (_udpClient == null) CreatUdp();
        return _udpClient;
    }
}