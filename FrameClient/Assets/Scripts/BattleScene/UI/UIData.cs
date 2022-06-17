using UnityEngine;
using UnityEngine.UI;

public class UIData : MonoBehaviour
{
    public Text fpsText;
    public Text netText;
    public Text timeText;
    public Text frameText;
    public Text sendText;

    public Text recvText;

    // Use this for initialization
    private void Start()
    {
        InvokeRepeating("UpdateNetInfo", 1f, 1f);
    }

    // Update is called once per frame
    private void Update()
    {
        BattleData.Instance.fps++;
    }

    private void UpdateNetInfo()
    {
        if (BattleData.Instance.netPack == 0)
        {
            netText.text = "1000ms";
        }
        else
        {
            var _nt = (int)(1000f / BattleData.Instance.netPack);
            netText.text = _nt + "ms";
        }

        BattleData.Instance.netPack = 0;

        var _frameNum = BattleData.Instance.GetFrameDataNum();
        var time = _frameNum * NetConfig.frameTime / 1000;

        var _timeStr = string.Format("{0}:{1}", time / 60, time % 60);
        timeText.text = _timeStr;

        frameText.text = "f:" + _frameNum;

        sendText.text = "s:" + GetNumberString(BattleData.Instance.sendNum);
        recvText.text = "r:" + GetNumberString(BattleData.Instance.recvNum);


        fpsText.text = "fps:" + BattleData.Instance.fps;
        BattleData.Instance.fps = 0;
    }

    private string GetNumberString(int _number)
    {
        if (_number < 1024) return _number + "B";

        var _num1 = _number / 1024f;
        if (_num1 < 1024f)
        {
            return string.Format("{0:F1}K", _num1);
        }

        var _num2 = _num1 / 1024f;
        return string.Format("{0:F1}M", _num2);
    }

    private void GameOver()
    {
        CancelInvoke();
    }
}