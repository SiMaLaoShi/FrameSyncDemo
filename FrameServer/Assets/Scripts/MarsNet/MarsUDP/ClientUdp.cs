using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using PBCommon;

public class ClientUdp
{
    public delegate void DelegateAnalyzeMessage(CSID messageId, byte[] bodyData);

    public DelegateAnalyzeMessage delegate_analyze_message;
    private bool isRun;
    private UdpClient sendClient;
    private IPEndPoint sendEndPort;
    private int sendPortNum;
    private string serverIp;

    public int userUid;

    public void StartClientUdp(string _ip, int _uid)
    {
        if (sendEndPort != null)
        {
            LogManage.Instance.AddLog("客户端udp已经启动~");
            return;
        }

        userUid = _uid;
        serverIp = _ip;
        isRun = true;

        sendClient = UdpManager.Instance.GetClient();
//		sendClient = new UdpClient(NormalData.recvPort);
//		sendEndPort = new IPEndPoint(IPAddress.Parse(_ip), ServerConfig.udpRecvPort);	

        var t = new Thread(RecvThread);
        t.Start();
    }

    public void EndClientUdp()
    {
        try
        {
            isRun = false;
            if (sendEndPort != null)
            {
                UdpManager.Instance.CloseUdpClient();
                sendClient = null;
                sendEndPort = null;
            }

            delegate_analyze_message = null;
        }
        catch (Exception ex)
        {
            LogManage.Instance.AddLog("udp连接关闭异常:" + ex.Message, CLogType.Exception);
        }
    }

    private void CreatSendEndPort(int _port)
    {
        sendEndPort = new IPEndPoint(IPAddress.Parse(serverIp), _port);
    }

    public void SendMessage(byte[] _mes)
    {
        if (isRun)
            try
            {
                sendClient.Send(_mes, _mes.Length, sendEndPort);
//				GameData.Instance().sendNum+=_mes.Length;
                //				LogManage.Instance.AddLog("发送量:" + _mes.Length.ToString() + "," + GameData.Instance().sendNum.ToString());
            }
            catch (Exception ex)
            {
                LogManage.Instance.AddLog("udp发送失败:" + ex.Message, CLogType.Exception);
            }
    }


    public void RecvClientReady(int _userUid)
    {
        if (_userUid == userUid && sendEndPort == null) CreatSendEndPort(sendPortNum);
    }

    private void RecvThread()
    {
        LogManage.Instance.AddLog(string.Format("RecvThread ip:{0} port:{1}", serverIp, UdpManager.Instance.recvPort));
        var endpoint = new IPEndPoint(IPAddress.Parse(serverIp), UdpManager.Instance.recvPort);
        while (isRun)
        {
            /*try
            {
                var buf = sendClient.Receive(ref endpoint);

                if (sendEndPort == null)
                {
                    LogManage.Instance.AddLog("接收客户端udp信息:" + endpoint.Port);
                    sendPortNum = endpoint.Port;
                }

                var packMessageId = buf[PackageConstant.PackMessageIdOffset]; //消息id (1个字节)
                var packlength = BitConverter.ToInt16(buf, PackageConstant.PacklengthOffset); //消息包长度 (2个字节)
                var bodyDataLenth = packlength - PackageConstant.PacketHeadLength;
                var bodyData = new byte[bodyDataLenth];
                Array.Copy(buf, PackageConstant.PacketHeadLength, bodyData, 0, bodyDataLenth);

                delegate_analyze_message((CSID)packMessageId, bodyData);

                //是客户端,统计接收量
//				GameData.Instance().recvNum+=buf.Length;
                //				LogManage.Instance.AddLog("发送量:" + buf.Length.ToString() + "," + GameData.Instance().recvNum.ToString());
            }
            catch (Exception ex)
            {
                LogManage.Instance.AddLog("udpClient接收数据异常:" + ex.Message, CLogType.Exception);
            }*/
            var buf = sendClient.Receive(ref endpoint);

            if (sendEndPort == null)
            {
                LogManage.Instance.AddLog("接收客户端udp信息:" + endpoint.Port);
                sendPortNum = endpoint.Port;
            }

            var packMessageId = buf[PackageConstant.PackMessageIdOffset]; //消息id (1个字节)
            var packlength = BitConverter.ToInt16(buf, PackageConstant.PacklengthOffset); //消息包长度 (2个字节)
            var bodyDataLenth = packlength - PackageConstant.PacketHeadLength;
            var bodyData = new byte[bodyDataLenth];
            Array.Copy(buf, PackageConstant.PacketHeadLength, bodyData, 0, bodyDataLenth);

            delegate_analyze_message((CSID)packMessageId, bodyData);
           
        }
          

        LogManage.Instance.AddLog("udp接收线程退出~~~~~");
    }


    private void OnDestroy()
    {
        EndClientUdp();
    }
}