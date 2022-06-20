using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Lib.Runtime;
using LitJson;
using PBBattle;
using PBCommon;
using SimpleJSON;
using UnityEngine;

namespace BattleScene.Replay
{

    public enum FrameType : byte
    {
        None = Byte.MinValue,
        
        BattleStart,

        Max = Byte.MaxValue, 
    } 
    
    public class ReplaySystem : Singleton<ReplaySystem>
    {
        private List<FrameInfo> frameInfos = new List<FrameInfo>();
        private List<byte> bytes = new List<byte>();
        private List<byte[]> serverBytes = new List<byte[]>();
        private TcpEnterBattle tcpEnterBattle;
        public bool IsReplayIng { get; set; }

        private const string ReplayName = "replay";
        public readonly string SaveReplayDir = Path.Combine(Environment.CurrentDirectory, ReplayName);
        public override void OnSingletonInit()
        {
            CommonUtility.MkdirNotDelete(SaveReplayDir);
        }

        public void AddTcpEnterBattle(TcpEnterBattle tcpEnterBattle)
        {
            this.tcpEnterBattle = tcpEnterBattle;
        }

        public void AddFrame(int frameId, byte[] bytes, SCID scid)
        {
            if (IsReplayIng)
                return;
            frameInfos.Add(new FrameInfo()
            {
                FrameIdx = frameId,
                ScId = (int)scid,
                Length = bytes.Length,
            });
            this.bytes.AddRange(bytes);
            if (scid == SCID.UDP_DOWN_GAME_OVER)
                SaveReplayInfo();
        }

        public void SaveReplayInfo()
        {
            var date = DateTime.Now.ToLocalTime().ToString("yyyyMMddHHmmss");
            var directoryInfo = CommonUtility.Mkdir(Path.Combine(SaveReplayDir, date));
            var bytePath = Path.Combine(directoryInfo.FullName, string.Format("replay_{0}.byte", date));
            var jsonPath = Path.Combine(directoryInfo.FullName, string.Format("replay_{0}.json", date));
            var rolePath = Path.Combine(directoryInfo.FullName, string.Format("replay_{0}_role.byte", date));
            File.WriteAllText(jsonPath, JsonMapper.ToJson(frameInfos));
            File.WriteAllBytes(rolePath, CSData.SerializeData(tcpEnterBattle));
            File.WriteAllBytes(bytePath, bytes.ToArray());
            
            Debug.Log("保存回放文件成功 " + bytePath);
        }

        public void StartReplayInfo(string date)
        {
            var dir = Path.Combine(SaveReplayDir, date);
            var bytePath = Path.Combine(dir, string.Format("replay_{0}.byte", date));
            var jsonPath = Path.Combine(dir, string.Format("replay_{0}.json", date));
            var infos = JsonMapper.ToObject<List<FrameInfo>>(File.ReadAllText(jsonPath));
            var rolePath = Path.Combine(dir, string.Format("replay_{0}_role.byte", date));
            tcpEnterBattle = CSData.DeserializeData<TcpEnterBattle>(File.ReadAllBytes(rolePath));
            frameInfos = infos;
            BattleData.Instance.UpdateBattleInfo(tcpEnterBattle.randSeed, tcpEnterBattle.battleUserInfo);
            var allBytes = File.ReadAllBytes(bytePath);
            var pos = 0;
            for (int i = 0; i < infos.Count; i++)
            {
                Debug.Log(infos[i].FrameIdx + "|" + infos[i].ScId);
                var bytes = new byte[infos[i].Length];
                for (int j = 0; j < infos[i].Length; j++)
                {
                    bytes[j] = allBytes[pos];
                    pos++;
                }
                serverBytes.Add(bytes);
            }
            ClearSceneData.LoadScene(GameConfig.replayScene);
        }

        private int renderByteIdx = 0;
        
        public void ReadByte()
        {
            if (renderByteIdx >= serverBytes.Count)
            {
                Debug.Log("回放结束");
                IsReplayIng = false;
                return;
            }
                
            ReplayBattleCon.Instance.UpdateLogic((SCID)frameInfos[renderByteIdx].ScId, serverBytes[renderByteIdx]);
            renderByteIdx++;
        }

        class FrameInfo
        {
            public int ScId { get; set; }
            public int FrameIdx { get; set; }
            public int Length { get; set; }
        }
    }
}