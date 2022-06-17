using PBBattle;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MarsClickBtn : MonoBehaviour
{
    private EventTrigger _EventTri;
    private Image btnImage;

    private void Start()
    {
        _EventTri = GetComponent<EventTrigger>();
        btnImage = GetComponent<Image>();
    }

    public void EnableButton()
    {
        _EventTri.enabled = true;
        btnImage.raycastTarget = true;
    }

    public void DisableButton()
    {
        _EventTri.enabled = false;
        btnImage.raycastTarget = true;
    }

    public void OnClickDown()
    {
        btnImage.color = Color.gray;

        if (gameObject.CompareTag("NormalAttackButton"))
        {
            //普通攻击
            var _role = BattleCon.Instance.roleManage.GetRoleFromBattleID(BattleData.Instance.battleID);
            if (_role.IsCloudAttack()) BattleData.Instance.UpdateRightOperation(RightOpType.rop1, 0, 0);
        }
        else if (gameObject.CompareTag("BtnGameOver"))
        {
            BattleCon.Instance.OnClickGameOver();
        }
    }

    public void OnClickUp()
    {
        btnImage.color = Color.white;
    }
}