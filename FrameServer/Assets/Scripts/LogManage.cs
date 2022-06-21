using System;
using System.Reflection;
using UnityEditor;
using UnityEditor.Callbacks;

public enum CLogType
{
    /// <summary>
    ///   <para>LogType used for Errors.</para>
    /// </summary>
    Error,
    /// <summary>
    ///   <para>LogType used for Asserts. (These could also indicate an error inside Unity itself.)</para>
    /// </summary>
    Assert,
    /// <summary>
    ///   <para>LogType used for Warnings.</para>
    /// </summary>
    Warning,
    /// <summary>
    ///   <para>LogType used for regular log messages.</para>
    /// </summary>
    Log,
    /// <summary>
    ///   <para>LogType used for Exceptions.</para>
    /// </summary>
    Exception,
}

public class LogManage
{
    public delegate void DelegateLogChange(string _str, CLogType logType);

    private static LogManage instance;
    public DelegateLogChange logChange;

    private LogManage()
    {
    }

    public static LogManage Instance
    {
        get
        {
            if (instance == null) instance = new LogManage();
            return instance;
        }
    }

    public void Destory()
    {
        logChange = null;
        instance = null;
    }

    public void AddLog(string _str, CLogType logType = CLogType.Log)
    {
        if (logChange != null) logChange(_str, logType);
    }
}
#if UNITY_EDITOR
    //Log重定向
    //http://dsqiu.iteye.com/blog/2263664
    //https://blog.csdn.net/suifcd/article/details/72553678
    public class DebugRedirect
    {
        const string logCSName = "Debuger.cs";

        static object logListView;
        static object logEntry;
        static FieldInfo logListViewCurrentRow;
        static FieldInfo logEntryCondition;
        static MethodInfo LogEntriesGetEntry;

        static bool GetConsoleWindowListView()
        {
            if (logListView == null)
            {
                Assembly unityEditorAssembly = Assembly.GetAssembly(typeof(EditorWindow));
                Type consoleWindowType = unityEditorAssembly.GetType("UnityEditor.ConsoleWindow");
                FieldInfo fieldInfo = consoleWindowType.GetField("ms_ConsoleWindow", BindingFlags.Static | BindingFlags.NonPublic);
                EditorWindow consoleWindow = fieldInfo.GetValue(null) as EditorWindow;

                if (consoleWindow == null)
                {
                    logListView = null;
                    return false;
                }

                FieldInfo listViewFieldInfo = consoleWindowType.GetField("m_ListView", BindingFlags.Instance | BindingFlags.NonPublic);
                logListView = listViewFieldInfo.GetValue(consoleWindow);
                logListViewCurrentRow = listViewFieldInfo.FieldType.GetField("row", BindingFlags.Instance | BindingFlags.Public);

                //Type logEntriesType = unityEditorAssembly.GetType("UnityEditorInternal.LogEntries");
                Type logEntriesType = unityEditorAssembly.GetType("UnityEditor.LogEntries");
                LogEntriesGetEntry = logEntriesType.GetMethod("GetEntryInternal", BindingFlags.Static | BindingFlags.Public);

                //Type logEntryType = unityEditorAssembly.GetType("UnityEditorInternal.LogEntry");
                Type logEntryType = unityEditorAssembly.GetType("UnityEditor.LogEntry");
                logEntry = Activator.CreateInstance(logEntryType);
                logEntryCondition = logEntryType.GetField("condition", BindingFlags.Instance | BindingFlags.Public);
            }
            return true;
        }

        static string GetListViewRowCount(ref int line)
        {
            if (!GetConsoleWindowListView())
                return null;

            int row = (int)logListViewCurrentRow.GetValue(logListView);
            LogEntriesGetEntry.Invoke(null, new object[] { row, logEntry });
            string condition = logEntryCondition.GetValue(logEntry) as string;

            int index = condition.IndexOf(logCSName, StringComparison.Ordinal);
            //不是经过我们封装的日志
            if (index < 0)
                return null;

            int lineIndex = condition.IndexOf(")", index, StringComparison.Ordinal);
            condition = condition.Substring(lineIndex + 2);
            index = condition.IndexOf(".cs:", StringComparison.Ordinal);

            if (index >= 0)
            {
                int lineStartIndex = condition.IndexOf(")", StringComparison.Ordinal);
                int lineEndIndex = condition.IndexOf(")", index, StringComparison.Ordinal);
                string _line = condition.Substring(index + 4, lineEndIndex - index - 4);
                Int32.TryParse(_line, out line);

                condition = condition.Substring(0, index);
                int startIndex = condition.LastIndexOf("/", StringComparison.Ordinal);

                string fileName = condition.Substring(startIndex + 1);
                fileName += ".cs";
                return fileName;
            }
            return null;
        }

        static int openInstanceID;
        static int openLine;

        [OnOpenAsset(0)]
        public static bool OnOpenAsset(int instanceID, int line)
        {
            //只对控制台的开启进行重定向
            if (!EditorWindow.focusedWindow.titleContent.text.Equals("Console"))
                return false;

            // 只对开启的脚本进行重定向
            //UnityEngine.Object assetObj = EditorUtility.InstanceIDToObject(instanceID);
            //Type assetType = assetObj.GetType();
            //if (assetType != typeof(UnityEditor.MonoScript))
            //return false;

            if (openInstanceID == instanceID && openLine == line)
            {
                openInstanceID = -1;
                openLine = -1;
                return false;
            }

            openInstanceID = instanceID;
            openLine = line;

            string fileName = GetListViewRowCount(ref line);

            if (string.IsNullOrEmpty(fileName) || !fileName.EndsWith(".cs", StringComparison.Ordinal))
                return false;

            string filter = fileName.Substring(0, fileName.Length - 3);
            filter += " t:MonoScript";
            string[] searchPaths = AssetDatabase.FindAssets(filter);

            for (int i = 0; i < searchPaths.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(searchPaths[i]);

                if (path.EndsWith(fileName, StringComparison.Ordinal))
                {
                    UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath(path, typeof(MonoScript));
                    AssetDatabase.OpenAsset(obj, line);
                    return true;
                }
            }
            return false;
        }
    }

#endif
