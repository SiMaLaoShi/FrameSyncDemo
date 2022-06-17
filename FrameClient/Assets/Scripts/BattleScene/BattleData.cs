using System.Collections.Generic;
using PBBattle;
using UnityEngine;

public class BattleData
{
    public const int mapRow = 7; //行数
    public const int mapColumn = 13; //列
    public const int gridLenth = 10000; //格子的逻辑大小
    public const int gridHalfLenth = 5000; //格子的逻辑大小

    private static BattleData instance;
    public int battleID;

    private int curFramID;

    private int curOperationID;
    private readonly Dictionary<int, AllPlayerOperation> dic_frameDate;
    private readonly Dictionary<int, int> dic_rightOperationID;
    private readonly Dictionary<int, GameVector2> dic_speed;


    //一些统计数据
    public int fps;

    private readonly List<int> lackFrame;

    public List<BattleUserInfo> list_battleUser;
    public int mapHeigh;
    public int mapTotalGrid;
    public int mapWidth;
    private int maxFrameID;
    private int maxSendNum;
    public int netPack;
    public int randSeed; //随机种子
    public int recvNum;
    public PlayerOperation selfOperation;
    public int sendNum;

    private BattleData()
    {
        mapTotalGrid = mapRow * mapColumn;
        mapWidth = mapColumn * gridLenth;
        mapHeigh = mapRow * gridLenth;

        curOperationID = 1;
        selfOperation = new PlayerOperation();
        selfOperation.move = 121;
        ResetRightOperation();

        dic_speed = new Dictionary<int, GameVector2>();
        //初始化速度表
        GlobalData.Instance()
            .GetFileStringFromStreamingAssets("Desktopspeed.txt", _fileStr => { InitSpeedInfo(_fileStr); });

        curFramID = 0;
        maxFrameID = 0;
        maxSendNum = 5;

        lackFrame = new List<int>();
        dic_rightOperationID = new Dictionary<int, int>();
        dic_frameDate = new Dictionary<int, AllPlayerOperation>();
    }

    public static BattleData Instance
    {
        get
        {
            // 如果类的实例不存在则创建，否则直接返回
            if (instance == null) instance = new BattleData();

            return instance;
        }
    }

    public void UpdateBattleInfo(int _randseed, List<BattleUserInfo> _userInfo)
    {
        randSeed = _randseed;
        list_battleUser = new List<BattleUserInfo>(_userInfo);

        foreach (var item in list_battleUser)
        {
            if (item.uid == NetGlobal.Instance().userUid)
            {
                battleID = item.battleID;
                selfOperation.battleID = battleID;
                Debug.Log("自己的战斗id:" + battleID);
            }

            dic_rightOperationID[item.battleID] = 0;
        }
    }

    public void ClearData()
    {
        curOperationID = 1;
        selfOperation.move = 121;
        ResetRightOperation();

        curFramID = 0;
        maxFrameID = 0;
        maxSendNum = 5;

        lackFrame.Clear();
        dic_rightOperationID.Clear();
        dic_frameDate.Clear();
    }

    public void SaveFrameOp()
    {
        foreach (var operation in dic_frameDate)
        {
            
        }
    }


    public void Destory()
    {
        list_battleUser.Clear();
        list_battleUser = null;
        instance = null;
    }

    private void InitSpeedInfo(string _fileStr)
    {
        var lineArray = _fileStr.Split("\n"[0]);

        int dir;
        for (var i = 0; i < lineArray.Length; i++)
            if (lineArray[i] != "")
            {
                var date = new GameVector2();
                var line = lineArray[i].Split(new char[1] { ',' }, 3);
                dir = int.Parse(line[0]);
                date.x = int.Parse(line[1]);
                date.y = int.Parse(line[2]);
                dic_speed[dir] = date;
            }
    }

    public GameVector2 GetSpeed(int _dir)
    {
        return dic_speed[_dir];
    }

    //坐标不超出地图
    public GameVector2 GetMapLogicPosition(GameVector2 _pos)
    {
        return new GameVector2(Mathf.Clamp(_pos.x, 0, mapWidth), Mathf.Clamp(_pos.y, 0, mapHeigh));
    }

    public GameVector2 GetMapGridCenterPosition(int _row, int _column)
    {
        return new GameVector2(_column * gridLenth + gridHalfLenth, _row * gridLenth + gridHalfLenth);
    }

    public GameVector2 GetMapGridFromRand(int _randNum)
    {
        var _num1 = _randNum % mapTotalGrid;
        var _row = _num1 / mapColumn;
        var _column = _num1 % mapColumn;
        return new GameVector2(_row, _column);
    }

    public GameVector2 GetMapGridCenterPositionFromRand(int _randNum)
    {
        var grid = GetMapGridFromRand(_randNum);
        return GetMapGridCenterPosition(grid.x, grid.y);
    }


    public void UpdateMoveDir(int _dir)
    {
        selfOperation.move = _dir;
    }

    public void UpdateRightOperation(RightOpType _type, int _value1, int _value2)
    {
        selfOperation.rightOperation = _type;
        selfOperation.operationValue1 = _value1;
        selfOperation.operationValue2 = _value2;
        selfOperation.operationID = curOperationID;
    }

    public bool IsValidRightOp(int _battleID, int _rightOpID)
    {
        return _rightOpID > dic_rightOperationID[_battleID];
    }

    public void UpdateRightOperationID(int _battleID, int _opID, RightOpType _type)
    {
        dic_rightOperationID[_battleID] = _opID;
        if (battleID == _battleID)
        {
            //玩家自己
            curOperationID++;
            if (_type == selfOperation.rightOperation) ResetRightOperation();
        }
    }

    public void ResetRightOperation()
    {
        selfOperation.rightOperation = RightOpType.noop;
        selfOperation.operationValue1 = 0;
        selfOperation.operationValue2 = 0;
        selfOperation.operationID = 0;
    }

    public int GetFrameDataNum()
    {
        if (dic_frameDate == null)
            return 0;
        return dic_frameDate.Count;
    }

    public void AddNewFrameData(int _frameID, AllPlayerOperation _op)
    {
        dic_frameDate[_frameID] = _op;
        for (var i = maxFrameID + 1; i < _frameID; i++)
        {
            lackFrame.Add(i);
            Debug.Log("缺失 :" + i);
        }

        maxFrameID = _frameID;

        //发送缺失帧数据
        if (lackFrame.Count > 0)
        {
            if (lackFrame.Count > maxSendNum)
            {
                var sendList = lackFrame.GetRange(0, maxSendNum);
                UdpPB.Instance().SendDeltaFrames(selfOperation.battleID, sendList);
            }
            else
            {
                UdpPB.Instance().SendDeltaFrames(selfOperation.battleID, lackFrame);
            }
        }
    }

    public void AddLackFrameData(int _frameID, AllPlayerOperation _newOp)
    {
        //删除缺失的帧记录
        if (lackFrame.Contains(_frameID))
        {
            dic_frameDate[_frameID] = _newOp;
            lackFrame.Remove(_frameID);
            Debug.Log("补上 :" + _frameID);
        }
    }

    public bool TryGetNextPlayerOp(out AllPlayerOperation _op)
    {
        var _frameID = curFramID + 1;
        return dic_frameDate.TryGetValue(_frameID, out _op);
    }

    public void RunOpSucces()
    {
        curFramID++;
    }
}