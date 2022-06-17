using UnityEngine;

public class ShapeCircle : ShapeBase
{
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (closeLine) return;
        Gizmos.color = lineColor;

        if (Application.isPlaying)
            Gizmos.DrawWireSphere(ToolGameVector.ChangeGameVectorToVector3(basePosition),
                ToolMethod.Config2Render(baseRadiusCon));
        else
            Gizmos.DrawWireSphere(ToolGameVector.ChangeGameVectorConToVector3(baseCenter),
                ToolMethod.Config2Render(baseRadiusCon));

        if (fixPosition)
        {
            var _posX = ToolMethod.Render2Config(transform.position.x) + 25;
            var _posY = ToolMethod.Render2Config(transform.position.z) + 25;

            _posX = _posX - _posX % 50;
            _posY = _posY - _posY % 50;

            baseCenter.x = _posX;
            baseCenter.y = _posY;

            transform.position = ToolGameVector.ChangeGameVectorConToVector3(baseCenter);

            fixPosition = false;
        }
    }
#endif

    public override void InitData()
    {
        type = ShapeType.circle;
    }
}