using System.Collections;
using PBBattle;
using PBCommon;
using UnityEngine;

namespace BattleScene.Replay
{
    public class ReplayBattleCon : MonoBehaviour
    {
        public delegate void DelegateEvent();

        [HideInInspector] public RoleManage roleManage;
        [HideInInspector] public ObstacleManage obstacleManage;
        [HideInInspector] public BulletManage bulletManage;
        private bool isBattleFinish;

        private bool isBattleStart;
        
        public Transform mapTranform;

        public static ReplayBattleCon Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
            InitData(mapTranform);
            StartCoroutine("WaitInitData");
        }

        public void UpdateLogic(SCID messageId, byte[] bodyData)
        {
            switch (messageId)
            {
                case SCID.UDP_BATTLE_START:
                {
                    var pb_ReceiveMes = CSData.DeserializeData<UdpBattleStart>(bodyData);
                    // BattleStart();
                }
                    break;
                case SCID.UDP_DOWN_FRAME_OPERATIONS:
                {
                    var pb_ReceiveMes = CSData.DeserializeData<UdpDownFrameOperations>(bodyData);
                    BattleData.Instance.AddNewFrameData(pb_ReceiveMes.frameID, pb_ReceiveMes.operations);
                    BattleData.Instance.netPack++;
                }
                    break;
                case SCID.UDP_DOWN_DELTA_FRAMES:
                {
                    // var pb_ReceiveMes = CSData.DeserializeData<UdpDownDeltaFrames>(bodyData);
                    // ReplaySystem.Instance.AddFrame(BattleData.Instance.FrameId, bodyData, SCID.UDP_DOWN_DELTA_FRAMES);
                    // NetGlobal.Instance().AddAction(() => { mes_delta_frame_data(pb_ReceiveMes); });
                }
                    break;
                case SCID.UDP_DOWN_GAME_OVER:
                {
                    // var pb_ReceiveMes = CSData.DeserializeData<UdpDownGameOver>(bodyData);
                    // ReplaySystem.Instance.AddFrame(BattleData.Instance.FrameId, bodyData, SCID.UDP_DOWN_GAME_OVER);
                    // NetGlobal.Instance().AddAction(() => { mes_down_game_over(pb_ReceiveMes); });
                }
                    break;
            }
        }
        
        
        public void InitData(Transform _map)
        {
            ToolRandom.srand((ulong)BattleData.Instance.randSeed);
            roleManage = gameObject.AddComponent<RoleManage>();
            obstacleManage = gameObject.AddComponent<ObstacleManage>();
            bulletManage = gameObject.AddComponent<BulletManage>();

            GameVector2[] roleGrid;
            roleManage.InitData(_map.Find("Role"), out roleGrid);
            obstacleManage.InitData(_map.Find("Obstacle"), roleGrid);
            bulletManage.InitData(_map.Find("Bullet"));
        }
        
        private IEnumerator WaitInitData()
        {
            yield return new WaitUntil(() =>
            {
                return roleManage.initFinish && obstacleManage.initFinish && bulletManage.initFinish;
            });
            
            ReplaySystem.Instance.IsReplayIng = true;
            InvokeRepeating("LogicUpdate", 0f, 0.020f);
        }
        
        private void LogicUpdate()
        {
            if (ReplaySystem.Instance.IsReplayIng)
            {
                AllPlayerOperation op;
                if (BattleData.Instance.TryGetNextPlayerOp(out op))
                {
                    roleManage.Logic_Operation(op);
                    roleManage.Logic_Move();
                    bulletManage.Logic_Move();
                    bulletManage.Logic_Collision();
                    roleManage.Logic_Move_Correction();
                    obstacleManage.Logic_Destory();
                    bulletManage.Logic_Destory();
                    BattleData.Instance.RunOpSucces();
                }
            }
        }
    }
}