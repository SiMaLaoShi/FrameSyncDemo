using System;

namespace BattleScene.Replay
{

    public enum FrameType : byte
    {
        None = Byte.MinValue,
        
        Max = Byte.MaxValue, 
    } 
    
    public class ReplaySystem : Singleton<ReplaySystem>
    {
        public void AddFrame(int frameId)
        {
            
        }
    }
}