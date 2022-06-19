using System;
using System.Collections.Generic;
using System.IO;
using BattleScene.Replay;
using Lib.Runtime;
using UnityEngine;

namespace BattleScene.UI.Replay
{
    public class UIReplayListWnd : MonoBehaviour
    {
        private List<ReplayItem> ReplayItems;
        public GameObject ReplayItemGo;
        public RectTransform ReplayListTr;
        private readonly List<string> ReplayDates = new List<string>();
        private void Awake()
        {
            LoadReplayList();
            foreach (var date in ReplayDates)
            {
                var go = Instantiate(ReplayItemGo, ReplayListTr);
                var replayItem = go.GetComponent<ReplayItem>();
                replayItem.SetView(date);
            }
        }

        void LoadReplayList()
        {
            var dir = Directory.GetDirectories(ReplaySystem.Instance.SaveReplayDir);
            foreach (var s in dir)
            {
                var directoryInfo = new DirectoryInfo(s);
                ReplayDates.Add(directoryInfo.Name);
            }
        }
    }
}