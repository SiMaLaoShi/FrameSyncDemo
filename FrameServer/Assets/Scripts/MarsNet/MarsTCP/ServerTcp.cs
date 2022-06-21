using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using PBCommon;
using PBLogin;

public class ServerTcp
{
    private static Socket serverSocket;


    private static readonly object stLockObj = new object();
    private static ServerTcp instance;
    private readonly Dictionary<string, Socket> dic_clientSocket = new Dictionary<string, Socket>();

    private bool isRun;

    private ServerTcp()
    {
    }

    public static ServerTcp Instance
    {
        get
        {
            lock (stLockObj)
            {
                if (instance == null) instance = new ServerTcp();
            }

            return instance;
        }
    }

    public void Destory()
    {
        instance = null;
    }

    public void StartServer()
    {
        try
        {
            var ip = IPAddress.Parse(ServerGlobal.Instance.serverIp);

            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            serverSocket.Bind(new IPEndPoint(ip, ServerConfig.servePort)); //绑定IP地址：端口  
            serverSocket.Listen(20); //设定最多10个排队连接请求  
            LogManage.Instance.AddLog("启动监听" + serverSocket.LocalEndPoint + "成功");
            isRun = true;

            //通过Clientsoket发送数据  
            var myThread = new Thread(ListenClientConnect);
            myThread.Start();
        }
        catch (Exception ex)
        {
            LogManage.Instance.AddLog("服务器启动失败:" + ex.Message, CLogType.Exception);
        }
    }


    private void ListenClientConnect()
    {
        while (isRun)
            try
            {
                var clientSocket = serverSocket.Accept();

//				clientSocket.IOControl(IOControlCode.KeepAliveValues, inOptionValues, null);

                var receiveThread = new Thread(ReceiveMessage);
                receiveThread.Start(clientSocket);
            }
            catch (Exception ex)
            {
                LogManage.Instance.AddLog("监听失败:" + ex.Message, CLogType.Exception);
            }
    }


    public void EndServer()
    {
        if (!isRun) return;

        isRun = false;
        try
        {
            foreach (var item in dic_clientSocket) item.Value.Close();

            dic_clientSocket.Clear();

            if (serverSocket != null)
            {
                serverSocket.Close();
                serverSocket = null;
            }
        }
        catch (Exception ex)
        {
            LogManage.Instance.AddLog("tcp服务器关闭失败:" + ex.Message);
        }
    }

    public void CloseClientTcp(string _socketIp)
    {
        try
        {
            if (dic_clientSocket.ContainsKey(_socketIp))
            {
                if (dic_clientSocket[_socketIp] != null) dic_clientSocket[_socketIp].Close();
                dic_clientSocket.Remove(_socketIp);
            }
        }
        catch (Exception ex)
        {
            LogManage.Instance.AddLog("关闭客户端..." + ex.Message);
        }
    }

    public int GetClientCount()
    {
        return dic_clientSocket.Count;
    }

    public List<string> GetAllClientIp()
    {
        return new List<string>(dic_clientSocket.Keys);
    }


    private void ReceiveMessage(object clientSocket)
    {
        var myClientSocket = (Socket)clientSocket;
        var _socketIp = myClientSocket.RemoteEndPoint.ToString().Split(':')[0];

        LogManage.Instance.AddLog("有客户端连接:" + _socketIp);
        
        dic_clientSocket[_socketIp] = myClientSocket;

        var _flag = true;

        var resultData = new byte[1024];
        while (isRun && _flag)
            try
            {
				LogManage.Instance.AddLog("_socketName是否连接:" + myClientSocket.Connected);
                //通过clientSocket接收数据  
                if (myClientSocket.Poll(1000, SelectMode.SelectRead)) throw new Exception("客户端关闭了1~");

                var _size = myClientSocket.Receive(resultData);

                if (_size <= 0) throw new Exception("客户端关闭了2~");

                var packMessageId = resultData[PackageConstant.PackMessageIdOffset]; //消息id (1个字节)
                var packlength = BitConverter.ToInt16(resultData, PackageConstant.PacklengthOffset); //消息包长度 (2个字节)
                var bodyDataLenth = packlength - PackageConstant.PacketHeadLength;
                var bodyData = new byte[bodyDataLenth];
                Array.Copy(resultData, PackageConstant.PacketHeadLength, bodyData, 0, bodyDataLenth);

                if ((CSID)packMessageId == CSID.TCP_LOGIN)
                {
                    var _info = CSData.DeserializeData<TcpLogin>(bodyData);
                    var _uid = UserManage.Instance.UserLogin(_info.token, _socketIp);
                    dic_clientSocket[_uid.ToString()] = myClientSocket;
                    var _result = new TcpResponseLogin();
                    _result.result = true;
                    _result.uid = _uid;
                    _result.udpPort = UdpManager.Instance.recvPort;
                    LogManage.Instance.AddLog(string.Format("tcp login {0}", _socketIp));
                    ServerTcp.Instance.SendMessage(_uid, CSData.GetSendMessage(_result, SCID.TCP_RESPONSE_LOGIN));
                }
                else
                {
                    TcpPB.Instance().AnalyzeMessage((CSID)packMessageId, bodyData, _socketIp);    
                }
                
            }
            catch (Exception ex)
            {
                LogManage.Instance.AddLog("接收客户端数据异常:" + ex.Message);

                _flag = false;
                break;
            }

        CloseClientTcp(_socketIp);
    }

    public void SendMessageAll(byte[] _mes)
    {
        if (isRun)
            try
            {
                foreach (var item in dic_clientSocket) item.Value.Send(_mes);
            }
            catch (Exception ex)
            {
                LogManage.Instance.AddLog("发数据给所有人异常:" + ex.Message);
            }
    }

    public void SendMessage(int uid, byte[] _mes)
    {
        if (isRun)
            try
            {
                dic_clientSocket[uid.ToString()].Send(_mes);
            }
            catch (Exception ex)
            {
                LogManage.Instance.AddLog("发数据给异常:" + ex.Message);
            }
    }

    public void SendMessage(string _socketName, byte[] _mes)
    {
        if (isRun)
            try
            {
                dic_clientSocket[_socketName].Send(_mes);
            }
            catch (Exception ex)
            {
                LogManage.Instance.AddLog("发数据给异常:" + ex.Message);
            }
    }
}