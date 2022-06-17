﻿using System.Collections.Generic;

public class BattleManage
{
    private static BattleManage instance;

    private int battleID;
    private readonly Dictionary<int, BattleCon> dic_battles;

    private BattleManage()
    {
        battleID = 0;
        dic_battles = new Dictionary<int, BattleCon>();
    }

    public static BattleManage Instance
    {
        get
        {
            if (instance == null) instance = new BattleManage();
            return instance;
        }
    }

    public void Creat()
    {
    }


    public void Destroy()
    {
        foreach (var item in dic_battles) item.Value.DestroyBattle();

        dic_battles.Clear();
        instance = null;
    }

    public void BeginBattle(List<MatchUserInfo> _battleUser)
    {
        battleID++;
        var _battle = new BattleCon();
        _battle.CreatBattle(battleID, _battleUser);

        dic_battles[battleID] = _battle;

        LogManage.Instance.AddLog("开始战斗。。。。。" + battleID);
    }


    public void FinishBattle(int _battleID)
    {
        dic_battles.Remove(_battleID);
        LogManage.Instance.AddLog("战斗结束。。。。。" + _battleID);
    }
}