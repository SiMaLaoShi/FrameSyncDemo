using System;
using UnityEngine;

namespace BattleScene.Replay
{
    public class ReplayMono : MonoBehaviour
    {
        public const float frameTime = 0.066f;
        private void Awake()
        {
            DontDestroyOnLoad(this);
            InvokeRepeating("ReplayLogicUpdate", 0f, frameTime );
        }
        
        private void ReplayLogicUpdate()
        {
            if (ReplaySystem.Instance.IsReplayIng)
                ReplaySystem.Instance.ReadByte();
        }

        private void OnGUI()
        {
            if (GUILayout.Button("回放"))
                ReplaySystem.Instance.StartReplayInfo();
        }
    }
}