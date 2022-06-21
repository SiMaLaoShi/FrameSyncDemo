using System.Collections.Generic;
using System.Threading;
using PBBattle;
using PBCommon;
using UnityEngine;

public class BattleCon
{
    private bool _isRun;
    private bool allGameOver;
    private int battleID;

    private Dictionary<int, bool> dic_battleReady;
    private Dictionary<int, int> dic_battleUserUid;

    private readonly Dictionary<int, AllPlayerOperation> dic_gameOperation = new Dictionary<int, AllPlayerOperation>();
    private Dictionary<int, ClientUdp> dic_udp;
    private float finishTime; //结束倒计时
    /// <summary>
    /// 游戏帧数
    /// </summary>
    private int frameNum;

    /// <summary>
    /// 记录当前帧操作的玩家
    /// </summary>
    private PlayerOperation[] frameOperation; 
    private bool isBeginBattle;
    private int lastFrame;
    private bool oneGameOver;
    /// <summary>
    /// 记录玩家游戏结束
    /// </summary>
    private bool[] playerGameOver;
    /// <summary>
    /// 记录玩家的包id
    /// </summary>
    private int[] playerMesNum;


    private Timer waitBattleFinish;

    public void CreatBattle(int _battleID, List<MatchUserInfo> _battleUser)
    {
        var randSeed = Random.Range(0, 100);
        // ThreadPool.QueueUserWorkItem(obj =>
        // {
        //     
        // }, null);
        battleID = _battleID;
        dic_battleUserUid = new Dictionary<int, int>();
        dic_udp = new Dictionary<int, ClientUdp>();
        dic_battleReady = new Dictionary<int, bool>();

        var userBattleID = 0;

        var _mes = new TcpEnterBattle
        {
            randSeed = randSeed
        };
        for (var i = 0; i < _battleUser.Count; i++)
        {
            var _userUid = _battleUser[i].uid;
            userBattleID++;

            dic_battleUserUid[_userUid] = userBattleID;

            var _ip = UserManage.Instance.GetUserInfo(_userUid).socketIp;
            var _upd = new ClientUdp();

            _upd.StartClientUdp(_ip, _userUid);
            _upd.delegate_analyze_message = AnalyzeMessage;
            dic_udp[userBattleID] = _upd;
            dic_battleReady[userBattleID] = false;

            var _bUser = new BattleUserInfo();
            _bUser.uid = _userUid;
            _bUser.battleID = userBattleID;
            _bUser.roleID = _battleUser[i].roleID;

            _mes.battleUserInfo.Add(_bUser);
        }

        LogManage.Instance.AddLog(string.Format("init battle {0}", _battleUser.Count));
        for (var i = 0; i < _battleUser.Count; i++)
        {
            LogManage.Instance.AddLog(string.Format("send {0}", i));
            var _userUid = _battleUser[i].uid;
            var _ip = UserManage.Instance.GetUserInfo(_userUid).socketIp;
            LogManage.Instance.AddLog(string.Format("send {1} TCP_ENTER_BATTLE {0}", _ip, _userUid));
            //给玩家发送进入战斗的消息
            ServerTcp.Instance.SendMessage(_userUid, CSData.GetSendMessage(_mes, SCID.TCP_ENTER_BATTLE));
        }
    }

    public void DestroyBattle()
    {
        foreach (var item in dic_udp.Values) item.EndClientUdp();

        _isRun = false;
    }

    private void FinishBattle()
    {
        foreach (var item in dic_udp.Values) item.EndClientUdp();

        BattleManage.Instance.FinishBattle(battleID);
    }

    private void CheckBattleBegin(int _userBattleID)
    {
        if (isBeginBattle) return;

        dic_battleReady[_userBattleID] = true;

        isBeginBattle = true;
        foreach (var item in dic_battleReady.Values) isBeginBattle = isBeginBattle && item;

        LogManage.Instance.AddLog(string.Format("{0} CheckBattleBegin {1}", _userBattleID, isBeginBattle));
        if (isBeginBattle)
            //开始战斗
            BeginBattle();
    }

    private void BeginBattle()
    {
        frameNum = 0;
        lastFrame = 0;
        _isRun = true;
        oneGameOver = false;
        allGameOver = false;

        var playerNum = dic_battleUserUid.Keys.Count;

        frameOperation = new PlayerOperation[playerNum];
        playerMesNum = new int[playerNum];
        playerGameOver = new bool[playerNum];
        for (var i = 0; i < playerNum; i++)
        {
            frameOperation[i] = null;
            playerMesNum[i] = 0;
            playerGameOver[i] = false;
        }

        var _threadSenfd = new Thread(Thread_SendFrameData);
        _threadSenfd.Start();
    }


