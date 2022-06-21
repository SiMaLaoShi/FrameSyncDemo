using GameFramework.Debugger;
using System;
using UnityEngine;

namespace UnityGameFramework.Runtime
{
    public sealed partial class DebuggerComponent
    {
        [Serializable]
        private sealed class LuaWindow : IDebuggerWindow
        {
            private string _lua = string.Empty;
            private string _tips = "这是一个快捷调用lua的窗口，只要把你的lua代码复制到下面的框框，点击执行，就能调用到lua堆栈，方便在真机上测试";
            public void Initialize(params object[] args)
            {
            }

            public void OnDraw()
            {
                

                GUILayout.BeginVertical("box");
                {
                    GUILayout.TextArea(_tips);
                    _lua = GUILayout.TextField(_lua);
                }
                GUILayout.EndVertical();
                if (GUILayout.Button("执行"))
                {
                    if (!string.IsNullOrEmpty(_lua))
                    {
                        //todo 通过LuaState doString
                        _lua = string.Empty;
                    }
                        
                }
            }

            public void OnEnter()
            {
            }

            public void OnLeave()
            {
            }

            public void OnUpdate(float elapseSeconds, float realElapseSeconds)
            {
            }

            public void Shutdown()
            {
                _lua = string.Empty;
            }
        }
    }
}