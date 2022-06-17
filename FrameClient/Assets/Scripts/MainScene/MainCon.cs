using PBBattle;
using PBCommon;
using PBMatch;
using UnityEngine;

public class MainCon : MonoBehaviour
{
    public GameObject waitMatchObj;

    private void Start()
    {
        waitMatchObj.SetActive(false);

        TcpPB.Instance().mes_request_match_result += Message_Reauest_Match_Result;
        TcpPB.Instance().mes_cancel_match_result += Message_Cancel_Match_Result;
        TcpPB.Instance().mes_enter_battle += Message_Enter_Battle;
    }


    private void OnDestroy()
    {
        TcpPB.Instance().mes_request_match_result -= Message_Reauest_Match_Result;
        TcpPB.Instance().mes_cancel_match_result -= Message_Cancel_Match_Result;
        TcpPB.Instance().mes_enter_battle -= Message_Enter_Battle;
    }

    public void OnClickMatch()
    {
        //开始匹配
//		SceneManager.LoadScene (GameConfig.battleScene);
        var _mes = new TcpRequestMatch();
        _mes.uid = NetGlobal.Instance().userUid;
        _mes.roleID = 2;
        MarsTcp.Instance.SendMessage(CSData.GetSendMessage(_mes, CSID.TCP_REQUEST_MATCH));
    }

    public void OnCliclCancelMatch()
    {
        //取消匹配
        var _mes = new TcpCancelMatch();
        _mes.uid = NetGlobal.Instance().userUid;
        MarsTcp.Instance.SendMessage(CSData.GetSendMessage(_mes, CSID.TCP_CANCEL_MATCH));
    }

    private void Message_Reauest_Match_Result(TcpResponseRequestMatch _result)
    {
        waitMatchObj.SetActive(true);
    }

    private void Message_Cancel_Match_Result(TcpResponseCancelMatch _result)
    {
        waitMatchObj.SetActive(false);
    }

    private void Message_Enter_Battle(TcpEnterBattle _mes)
    {
        BattleData.Instance.UpdateBattleInfo(_mes.randSeed, _mes.battleUserInfo);

        ClearSceneData.LoadScene(GameConfig.battleScene);
    }
}