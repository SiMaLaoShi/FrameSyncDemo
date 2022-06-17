using PBCommon;
using PBLogin;
using PBMatch;

public class TcpPB
{
    private static TcpPB singleInstance;

    private TcpPB()
    {
    }

    public static TcpPB Instance()
    {
        // 如果类的实例不存在则创建，否则直接返回
        if (singleInstance == null) singleInstance = new TcpPB();
        return singleInstance;
    }

    public void Destory()
    {
        singleInstance = null;
    }


    public void AnalyzeMessage(CSID messageId, byte[] bodyData, string _socketIp)
    {
        switch (messageId)
        {
            case CSID.TCP_REQUEST_MATCH:
            {
                var _mes = CSData.DeserializeData<TcpRequestMatch>(bodyData);
                LogManage.Instance.AddLog(string.Format("add user match {0} {1}", _mes.uid, _mes.roleID));
                MatchManage.Instance.NewMatchUser(_mes.uid, _mes.roleID);

                var rmRes = new TcpResponseRequestMatch();
                ServerTcp.Instance.SendMessage(_mes.uid,
                    CSData.GetSendMessage(rmRes, SCID.TCP_RESPONSE_REQUEST_MATCH));
            }
                break;

            case CSID.TCP_CANCEL_MATCH:
            {
                var _mes = CSData.DeserializeData<TcpCancelMatch>(bodyData);
                MatchManage.Instance.CancleMatch(_mes.uid);

                var cmRes = new TcpResponseCancelMatch();
                ServerTcp.Instance.SendMessage(_mes.uid, CSData.GetSendMessage(cmRes, SCID.TCP_RESPONSE_CANCEL_MATCH));
            }
                break;
        }
    }
}