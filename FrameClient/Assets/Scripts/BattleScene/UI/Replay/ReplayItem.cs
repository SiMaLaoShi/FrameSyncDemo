using System;
using BattleScene.Replay;
using UnityEngine;
using UnityEngine.UI;

namespace BattleScene.UI.Replay
{
    public class ReplayItem : MonoBehaviour
    {
        public Text ReplayDesc;
        public Button BtnReplay;
        private string date;
        private void Awake()
        {
            BtnReplay.onClick.AddListener((() =>
            {
                if (string.IsNullOrEmpty(date))
                    return;
                ReplaySystem.Instance.StartReplayInfo(date);
            }));
        }

        public void SetView(string replayDate)
        {
            date = replayDate;
            ReplayDesc.text = replayDate;
        }
    }
}