    private void Thread_SendFrameData()
    {
        //向玩家发送战斗开始
        var isFinishBS = false;
        while (!isFinishBS)
        {
            var _btData = new UdpBattleStart();
            var _data = CSData.GetSendMessage(_btData, SCID.UDP_BATTLE_START);
            foreach (var item in dic_udp)
            {
                LogManage.Instance.AddLog(string.Format("send {0} SCID.UDP_BATTLE_START", item.Value.userUid));
                item.Value.SendMessage(_data);
            }

            var _allData = true;
            for (var i = 0; i < frameOperation.Length; i++)
                if (frameOperation[i] == null)
                {
                    _allData = false;
                    break;
                }

            if (_allData)
            {
                LogManage.Instance.AddLog("战斗服务器:收到全部玩家的第一次操作数据....");
                frameNum = 1;

                isFinishBS = true;
            }

            Thread.Sleep(500);
        }

        LogManage.Instance.AddLog("开始发送帧数据～～～～");

        while (_isRun)
        {
            var _dataPb = new UdpDownFrameOperations();
            if (oneGameOver)
            {
                _dataPb.frameID = lastFrame;
                _dataPb.operations = dic_gameOperation[lastFrame];
            }
            else
            {
                _dataPb.operations = new AllPlayerOperation();
                _dataPb.operations.operations.AddRange(frameOperation);
                _dataPb.frameID = frameNum;
                dic_gameOperation[frameNum] = _dataPb.operations;
                lastFrame = frameNum;
                frameNum++;
            }

            var _data = CSData.GetSendMessage(_dataPb, SCID.UDP_DOWN_FRAME_OPERATIONS);
            foreach (var item in dic_udp)
            {
                var _index = item.Key - 1;
                if (!playerGameOver[_index])
                {
                    item.Value.SendMessage(_data);
                }
            }

            Thread.Sleep(ServerConfig.frameTime);
        }

        LogManage.Instance.AddLog("帧数据发送线程结束.....................");
    }

    public void UpdatePlayerOperation(PlayerOperation _operation, int _mesNum)
    {
        //battleID 是房间玩家的唯一标识
        var _index = _operation.battleID - 1;
        // LogManage.Instance.AddLog("收到玩家操作:" + _index + "," + _mesNum + "," + playerMesNum[_index]);
        if (_mesNum > playerMesNum[_index])
        {
            frameOperation[_index] = _operation;
            playerMesNum[_index] = _mesNum;
        }
    }

    public void UpdatePlayerGameOver(int _battleId)
    {
        oneGameOver = true;

        var _index = _battleId - 1;
        playerGameOver[_index] = true;

        allGameOver = true;
        for (var i = 0; i < playerGameOver.Length; i++)
            if (playerGameOver[i] == false)
            {
                allGameOver = false;
                break;
            }

        if (allGameOver)
        {
            //			LogManage.Instance.AddLog ("战斗即将结束咯......");
            _isRun = false;

            finishTime = 2000f;
            if (waitBattleFinish == null) waitBattleFinish = new Timer(WaitClientFinish, null, 1000, 1000);
        }
    }

    private void WaitClientFinish(object snder)
    {
        //		LogManage.Instance.AddLog ("等待客户端结束～");
        finishTime -= 1000f;
        if (finishTime <= 0)
        {
            waitBattleFinish.Dispose();
            FinishBattle();
            //			LogManage.Instance.AddLog ("战斗结束咯......");
        }
    }
    
    public void AnalyzeMessage(CSID messageId, byte[] bodyData)
    {
        switch (messageId)
        {
            case CSID.UDP_BATTLE_READY:
            {
                //接收战斗准备
                var _mes = CSData.DeserializeData<UdpBattleReady>(bodyData);
                CheckBattleBegin(_mes.battleID);
                dic_udp[_mes.battleID].RecvClientReady(_mes.uid);
            }
                break;
            case CSID.UDP_UP_PLAYER_OPERATIONS:
            {
                var pb_ReceiveMes = CSData.DeserializeData<UdpUpPlayerOperations>(bodyData);
                UpdatePlayerOperation(pb_ReceiveMes.operation, pb_ReceiveMes.mesID);
            }
                break;
            case CSID.UDP_UP_DELTA_FRAMES:
            {
                var pb_ReceiveMes = CSData.DeserializeData<UdpUpDeltaFrames>(bodyData);

                var _downData = new UdpDownDeltaFrames();

                for (var i = 0; i < pb_ReceiveMes.frames.Count; i++)
                {
                    var framIndex = pb_ReceiveMes.frames[i];

                    var _downOp = new UdpDownFrameOperations();
                    _downOp.frameID = framIndex;
                    _downOp.operations = dic_gameOperation[framIndex];

                    _downData.framesData.Add(_downOp);
                }

                var _data = CSData.GetSendMessage(_downData, SCID.UDP_DOWN_DELTA_FRAMES);
                dic_udp[pb_ReceiveMes.battleID].SendMessage(_data);
            }
                break;
            case CSID.UDP_UP_GAME_OVER:
            {
                var pb_ReceiveMes = CSData.DeserializeData<UdpUpGameOver>(bodyData);
                UpdatePlayerGameOver(pb_ReceiveMes.battleID);

                var _downData = new UdpDownGameOver();
                var _data = CSData.GetSendMessage(_downData, SCID.UDP_DOWN_GAME_OVER);
                dic_udp[pb_ReceiveMes.battleID].SendMessage(_data);
            }
                break;
        }
    }
}