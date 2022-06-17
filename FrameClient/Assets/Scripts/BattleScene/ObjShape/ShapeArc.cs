using UnityEngine;

public class ShapeArc : ShapeBase
{
    public GameVec2Con arcCenterCon;
    public int arcRadiusCon;

    public int arcAngle;
    public int arcAngleSize;

#if UNITY_EDITOR
    [Header("是否显示圆弧的整圆")] public bool showArcCircle = true;

#endif
    protected GameVector2 arcCenter;
    protected int arcRadius;

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (closeLine) return;
        Gizmos.color = lineColor;


        var baseVec3 = ToolGameVector.ChangeGameVectorConToVector3(baseCenter);
        Gizmos.DrawWireSphere(baseVec3, baseRadiusCon);

        var arcCenter = baseVec3 + ToolGameVector.ChangeGameVectorConToVector3(arcCenterCon);

        if (showArcCircle) Gizmos.DrawWireSphere(arcCenter, arcRadiusCon);

        var vectNumber = arcAngleSize / 3;

        var firstVect1 = arcCenter + new Vector3(Mathf.Cos(Mathf.Deg2Rad * arcAngle) * arcRadiusCon,
            Mathf.Sin(Mathf.Deg2Rad * arcAngle) * arcRadiusCon, 0);
        var firstVect2 = firstVect1;

//		Gizmos.color = Color.blue;
        for (var i = 1; i <= vectNumber; i++)
        {
            var angle1 = arcAngle + 3 * i;
            var angle2 = arcAngle - 3 * i;
            if (i == vectNumber)
            {
                angle1 = arcAngle + arcAngleSize;
                angle2 = arcAngle - arcAngleSize;
            }

            var fAngle1 = Mathf.Deg2Rad * angle1;
            var fAngle2 = Mathf.Deg2Rad * angle2;

            var vectPos1 = arcCenter +
                           new Vector3(Mathf.Cos(fAngle1) * arcRadiusCon, Mathf.Sin(fAngle1) * arcRadiusCon, 0);
            Gizmos.DrawLine(firstVect1, vectPos1);
            firstVect1 = vectPos1;


            var vectPos2 = arcCenter +
                           new Vector3(Mathf.Cos(fAngle2) * arcRadiusCon, Mathf.Sin(fAngle2) * arcRadiusCon, 0);
            Gizmos.DrawLine(firstVect2, vectPos2);
            firstVect2 = vectPos2;
        }


        if (fixPosition)
        {
            var _posX = (int)transform.position.x + 25;
            var _posY = (int)transform.position.y + 25;

            _posX = _posX - _posX % 50;
            _posY = _posY - _posY % 50;


            baseCenter.x = _posX;
            baseCenter.y = _posY;

            transform.position = new Vector3(_posX * 1.0f, _posY * 1.0f);

            fixPosition = false;
        }
    }
#endif
    public override void InitData()
    {
        type = ShapeType.arc;
        arcRadius = ToolMethod.Config2Logic(arcRadiusCon);

        arcCenter = ToolGameVector.ChangeGameVectorConToGameVector2(arcCenterCon) + basePosition;
    }

    public override bool IsCollisionCircle(GameVector2 _pos, int _radius)
    {
        return ToolGameVector.CollideCircleAndArc(_pos, _radius, arcCenter, arcRadius, arcAngle, arcAngleSize);
    }

    public override bool IsCollisionCircleCorrection(GameVector2 _pos, int _radius, out GameVector2 _amend)
    {
        return ToolGameVector.CollideCircleAndArc(_pos, _radius, arcCenter, arcRadius, arcAngle, arcAngleSize,
            out _amend);
    }
}