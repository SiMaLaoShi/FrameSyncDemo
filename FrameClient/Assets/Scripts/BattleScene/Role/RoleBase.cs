using UnityEngine;

[RequireComponent(typeof(ShapeCircle))]
public class RoleBase : MonoBehaviour
{
    public int moveSpeed;
    public int attackTime; //攻击时间间隔
    [HideInInspector] public ShapeBase objShape;

    private int curAttackTime;
    private GameVector2 logicSpeed;

//	private GameObject uiObj;
    private Transform modleParent;
    private Quaternion renderDir;

    private Vector3 renderPosition;
    private int roleDirection; //角色朝向

    private void Update()
    {
        transform.position = Vector3.Lerp(transform.position, renderPosition, 0.4f);
        modleParent.rotation = Quaternion.Lerp(modleParent.rotation, renderDir, 0.2f);
    }

    public void InitData(GameObject _ui, GameObject _modle, int _roleID, GameVector2 _logicPos)
    {
        objShape = GetComponent<ShapeBase>();
        logicSpeed = GameVector2.zero;
        modleParent = transform.Find("Modle");

//		uiObj = _ui;

        _modle.transform.SetParent(modleParent);
        _modle.transform.localPosition = new Vector3(0, 0.5f, 0);

        objShape.InitSelf(ObjectType.role, _roleID);
        objShape.SetPosition(_logicPos);
        renderPosition = objShape.GetPositionVec3();
        transform.position = renderPosition;

        roleDirection = 0;
        renderDir = Quaternion.LookRotation(new Vector3(1f, 0f, 0f));
        modleParent.rotation = renderDir;

        curAttackTime = 0;
    }

    public bool IsCloudAttack()
    {
        return curAttackTime <= 0;
    }

    public virtual void Logic_UpdateMoveDir(int _dir)
    {
        if (_dir > 120)
        {
            logicSpeed = GameVector2.zero;
        }
        else
        {
            roleDirection = _dir * 3;
            logicSpeed = moveSpeed * BattleData.Instance.GetSpeed(roleDirection);
            var _renderDir = ToolGameVector.ChangeGameVectorToVector3(logicSpeed);
            renderDir = Quaternion.LookRotation(_renderDir);
        }
    }

    public virtual void Logic_NormalAttack()
    {
        curAttackTime = attackTime;

        var _bulletPos = objShape.GetPosition() +
                         ToolMethod.Logic2Config(objShape.GetRadius()) *
                         BattleData.Instance.GetSpeed(roleDirection);
        BattleCon.Instance.bulletManage.AddBullet(objShape.ObjUid.objectID, _bulletPos, roleDirection);
    }


    public virtual void Logic_Move()
    {
        if (logicSpeed != GameVector2.zero)
        {
            var _targetPos = objShape.GetPosition() + logicSpeed;
            UpdateLogicPosition(_targetPos);
            renderPosition = objShape.GetPositionVec3();
        }

        if (curAttackTime > 0) curAttackTime--;
    }

    public virtual void Logic_Move_Correction()
    {
        GameVector2 _ccLogicPos;
        if (BattleCon.Instance.obstacleManage.CollisionCorrection(objShape.GetPosition(), objShape.GetRadius(),
                out _ccLogicPos))
            UpdateLogicPosition(_ccLogicPos);
    }

    private void UpdateLogicPosition(GameVector2 _logicPos)
    {
        objShape.SetPosition(BattleData.Instance.GetMapLogicPosition(_logicPos));
    }
}