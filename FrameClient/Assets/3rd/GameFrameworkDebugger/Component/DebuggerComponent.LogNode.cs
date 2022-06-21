//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2020 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using GameFramework;
using System;
using UnityEngine;

namespace UnityGameFramework.Runtime
{
    public sealed partial class DebuggerComponent
    {
        /// <summary>
        /// 日志记录结点。
        /// </summary>
        public sealed class LogNode
        {
            private DateTime m_LogTime;
            private int m_LogFrameCount;
            private LogType m_LogType;
            private string m_LogMessage;
            private string m_StackTrack;

            /// <summary>
            /// 初始化日志记录结点的新实例。
            /// </summary>
            public LogNode(LogType logType, string logMessage, string stackTrack)
            {
                m_LogTime = DateTime.Now;
                m_LogFrameCount = Time.frameCount;
                m_LogType = logType;
                m_LogMessage = logMessage;
                m_StackTrack = stackTrack;
            }

            /// <summary>
            /// 获取日志时间。
            /// </summary>
            public DateTime LogTime
            {
                get
                {
                    return m_LogTime;
                }
            }

            /// <summary>
            /// 获取日志帧计数。
            /// </summary>
            public int LogFrameCount
            {
                get
                {
                    return m_LogFrameCount;
                }
            }

            /// <summary>
            /// 获取日志类型。
            /// </summary>
            public LogType LogType
            {
                get
                {
                    return m_LogType;
                }
            }

            /// <summary>
            /// 获取日志内容。
            /// </summary>
            public string LogMessage
            {
                get
                {
                    return m_LogMessage;
                }
            }

            /// <summary>
            /// 获取日志堆栈信息。
            /// </summary>
            public string StackTrack
            {
                get
                {
                    return m_StackTrack;
                }
            }
        }
    }
}
