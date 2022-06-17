using System;
using System.Collections;
using System.IO;
using UnityEngine;

public class ClassForUpdate : MonoBehaviour
{
    public void GetFileStringFromStreamingAssets(string _fileName, Action<string> _action)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
		StartCoroutine (GetFileStringForAndroid(_fileName,_action));
#elif UNITY_IPHONE && !UNITY_EDITOR
		GetFileStringForIos(_fileName,_action);
#else
        GetFileString(_fileName, _action);
#endif
    }

    private IEnumerator GetFileStringForAndroid(string _path, Action<string> _action)
    {
        var wwwCar = new WWW(_path);
        yield return wwwCar;
        _action(wwwCar.text);
    }

    private void GetFileStringForIos(string _path, Action<string> _action)
    {
        if (File.Exists(_path))
            try
            {
                var sr = File.OpenText(_path);
                _action(sr.ReadToEnd());
                sr.Close();
                sr.Dispose();
            }
            catch
            {
                Debug.Log("_path_car出错咯～");
            }
    }

    private void GetFileString(string _path, Action<string> _action)
    {
        if (File.Exists(_path))
            try
            {
                //实例化文件流，参数1 路径，参数2文件操作方式  
                var file = new FileStream(_path, FileMode.Open);
                var sr = new StreamReader(file);
                _action(sr.ReadToEnd());
                sr.Close(); //关闭流释放空间  
                file.Close();
                sr.Dispose();
            }
            catch
            {
                Debug.Log("文件出错咯:" + _path);
            }
    }
}