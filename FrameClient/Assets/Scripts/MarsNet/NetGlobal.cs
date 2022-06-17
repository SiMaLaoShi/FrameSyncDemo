using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Object = UnityEngine.Object;

public class NetGlobal
{
    private static NetGlobal singleInstance;
    private readonly List<Action> list_action = new List<Action>();
    private readonly Mutex mutex_actionList = new Mutex();

    public string serverIP;
    public int udpSendPort;
    public int userUid;

    private NetGlobal()
    {
        var obj = new GameObject("NetGlobal");
        obj.AddComponent<NetUpdate>();
        Object.DontDestroyOnLoad(obj);
    }

    public static NetGlobal Instance()
    {
        // 如果类的实例不存在则创建，否则直接返回
        if (singleInstance == null) singleInstance = new NetGlobal();
        return singleInstance;
    }

    public void Destory()
    {
        singleInstance = null;
    }

    public void AddAction(Action _action)
    {
        mutex_actionList.WaitOne();
        list_action.Add(_action);
        mutex_actionList.ReleaseMutex();
    }

    public void DoForAction()
    {
        mutex_actionList.WaitOne();
        for (var i = 0; i < list_action.Count; i++) list_action[i]();
        list_action.Clear();
        mutex_actionList.ReleaseMutex();
    }
}