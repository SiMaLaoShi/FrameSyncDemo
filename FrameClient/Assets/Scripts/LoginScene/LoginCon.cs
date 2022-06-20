using System;
using PBCommon;
using PBLogin;
using UnityEngine;
using UnityEngine.UI;
using UnityGameFramework.Runtime;

public class LoginCon : MonoBehaviour
{
    public InputField inputField;
    public GameObject waitTip;

    private void Start()
    {
#if !UNITY_EDITOR
        var debug = new GameObject();
        DontDestroyOnLoad(debug);
        debug.AddComponent<DebuggerComponent>();
#endif
        
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        Application.targetFrameRate = 30;

        NetGlobal.Instance();
        TcpPB.Instance().mes_login_result = Message_Login_Result;
        waitTip.SetActive(false);
    }

    private void OnDestroy()
    {
        TcpPB.Instance().mes_login_result = null;
    }


    public void OnClickLogin()
    {
        Debug.Log(inputField.text);
        
        waitTip.SetActive(true);
        
        var _ip = inputField.text;
        MarsTcp.Instance.ConnectServer(_ip, _result =>
        {
            if (_result)
            {
                Debug.Log("连接成功～～");
                NetGlobal.Instance().serverIP = _ip;
                var _loginInfo = new TcpLogin();
                _loginInfo.token = SystemInfo.deviceUniqueIdentifier;
                MarsTcp.Instance.SendMessage(CSData.GetSendMessage(_loginInfo, CSID.TCP_LOGIN));
            }
            else
            {
                Debug.Log("连接失败～～");
                waitTip.SetActive(false);
            }
        });
    }


    private void Message_Login_Result(TcpResponseLogin _mes)
    {
        if (_mes.result)
        {
            NetGlobal.Instance().userUid = _mes.uid;
            NetGlobal.Instance().udpSendPort = _mes.udpPort;
            Debug.Log("登录成功～～～" + NetGlobal.Instance().userUid);
            ClearSceneData.LoadScene(GameConfig.mainScene);
        }
        else
        {
            Debug.Log("登录失败～～～");
            waitTip.SetActive(false);
        }
    }
}