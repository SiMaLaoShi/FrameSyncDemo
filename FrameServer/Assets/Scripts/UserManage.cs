using System.Collections.Generic;

public struct UserInfo
{
    public string socketIp;
    public bool isLogin;

    public UserInfo(string _socketIp, bool _isLogin)
    {
        socketIp = _socketIp;
        isLogin = _isLogin;
    }
}

public class UserManage
{
    private static readonly object umlockObj = new object();
    private static UserManage instance;
    private readonly Dictionary<string, int> dic_tokenUid;
    private readonly Dictionary<int, UserInfo> dic_userInfo;

    private int userUid;

    private UserManage()
    {
        userUid = 0;
        dic_userInfo = new Dictionary<int, UserInfo>();
        dic_tokenUid = new Dictionary<string, int>();
    }

    public static UserManage Instance
    {
        get
        {
            lock (umlockObj)
            {
                if (instance == null) instance = new UserManage();
            }

            return instance;
        }
    }

    public void Creat()
    {
    }


    public void Destroy()
    {
        instance = null;
    }


    public int UserLogin(string _token, string _socketIp)
    {
        // int _uid;
        // if (dic_tokenUid.ContainsKey(_token))
        // {
        //     _uid = dic_tokenUid[_token];
        // }
        // else
        // {
        //     userUid++;
        //     _uid = userUid;
        //     dic_tokenUid[_token] = userUid;
        // }
        int uid = userUid++;

        LogManage.Instance.AddLog(string.Format("userLogin {0}, {1}", _token, _socketIp));
        var _userInfo = new UserInfo(_socketIp, true);
        dic_userInfo[uid] = _userInfo;

        return uid;
    }

    public void UserLogout(string _socketIp)
    {
        //掉线
    }

    public void UserLogout(int _uid)
    {
        //自己登出
    }

    public UserInfo GetUserInfo(int _uid)
    {
        return dic_userInfo[_uid];
    }
}