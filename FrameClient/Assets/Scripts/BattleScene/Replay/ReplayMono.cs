using System;
using UnityEngine;

namespace BattleScene.Replay
{
    public class ReplayMono : MonoBehaviour
    {
        private void Awake()
        {
            DontDestroyOnLoad(this);
        }

        private void Update()
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