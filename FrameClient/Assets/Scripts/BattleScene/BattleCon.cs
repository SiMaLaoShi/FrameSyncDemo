using System.Collections;
using System.Collections.Generic;
using System.IO;
using PBBattle;
using UnityEngine;

public class BattleCon : MonoBehaviour
{
    public delegate void DelegateEvent();

    [HideInInspector] public RoleManage roleManage;
    [HideInInspector] public ObstacleManage obstacleManage;
    [HideInInspector] public BulletManage bulletManage;
    public DelegateEvent delegate_gameOver;

    public DelegateEvent delegate_readyOver;
    private bool isBattleFinish;

    private bool isBattleStart;

    public bool IsReplay { get; set; }

    public static BattleCon Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        UdpPB.Instance().StartClientUdp();
        UdpPB.Instance().mes_battle_start += Message_Battle_Start;
        UdpPB.Instance().mes_frame_operation += Message_Frame_Operation;
        UdpPB.Instance().mes_delta_frame_data += Message_Delta_Frame_Data;
        UdpPB.Instance().mes_down_game_over += Message_Down_Game_Over;

        isBattleStart = false;
        StartCoroutine("WaitInitData");
    }


    private void OnDestroy()
    {
        BattleData.Instance.ClearData();
        UdpPB.Instance().mes_battle_start -= Message_Battle_Start;
        UdpPB.Instance().mes_frame_operation -= Message_Frame_Operation;
        UdpPB.Instance().mes_delta_frame_data -= Message_Delta_Frame_Data;
        UdpPB.Instance().mes_down_game_over -= Message_Down_Game_Over;
        UdpPB.Instance().Destory();
        Instance = null;
    }

    private IEnumerator WaitInitData()
    {
        yield return new WaitUntil(() =>
        {
            return roleManage.initFinish && obstacleManage.initFinish && bulletManage.initFinish;
        });
        InvokeRepeating("Send_BattleReady", 0.5f, 0.2f);
    }

    public void InitData(Transform _map)
    {
        ToolRandom.srand((ulong)BattleData.Instance.randSeed);
        roleManage = gameObject.AddComponent<RoleManage>();
        obstacleManage = gameObject.AddComponent<ObstacleManage>();
        bulletManage = gameObject.AddComponent<BulletManage>();

        GameVector2[] roleGrid;
        roleManage.InitData(_map.Find("Role"), out roleGrid);
        obstacleManage.InitData(_map.Find("Obstacle"), roleGrid);
        bulletManage.InitData(_map.Find("Bullet"));
    }

    private void Send_BattleReady()
    {
        UdpPB.Instance().SendBattleReady(NetGlobal.Instance().userUid, BattleData.Instance.battleID);
    }

    private void Message_Battle_Start(UdpBattleStart _mes)
    {
        BattleStart();
    }

    private void BattleStart()
    {
        if (isBattleStart) return;

        isBattleStart = true;
        CancelInvoke("Send_BattleReady");

        var _time = NetConfig.frameTime * 0.001f;
        InvokeRepeating("Send_operation", _time, _time);

        StartCoroutine("WaitForFirstMessage");
    }

    private void Send_operation()
    {
        UdpPB.Instance().SendOperation();
    }

    private IEnumerator WaitForFirstMessage()
    {
        yield return new WaitUntil(() => { return BattleData.Instance.GetFrameDataNum() > 0; });
        InvokeRepeating("LogicUpdate", 0f, 0.020f);

        if (delegate_readyOver != null) delegate_readyOver();
    }

    private Dictionary<int, byte[]> frames = new Dictionary<int, byte[]>();

    private void Message_Frame_Operation(UdpDownFrameOperations _mes)
    {
        frames.Add(_mes.frameID, CSData.SerializeData(_mes));
        BattleData.Instance.AddNewFrameData(_mes.frameID, _mes.operations);
        BattleData.Instance.netPack++;
    }
    
    public void SaveReplay()
    {
        var fs = File.Create(Path.Combine(System.Environment.CurrentDirectory, "BattleReplay.bytes"));
        var offset = 0;
        foreach (var frame in frames)
        {
            fs.Write(frame.Value, offset, frame.Value.Length);
            offset += frame.Value.Length;
        }   
        fs.Close();
    }

    //逻辑帧更新
    private void LogicUpdate()
    {
        AllPlayerOperation _op;
        if (BattleData.Instance.TryGetNextPlayerOp(out _op))
        {
            roleManage.Logic_Operation(_op);
            roleManage.Logic_Move();
            bulletManage.Logic_Move();
            bulletManage.Logic_Collision();
            roleManage.Logic_Move_Correction();
            obstacleManage.Logic_Destory();
            bulletManage.Logic_Destory();
            BattleData.Instance.RunOpSucces();
        }
    }

    private void Message_Delta_Frame_Data(UdpDownDeltaFrames _mes)
    {
        if (_mes.framesData.Count > 0)
            foreach (var item in _mes.framesData)
                BattleData.Instance.AddLackFrameData(item.frameID, item.operations);
    }

    public void OnClickGameOver()
    {
        SaveReplay();
        BeginGameOver();
    }

    private void BeginGameOver()
    {
        CancelInvoke("Send_operation");
        InvokeRepeating("SendGameOver", 0f, 0.5f);
    }

    private void SendGameOver()
    {
        UdpPB.Instance().SendGameOver(BattleData.Instance.battleID);
    }

    private void Message_Down_Game_Over(UdpDownGameOver _mes)
    {
        CancelInvoke("SendGameOver");
        Debug.Log("游戏结束咯～～～～～～");
        if (delegate_gameOver != null) delegate_gameOver();
    }
}