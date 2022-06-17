using System.Collections;
using System.Collections.Generic;
using PBBattle;
using UnityEngine;

public class RoleManage : MonoBehaviour
{
    public bool initFinish;
    private Dictionary<int, RoleBase> dic_role;
    private GameObject pre_roleBase;
    private GameObject pre_roleUI;

    private Transform roleParent;

    private void OnDestroy()
    {
    }
//	void Start () {
//		
//	}

    public void InitData(Transform _roleParent, out GameVector2[] roleGrid)
    {
        initFinish = false;
        roleParent = _roleParent;

        dic_role = new Dictionary<int, RoleBase>();

        pre_roleBase = Resources.Load<GameObject>("BattleScene/Role/RoleBase");
        pre_roleUI = Resources.Load<GameObject>("BattleScene/Role/RoleUI");


        var _roleNum = BattleData.Instance.list_battleUser.Count;
        roleGrid = new GameVector2[_roleNum];
        for (var i = 0; i < roleGrid.Length; i++)
            roleGrid[i] = BattleData.Instance.GetMapGridFromRand(ToolRandom.rand_10000());

        StartCoroutine(CreatRole(roleGrid));
    }

    private IEnumerator CreatRole(GameVector2[] _roleGrid)
    {
        var pre_roleModle = new Dictionary<string, GameObject>();
        var list_battleUser = BattleData.Instance.list_battleUser;

        for (var i = 0; i < list_battleUser.Count; i++)
        {
            yield return new WaitForEndOfFrame();
            var _info = list_battleUser[i];

            var _base = Instantiate(pre_roleBase, roleParent);
            var _ui = Instantiate(pre_roleUI, _base.transform);

            //			string _modleStr = string.Format("BattleScene/Role/RoleModel{0}",_info.roleID);
            var _modleStr = "BattleScene/Role/RoleModel";
            if (!pre_roleModle.ContainsKey(_modleStr)) pre_roleModle[_modleStr] = Resources.Load<GameObject>(_modleStr);
            var _modle = Instantiate(pre_roleModle[_modleStr]);

            var _grid = _roleGrid[_info.battleID - 1];
            var _pos = BattleData.Instance.GetMapGridCenterPosition(_grid.x, _grid.y);

            var _roleCon = _base.GetComponent<RoleBase>();
            _roleCon.InitData(_ui, _modle, _info.battleID, _pos);
            dic_role[_info.battleID] = _roleCon;
        }

        initFinish = true;
    }

    public RoleBase GetRoleFromBattleID(int _id)
    {
        return dic_role[_id];
    }

    public void Logic_Operation(AllPlayerOperation _allOp)
    {
        foreach (var item in _allOp.operations)
        {
            dic_role[item.battleID].Logic_UpdateMoveDir(item.move);
            if (item.rightOperation == RightOpType.noop || item.operationID == 0)
            {
                //无操作
            }
            else
            {
                if (BattleData.Instance.IsValidRightOp(item.battleID, item.operationID))
                {
                    //操作有效
                    switch (item.rightOperation)
                    {
                        case RightOpType.rop1:
                        {
                            dic_role[item.battleID].Logic_NormalAttack();
                        }
                            break;
                    }

                    BattleData.Instance.UpdateRightOperationID(item.battleID, item.operationID, item.rightOperation);
                }
            }
        }
    }

    public void Logic_Move()
    {
        foreach (var item in dic_role) item.Value.Logic_Move();
    }

    public void Logic_Move_Correction()
    {
        foreach (var item in dic_role) item.Value.Logic_Move_Correction();
    }
}