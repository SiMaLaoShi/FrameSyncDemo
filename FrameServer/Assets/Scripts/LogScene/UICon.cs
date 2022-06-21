using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UICon : MonoBehaviour
{
    public Text serverIP;
    public InputField input;
    public Button startserver;

    public RectTransform logContent;

    private readonly List<string> logs = new List<string>();
    private float logTextHeght;
    private int logTextNum;
    private GameObject logTextPrefab;

    private void Start()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        serverIP.text = ServerGlobal.Instance.serverIp;

        ServerConfig.battleUserNum = PlayerPrefs.GetInt("battleNumber", 1);
        input.text = ServerConfig.battleUserNum.ToString();

        logTextPrefab = Resources.Load<GameObject>("LogText");
        logTextHeght = 60f;
        logTextNum = 0;

        LogManage.Instance.logChange = LogChange;
    }

    private void Update()
    {
        for (var i = 0; i < logs.Count; i++) PushLog(logs[i]);
        logs.Clear();
    }

    public void StartServer()
    {
        int _number;
        if (int.TryParse(input.text, out _number))
        {
            ServerConfig.battleUserNum = _number;
            PlayerPrefs.SetInt("battleNumber", _number);
        }

        input.interactable = false;
        startserver.interactable = false;

        ServerTcp.Instance.StartServer();
        UdpManager.Instance.Creat();
    }

    private void PushLog(string _log)
    {
        var _logObj = Instantiate(logTextPrefab, logContent);
        var _logTextTran = _logObj.GetComponent<RectTransform>();
        _logTextTran.anchoredPosition = new Vector2(0, -logTextNum * logTextHeght);
        _logObj.GetComponent<Text>().text = _log;

        logTextNum++;
        logContent.sizeDelta = new Vector2(logContent.sizeDelta.x, logTextNum * logTextHeght);
    }

    private void LogChange(string _log, CLogType logType = CLogType.Log)
    {
        
        logs.Add(_log);
        switch (logType)
        {
            case CLogType.Error:
                Debug.LogError(_log);
                break;
            case CLogType.Assert:
                Debug.LogAssertionFormat(_log);
                break;
            case CLogType.Warning:
                Debug.LogWarning(_log);
                break;
            case CLogType.Log:
                Debug.Log(_log);
                break;
            case CLogType.Exception:
                Debug.LogErrorFormat(_log);
                break;
            default:
                throw new ArgumentOutOfRangeException("logType", logType, null);
        }
        
    }
}