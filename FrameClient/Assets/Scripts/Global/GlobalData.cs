using System;
using System.Collections;
using System.IO;
using UnityEngine;
using Object = UnityEngine.Object;
//using System.Threading;

public class GlobalData
{
    private static GlobalData instance;

    private readonly ClassForUpdate classForUpdate;

    //StreamingAssets文件夹路径
    public string m_sStreamingAssetsPath;

    //assetbundle对应的平台后缀
    public string m_strABExtra;

    public GlobalData()
    {
        var obj = new GameObject("GlobalObj");
        classForUpdate = obj.AddComponent<ClassForUpdate>();
        Object.DontDestroyOnLoad(obj);

#if UNITY_ANDROID && !UNITY_EDITOR
		m_sStreamingAssetsPath = "jar:file://" + Application.dataPath + "!/assets";
#elif UNITY_IPHONE && !UNITY_EDITOR
		m_sStreamingAssetsPath = Application.streamingAssetsPath;
#else
        m_sStreamingAssetsPath = Application.streamingAssetsPath;
#endif

#if UNITY_ANDROID
		m_strABExtra = "_android";
#elif UNITY_IOS
		m_strABExtra = "_ios";
#elif UNITY_STANDALONE_OSX
		m_strABExtra = "_mac";
#elif UNITY_STANDALONE_WIN
        m_strABExtra = "_win";
#else
		Debug.Log("没有ab文件的平台啊～～～～");
#endif
    }

    public static GlobalData Instance()
    {
        if (instance == null) instance = new GlobalData();
        return instance;
    }

    public void Destory()
    {
        instance = null;
    }

    public void SetScreenResolution(int _width, int _heigh)
    {
//		#if (UNITY_ANDROID || UNITY_IPHONE) && !UNITY_EDITOR
//		Screen.SetResolution (_width,_heigh,true);
//		scaleScreen =  originScreenSize.y / _heigh;
//		#endif  
    }

    public bool IsChinese()
    {
        return Application.systemLanguage == SystemLanguage.Chinese;
    }

    public string GetABPath(string _file)
    {
        return m_sStreamingAssetsPath + "/AssetBundle/" + _file + m_strABExtra;
    }

    //读取StreamingAssets下的文件
    public void GetFileStringFromStreamingAssets(string _fileName, Action<string> _action)
    {
        var fullPath = m_sStreamingAssetsPath + "/" + _fileName;
        classForUpdate.GetFileStringFromStreamingAssets(fullPath, _action);
    }
}