using System.Collections.Generic;
using BattleScene.Replay;
using PBBattle;
using PBCommon;
using UnityEngine;

public class UdpPB
{
    //返回给游戏的delegate
    public delegate void DelegateReceiveMessage<T>(T message);

    private static UdpPB singleInstance;
    private MarsUdp _upd;
    private int mesNum;

    private UdpPB()
    {
    }

    public DelegateReceiveMessage<UdpBattleStart> mes_battle_start { get; set; }
    public DelegateReceiveMessage<UdpDownFrameOperations> mes_frame_operation { get; set; }
    public DelegateReceiveMessage<UdpDownDeltaFrames> mes_delta_frame_data { get; set; }
    public DelegateReceiveMessage<UdpDownGameOver> mes_down_game_over { get; set; }

    public static UdpPB Instance()
    {
        // 如果类的实例不存在则创建，否则直接返回
        if (singleInstance == null) singleInstance = new UdpPB();
        return singleInstance;
    }

    public void Destory()
    {
        singleInstance = null;

        if (_upd != null)
        {
            _upd.EndClientUdp();
            _upd = null;
        }

        mes_frame_operation = null;
        mes_delta_frame_data = null;
        mes_down_game_over = null;
    }


    public void StartClientUdp()
    {
        mesNum = 0;
        _upd = new MarsUdp();
        _upd.StartClientUdp(NetGlobal.Instance().serverIP);

        _upd.delegate_analyze_message = AnalyzeMessage;
    }


    public void SendBattleReady(int _uid, int _battleID)
    {
        var _ready = new UdpBattleReady();
        _ready.uid = _uid;
        _ready.battleID = _battleID;
        _upd.SendMessage(CSData.GetSendMessage(_ready, CSID.UDP_BATTLE_READY));
    }


    public void SendOperation()
    {
        mesNum++;

        var _up = new UdpUpPlayerOperations();
        _up.mesID = mesNum;

        _up.operation = BattleData.Instance.selfOperation;

//		_up.operation = new PlayerOperation ();
//		_up.operation.battleID = BattleData.Instance.selfOperation.battleID;
//		_up.operation.move = BattleData.Instance.selfOperation.move;
//
//		if (BattleData.Instance.selfOperation.rightOperation != RightOpType.noop) {
//			_up.operation.operationID = BattleData.Instance.selfOperation.operationID;
//			_up.operation.rightOperation = BattleData.Instance.selfOperation.rightOperation;
//			_up.operation.operationValue1 = BattleData.Instance.selfOperation.operationValue1;
//			_up.operation.operationValue2 = BattleData.Instance.selfOperation.operationValue2;
//		}

        _upd.SendMessage(CSData.GetSendMessage(_up, CSID.UDP_UP_PLAYER_OPERATIONS));
    }


    public PlayerOperation ClonePlayerOperation()
    {
        var _operation = new PlayerOperation();
        _operation.battleID = BattleData.Instance.selfOperation.battleID;
        _operation.move = BattleData.Instance.selfOperation.move;
        _operation.operationID = BattleData.Instance.selfOperation.operationID;
        _operation.rightOperation = BattleData.Instance.selfOperation.rightOperation;
        _operation.operationValue1 = BattleData.Instance.selfOperation.operationValue1;
        _operation.operationValue2 = BattleData.Instance.selfOperation.operationValue2;
        return _operation;
    }

    public void SendDeltaFrames(int _battleID, List<int> _frames)
    {
        var _framespb = new UdpUpDeltaFrames();

        _framespb.battleID = _battleID;
        _framespb.frames.AddRange(_frames);

        _upd.SendMessage(CSData.GetSendMessage(_framespb, CSID.UDP_UP_DELTA_FRAMES));
    }


    public void SendGameOver(int _battleID)
    {
        var _gameOver = new UdpUpGameOver();
        _gameOver.battleID = _battleID;
        _upd.SendMessage(CSData.GetSendMessage(_gameOver, CSID.UDP_UP_GAME_OVER));
    }


    public void AnalyzeMessage(SCID messageId, byte[] bodyData)
    {
        switch (messageId)
        {
            case SCID.UDP_BATTLE_START:
            {
                Debug.Log("Recv UDP_BATTLE_START");
                var pb_ReceiveMes = CSData.DeserializeData<UdpBattleStart>(bodyData);
                ReplaySystem.Instance.AddFrame(BattleData.Instance.FrameId, bodyData, SCID.UDP_BATTLE_START);
                NetGlobal.Instance().AddAction(() => { mes_battle_start(pb_ReceiveMes); });
            }
                break;
            case SCID.UDP_DOWN_FRAME_OPERATIONS:
            {
                var pb_ReceiveMes = CSData.DeserializeData<UdpDownFrameOperations>(bodyData);
                ReplaySystem.Instance.AddFrame(BattleData.Instance.FrameId, bodyData, SCID.UDP_DOWN_FRAME_OPERATIONS);
                NetGlobal.Instance().AddAction(() => { mes_frame_operation(pb_ReceiveMes); });
            }
                break;
            case SCID.UDP_DOWN_DELTA_FRAMES:
            {
                var pb_ReceiveMes = CSData.DeserializeData<UdpDownDeltaFrames>(bodyData);
                ReplaySystem.Instance.AddFrame(BattleData.Instance.FrameId, bodyData, SCID.UDP_DOWN_DELTA_FRAMES);
                NetGlobal.Instance().AddAction(() => { mes_delta_frame_data(pb_ReceiveMes); });
            }
                break;
            case SCID.UDP_DOWN_GAME_OVER:
            {
                var pb_ReceiveMes = CSData.DeserializeData<UdpDownGameOver>(bodyData);
                ReplaySystem.Instance.AddFrame(BattleData.Instance.FrameId, bodyData, SCID.UDP_DOWN_GAME_OVER);
                NetGlobal.Instance().AddAction(() => { mes_down_game_over(pb_ReceiveMes); });
            }
                break;
        }
    }
}