using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Object = UnityEngine.Object;

public class ServerGlobal
{
    private static ServerGlobal instance;
    private readonly List<Action> list_action = new List<Action>();
    private readonly Mutex mutex_actionList = new Mutex();

    public string serverIp;

    private ServerGlobal()
    {
        var obj = new GameObject("ServerGlobal");
        obj.AddComponent<ServerUpdate>();
        Object.DontDestroyOnLoad(obj);
        serverIp = Network.player.ipAddress;
    }

    public static ServerGlobal Instance
    {
        get
        {
            if (instance == null) instance = new ServerGlobal();
            return instance;
        }
    }

    public void Destory()
    {
        instance = null;
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