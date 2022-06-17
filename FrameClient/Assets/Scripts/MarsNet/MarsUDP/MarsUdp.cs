using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using PBCommon;
using UnityEngine;

public class MarsUdp
{
    public delegate void DelegateAnalyzeMessage(SCID messageId, byte[] bodyData);

    public DelegateAnalyzeMessage delegate_analyze_message;

    private bool isRecv;

//	private IPEndPoint sendEndPort;
    private bool isRun;

    private UdpClient sendClient;

    public void StartClientUdp(string _ip)
    {
//		if (sendEndPort != null) {
//			Debug.Log ("客户端udp已经启动~");
//			return;
//		}

        if (isRun)
        {
            Debug.Log("客户端udp已经启动~");
            return;
        }

        isRun = true;

        sendClient = UdpManager.Instance.GetClient();
//		sendEndPort = new IPEndPoint(IPAddress.Parse(_ip), NetConfig.UdpSendPort);

        StartRecvMessage();
    }

    private void StartRecvMessage()
    {
        var t = new Thread(RecvThread);
        t.Start();
    }

    public void StopRecvMessage()
    {
        isRecv = false;
    }

    public void EndClientUdp()
    {
        try
        {
            isRun = false;
            isRecv = false;
//			if (sendEndPort != null) {
//				UdpManager.Instance.CloseUdpClient();
//				sendClient = null;
//				sendEndPort = null;
//			}
            UdpManager.Instance.CloseUdpClient();
            sendClient = null;
            delegate_analyze_message = null;
        }
        catch (Exception ex)
        {
            Debug.Log("udp连接关闭异常:" + ex.Message);
        }
    }

    public void SendMessage(byte[] _mes)
    {
        if (isRun)
            try
            {
                sendClient.Send(_mes, _mes.Length);
//				sendClient.Send (_mes,_mes.Length,sendEndPort);	
                BattleData.Instance.sendNum += _mes.Length;
//				Debug.Log("发送量:" + _mes.Length.ToString());
                // Debug.Log("udp发送量：" + _mes.Length);
            }
            catch (Exception ex)
            {
                Debug.Log("udp发送失败:" + ex.Message);
            }
    }


    private void RecvThread()
    {
        isRecv = true;
        var endpoint = new IPEndPoint(IPAddress.Parse(NetGlobal.Instance().serverIP), UdpManager.Instance.localPort);
        while (isRecv)
            try
            {
                var buf = sendClient.Receive(ref endpoint);

                var packMessageId = buf[PackageConstant.PackMessageIdOffset]; //消息id (1个字节)
                var packlength = BitConverter.ToInt16(buf, PackageConstant.PacklengthOffset); //消息包长度 (2个字节)
                var bodyDataLenth = packlength - PackageConstant.PacketHeadLength;
                var bodyData = new byte[bodyDataLenth];
                Array.Copy(buf, PackageConstant.PacketHeadLength, bodyData, 0, bodyDataLenth);

                delegate_analyze_message((SCID)packMessageId, bodyData);

                //是客户端,统计接收量
                BattleData.Instance.recvNum += buf.Length;
//				Debug.Log("发送量:" + buf.Length.ToString() + "," + GameData.Instance().recvNum.ToString());
            }
            catch (Exception ex)
            {
                Debug.Log("udpClient接收数据异常:" + ex.Message);
            }

        Debug.Log("udp接收线程退出~~~~~");
    }


    private void OnDestroy()
    {
        EndClientUdp();
    }
}