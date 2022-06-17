using System;
using UnityEngine;

public class ToolGameVector
{
    public static bool IsInDistance(GameVector2 _pos1, GameVector2 _pos2, long _distance)
    {
        return _pos1.sqrMagnitude(_pos2) <= _distance * _distance;
    }

    public static bool IsInDistance(GameVector2 _pos1, GameVector2 _pos2, long _distance, out long _realdis)
    {
        _realdis = _pos1.sqrMagnitude(_pos2);
        return _realdis <= _distance * _distance;
    }

    public static Vector3 ChangeGameVectorToVector3(GameVector2 _vec2, float _y = 0f)
    {
        return new Vector3(ToolMethod.Logic2Render(_vec2.x), _y, ToolMethod.Logic2Render(_vec2.y));
    }

    public static GameVector2 ChangeGameVectorConToGameVector2(GameVec2Con _vec2)
    {
        return new GameVector2(ToolMethod.Config2Logic(_vec2.x), ToolMethod.Config2Logic(_vec2.y));
    }

    public static Vector3 ChangeGameVectorConToVector3(GameVec2Con _vec2)
    {
        return new Vector3(ToolMethod.Config2Render(_vec2.x), 0f, ToolMethod.Config2Render(_vec2.y));
    }

    /// <summary>
    ///     把坐标往绝对值大取整到百位,主要用于碰撞后的位置修正
    /// </summary>
    public static void RoundGameVector2(ref GameVector2 _vec2)
    {
        _vec2.x = RoundInt(_vec2.x);
        _vec2.y = RoundInt(_vec2.y);
    }

    public static int RoundInt(int _int)
    {
        return _int - _int % ToolMethod.Config2LogicScale + Math.Sign(_int) * ToolMethod.Config2LogicScale;
    }

    //点和圆碰撞
    public static bool CollidePointAndCircle(GameVector2 _p, GameVector2 _c, int _radius)
    {
        long sprRadius = _radius * _radius;
        var sqrDistance = _c.sqrMagnitude(_p);
        return sqrDistance < sprRadius;
    }

    /// <summary>
    ///     点和圆的碰撞,圆不动修正点的位置
    /// </summary>
    /// <returns><c>true</c>, if point and circle was collided, <c>false</c> otherwise.</returns>
    /// <param name="_p">碰撞点.</param>
    /// <param name="_c">碰撞圆心.</param>
    /// <param name="_radius">圆半径.</param>
    /// <param name="_amend">修正值.</param>
    public static bool CollidePointAndCircle(GameVector2 _p, GameVector2 _c, int _radius, out GameVector2 _amend)
    {
        _amend = GameVector2.zero;

        long sprRadius = _radius * _radius;

        var sqrDistance = _c.sqrMagnitude(_p);

        if (sqrDistance >= sprRadius) return false;

        var distance = (int)Mathf.Sqrt(sqrDistance);
        var _angle = (_p - _c).Angle();
        var amendDis = 1 + (_radius - distance) / 100;
        _amend = amendDis * BattleData.Instance.GetSpeed(_angle);
        return true;
    }

    //圆和圆碰撞
    public static bool CollideCircleAndCircle(GameVector2 _c1, GameVector2 _c2, int _radius1, int _radius2)
    {
        var sumRadius = _radius1 + _radius2;
        long sprRadius = sumRadius * sumRadius;
        var sqrDistance = _c1.sqrMagnitude(_c2);
        return sqrDistance < sprRadius;
    }

    /// <summary>
    ///     圆和圆的碰撞,圆2不动,修正是的是圆1的位置
    /// </summary>
    /// <returns><c>true</c>, if circle and circle was collided, <c>false</c> otherwise.</returns>
    /// <param name="_c1">圆1的圆心.</param>
    /// <param name="_c2">圆2的圆心.</param>
    /// <param name="_radius1">圆1的半径.</param>
    /// <param name="_radius2">圆2的半径.</param>
    /// <param name="_amend">修正值.</param>
    public static bool CollideCircleAndCircle(GameVector2 _c1, GameVector2 _c2, int _radius1, int _radius2,
        out GameVector2 _amend)
    {
        return CollidePointAndCircle(_c1, _c2, _radius1 + _radius2, out _amend);
    }

    /// <summary>
    ///     圆和圆的碰撞,2个圆的位置都修正
    /// </summary>
    /// <returns><c>true</c>, if circle and circle was collided, <c>false</c> otherwise.</returns>
    /// <param name="_c1">圆1的圆心.</param>
    /// <param name="_c2">圆2的圆心.</param>
    /// <param name="_radius1">圆1的半径.</param>
    /// <param name="_radius2">圆2的半径.</param>
    /// <param name="_amend1">圆1的修正值.</param>
    /// <param name="_amend2">圆2的修正值.</param>
    public static bool CollideCircleAndCircle(GameVector2 _c1, GameVector2 _c2, int _radius1, int _radius2,
        out GameVector2 _amend1, out GameVector2 _amend2)
    {
        _amend1 = GameVector2.zero;
        _amend2 = GameVector2.zero;

        var radiusSum = _radius1 + _radius2;
        long sprRadius = radiusSum * radiusSum;

        var sqrDistance = _c1.sqrMagnitude(_c2);

        if (sqrDistance >= sprRadius) return false;

        var distance = (int)Mathf.Sqrt(sqrDistance);
        var _angle = (_c1 - _c2).Angle();
        var amendDis = 1 + (int)((radiusSum - distance) * 0.5) / 100;

        _amend1 = amendDis * BattleData.Instance.GetSpeed(_angle);
        _amend2 = -1 * _amend1;

        return true;
    }

    //圆和矩形碰撞
    public static bool CollideCircleAndRect(GameVector2 _c1, int _radius1, GameVector2 _rCenter, int _half_w,
        int _half_h)
    {
        if (_radius1 <= 100)
        {
            //半径足够小，当成点来处理
            if (_c1.x <= _rCenter.x - _half_w) return false;

            if (_c1.y <= _rCenter.y - _half_h) return false;
            if (_c1.x >= _rCenter.x + _half_w) return false;
            if (_c1.y >= _rCenter.y + _half_h) return false;
            return true;
        }

        var llx = _rCenter.x - _half_w - _radius1;
        if (_c1.x <= llx) return false;

        var bby = _rCenter.y - _half_h - _radius1;
        if (_c1.y <= bby) return false;

        var rrx = _rCenter.x + _half_w + _radius1;
        if (_c1.x >= rrx) return false;

        var tty = _rCenter.y + _half_h + _radius1;
        if (_c1.y >= tty) return false;

        var lx = _rCenter.x - _half_w;
        var by = _rCenter.y - _half_h;
        var rx = _rCenter.x + _half_w;
        var ty = _rCenter.y + _half_h;

        if (_c1.x <= lx)
        {
            if (_c1.y > ty)
            {
                //左上角的顶点矩形里
                var _ltPoint = new GameVector2(lx, ty);
                return CollidePointAndCircle(_c1, _ltPoint, _radius1);
            }

            if (_c1.y < by)
            {
                //左下角的顶点矩形里
                var _lbPoint = new GameVector2(lx, by);
                return CollidePointAndCircle(_c1, _lbPoint, _radius1);
            }

            return true;
        }

        if (_c1.x >= rx)
        {
            if (_c1.y > ty)
            {
                //右上角的顶点矩形里
                var _rtPoint = new GameVector2(rx, ty);
                return CollidePointAndCircle(_c1, _rtPoint, _radius1);
            }

            if (_c1.y < by)
            {
                //右下角的顶点矩形里
                var _rbPoint = new GameVector2(rx, by);
                return CollidePointAndCircle(_c1, _rbPoint, _radius1);
            }

            return true;
        }

        return true;
    }

    /// <summary>
    ///     圆和矩形的修正碰撞
    /// </summary>
    /// <returns><c>true</c>, 产生碰撞, <c>false</c> 没有碰撞.</returns>
    /// <param name="_c1">圆心.</param>
    /// <param name="_radius1">圆半径.</param>
    /// <param name="_rCenter">矩形中心.</param>
    /// <param name="_half_w">矩形宽度的一半.</param>
    /// <param name="_half_h">矩形高度的一半.</param>
    /// <param name="_amend">修正值.</param>
    public static bool CollideCircleAndRect(GameVector2 _c1, int _radius1, GameVector2 _rCenter, int _half_w,
        int _half_h, out GameVector2 _amend)
    {
        /*
            *l:左
            *r:右
            *t:上/顶
            *b:下/底
        */
        var llx = _rCenter.x - _half_w - _radius1;
        if (_c1.x <= llx)
        {
            _amend = GameVector2.zero;
            return false;
        }

        var bby = _rCenter.y - _half_h - _radius1;
        if (_c1.y <= bby)
        {
            _amend = GameVector2.zero;
            return false;
        }

        var rrx = _rCenter.x + _half_w + _radius1;
        if (_c1.x >= rrx)
        {
            _amend = GameVector2.zero;
            return false;
        }

        var tty = _rCenter.y + _half_h + _radius1;
        if (_c1.y >= tty)
        {
            _amend = GameVector2.zero;
            return false;
        }

        var lx = _rCenter.x - _half_w;
        var by = _rCenter.y - _half_h;
        var rx = _rCenter.x + _half_w;
        var ty = _rCenter.y + _half_h;

        if (_c1.x <= lx)
        {
            if (_c1.y > ty)
            {
                //左上角的顶点矩形里
                var _ltPoint = new GameVector2(lx, ty);
                return CollidePointAndCircle(_c1, _ltPoint, _radius1, out _amend);
            }

            if (_c1.y < by)
            {
                //左下角的顶点矩形里
                var _lbPoint = new GameVector2(lx, by);
                return CollidePointAndCircle(_c1, _lbPoint, _radius1, out _amend);
            }

            //左侧矩形
            _amend = new GameVector2(llx - _c1.x, 0);
        }
        else if (_c1.x >= rx)
        {
            if (_c1.y > ty)
            {
                //右上角的顶点矩形里
                var _rtPoint = new GameVector2(rx, ty);
                return CollidePointAndCircle(_c1, _rtPoint, _radius1, out _amend);
            }

            if (_c1.y < by)
            {
                //右下角的顶点矩形里
                var _rbPoint = new GameVector2(rx, by);
                return CollidePointAndCircle(_c1, _rbPoint, _radius1, out _amend);
            }

            //右侧矩形
            _amend = new GameVector2(rrx - _c1.x, 0);
        }
        else
        {
            if (_c1.y > ty)
            {
                //中上矩形
                _amend = new GameVector2(0, tty - _c1.y);
            }
            else if (_c1.y < by)
            {
                //中下矩形
                _amend = new GameVector2(0, bby - _c1.y);
            }
            else
            {
                //矩形内
                var _rtPoint = new GameVector2(rx, ty);
                var _rtAngle = (_rtPoint - _rCenter).Angle();
                var _ltAngle = 180 - _rtAngle;
                var _lbAngle = 180 + _rtAngle;
                var _rbAngel = 360 - _rtAngle;

                var circleAngle = (_c1 - _rCenter).Angle(); //圆心和矩形中心的角度

                if (circleAngle > _rbAngel)
                    _amend = new GameVector2(rrx - _c1.x, 0);
                else if (circleAngle > _lbAngle)
                    _amend = new GameVector2(0, bby - _c1.y);
                else if (circleAngle > _ltAngle)
                    _amend = new GameVector2(llx - _c1.x, 0);
                else if (circleAngle > _rtAngle)
                    _amend = new GameVector2(0, tty - _c1.y);
                else
                    _amend = new GameVector2(rrx - _c1.x, 0);
            }
        }

        return true;
    }

    /// <summary>
    ///     点到直线距离
    /// </summary>
    /// <param name="point">点坐标</param>
    /// <param name="linePoint1">直线上一个点的坐标</param>
    /// <param name="linePoint2">直线上另一个点的坐标</param>
    /// <returns></returns>
    public static float DisPoint2Line(Vector3 point, Vector3 linePoint1, Vector3 linePoint2)
    {
        var vec1 = point - linePoint1;
        var vec2 = linePoint2 - linePoint1;
        var vecProj = Vector3.Project(vec1, vec2);
        var dis = Mathf.Sqrt(Mathf.Pow(Vector3.Magnitude(vec1), 2) - Mathf.Pow(Vector3.Magnitude(vecProj), 2));
        return dis;
    }

    /// <summary>
    ///     圆和多边形的碰撞
    /// </summary>
    /// <returns><c>true</c>, if circle and polygon was collided, <c>false</c> otherwise.</returns>
    /// <param name="_c1">圆心.</param>
    /// <param name="_radius1">圆半径.</param>
    /// <param name="vectexs">多边形顶点.</param>
    /// <param name="normalDir">多边形每条边的法向量.</param>
    /// <param name="_amend">位置修正.</param>
    public static bool CollideCircleAndPolygon(GameVector2 _c1, int _radius1, GameVec2Con[] vectexs, int[] normalDir,
        out GameVector2 _amend)
    {
        _amend = GameVector2.zero;
        var _circleCenter = ChangeGameVectorToVector3(_c1);
        for (var i = 0; i < vectexs.Length; i++)
        {
            var lineBegin = ChangeGameVectorConToVector3(vectexs[i]);
            var endIndex = (i + 1) % vectexs.Length;
            var lineEnd = ChangeGameVectorConToVector3(vectexs[endIndex]);

            var _circleVec = _circleCenter - lineBegin;
            var _line = lineEnd - lineBegin;
            var _cross = Vector3.Cross(_circleVec, _line);
            if (_cross.z < 0)
            {
                //在线段的左侧,即多边形的外侧
                var vecProj = Vector3.Project(_circleVec, _line); //投影点
                var disLine = _line.magnitude;
                var proj_begin = vecProj.magnitude;
                var proj_end = (vecProj - _line).magnitude;
                var projlengh = proj_begin + proj_end;
                if (disLine + 1 >= projlengh)
                {
                    //投影在线段上
                    var dis = (int)Mathf.Sqrt(_circleVec.sqrMagnitude - vecProj.sqrMagnitude);
                    var disRadius = _radius1 / 100;
                    if (dis < disRadius)
                    {
                        var amendDis = 1 + (_radius1 / 100 - dis);
                        _amend = amendDis * BattleData.Instance.GetSpeed(normalDir[i]);
                        return true;
                    }

                    return false;
                }

                Vector3 newLineBegin;
                Vector3 newLineEnd;
                //投影不在当前线段上
                var isSameDir = Vector3.Dot(vecProj, _line) > 0f ? true : false; //为0的时候是垂直,不会出现该情况
                int normalIndex;
                int linePointIndex; //2条线段的交点
                if (isSameDir)
                {
                    //同向
                    newLineBegin = lineEnd;
                    var newEndIndex = (i + 2) % vectexs.Length;
                    newLineEnd = ChangeGameVectorConToVector3(vectexs[newEndIndex]);
                    normalIndex = endIndex;
                    linePointIndex = endIndex;
                }
                else
                {
                    //反向
                    if (i == 0)
                        newLineBegin = ChangeGameVectorConToVector3(vectexs[vectexs.Length - 1]);
                    else
                        newLineBegin = ChangeGameVectorConToVector3(vectexs[i - 1]);
                    newLineEnd = lineBegin;
                    normalIndex = vectexs.Length - 1;
                    linePointIndex = i;
                }

                var newCircleVec = _circleCenter - newLineBegin;
                var _newline = newLineEnd - newLineBegin;
                var newVecProj = Vector3.Project(newCircleVec, _newline); //投影点
                var newdisLine = _newline.magnitude;
                var newproj_begin = newVecProj.magnitude;
                var newproj_end = (newVecProj - _newline).magnitude;
                var newprojlengh = newproj_begin + newproj_end;
                if (newdisLine + 1 >= newprojlengh)
                {
                    //投影在线段上
                    var dis = (int)Mathf.Sqrt(newCircleVec.sqrMagnitude - newVecProj.sqrMagnitude);
                    var disRadius = _radius1 / 100;
                    if (dis < disRadius)
                    {
                        var amendDis = 1 + (_radius1 / 100 - dis);
                        _amend = amendDis * BattleData.Instance.GetSpeed(normalDir[normalIndex]);
                        return true;
                    }

                    return false;
                }

                var isNewSameDir = Vector3.Dot(newVecProj, _newline) > 0f ? true : false;
                if (isNewSameDir != isSameDir)
                {
                    //夹角处
                    var _point = ChangeGameVectorConToGameVector2(vectexs[linePointIndex]);
                    GameVector2 _outAmend;
                    var _result = CollidePointAndCircle(_c1, _point, _radius1, out _outAmend);
                    _amend = _outAmend;
                    return _result;
                }
            }
        }

        return false;
    }

    //圆和多边形碰撞
    public static bool CollideCircleAndPolygon(GameVector2 _c1, int _radius1, GameVec2Con[] vectexs, int[] normalDir)
    {
        var _circleCenter = ChangeGameVectorToVector3(_c1);
        var isAllIn = true; //是否都在多边形线段的右侧
        for (var i = 0; i < vectexs.Length; i++)
        {
            var lineBegin = ChangeGameVectorConToVector3(vectexs[i]);
            var endIndex = (i + 1) % vectexs.Length;
            var lineEnd = ChangeGameVectorConToVector3(vectexs[endIndex]);

            var _circleVec = _circleCenter - lineBegin;
            var _line = lineEnd - lineBegin;
            var _cross = Vector3.Cross(_circleVec, _line);
            if (_cross.z < 0)
            {
                isAllIn = false;
                //在线段的左侧,即多边形的外侧
                var vecProj = Vector3.Project(_circleVec, _line); //投影点
                var disLine = _line.magnitude;
                var proj_begin = vecProj.magnitude;
                var proj_end = (vecProj - _line).magnitude;
                var projlengh = proj_begin + proj_end;
                if (disLine + 1 >= projlengh)
                {
                    //投影在线段上
                    var dis = (int)Mathf.Sqrt(_circleVec.sqrMagnitude - vecProj.sqrMagnitude);
                    var disRadius = _radius1 / 100;
                    if (dis < disRadius)
                        return true;
                    return false;
                }

                //投影不在当前线段上
                if (CollidePointAndCircle(ChangeGameVectorConToGameVector2(vectexs[i]), _c1, _radius1))
                    return true;
                return CollidePointAndCircle(ChangeGameVectorConToGameVector2(vectexs[endIndex]), _c1, _radius1);
            }
        }

        if (isAllIn) //全在内侧,圆心在多边形内
            return true;
        return false;
    }

    /// <summary>
    ///     圆和正圆弧的碰撞,带位置修正
    /// </summary>
    /// <returns><c>true</c>, if circle and arc was collided, <c>false</c> otherwise.</returns>
    /// <param name="_c1">圆心.</param>
    /// <param name="_radius1">圆半径.</param>
    /// <param name="_arcCen">圆弧圆心.</param>
    /// <param name="_arcRadius">圆弧所在圆半径.</param>
    /// <param name="_arcAngle">角度.</param>
    /// <param name="_arcAngleSize">角度范围.</param>
    /// <param name="_amend">修正值.</param>
    public static bool CollideCircleAndArc(GameVector2 _c1, int _radius1, GameVector2 _arcCen, int _arcRadius,
        int _arcAngle, int _arcAngleSize, out GameVector2 _amend)
    {
        _amend = GameVector2.zero;
        var radiusSum = _radius1 + _arcRadius;
        long sqrRadiusSum = radiusSum * radiusSum;
        var sqrDistance = _c1.sqrMagnitude(_arcCen);
        if (sqrDistance >= sqrRadiusSum) return false;
        var radiusDel = _radius1 - _arcRadius;
        long sqrRadiusDel = radiusDel * radiusDel;
        if (sqrDistance <= sqrRadiusDel) return false;
        var _angle = (_c1 - _arcCen).Angle();
        var _point1Angle = _arcAngle + _arcAngleSize;
        var _point2Angle = _arcAngle - _arcAngleSize;
        bool inArc;
        if (_point2Angle < 0)
        {
            var _angle1 = (int)Mathf.Repeat(_angle - _point2Angle, 360f);
            if (_angle1 >= 0 && _angle1 <= _point1Angle - _point2Angle)
                inArc = true;
            else
                inArc = false;
        }
        else if (_point1Angle > 360)
        {
            var _delA = _point1Angle - 360;
            var _angle1 = (int)Mathf.Repeat(_angle - _delA, 360f);
            if (_angle1 >= _point2Angle - _delA && _angle1 <= 360)
                inArc = true;
            else
                inArc = false;
        }
        else
        {
            if (_angle >= _point2Angle && _angle <= _point1Angle)
                inArc = true;
            else
                inArc = false;
        }

        if (inArc)
        {
            //在圆弧内,需要修正
            long _arcRadiusSqr = _arcRadius * _arcRadius;
            var distance = (int)Mathf.Sqrt(sqrDistance);
            int amendDis;
            int amendAngle;
            if (sqrDistance >= _arcRadiusSqr)
            {
                //向外修正
                amendDis = 1 + (radiusSum - distance) / 100;
                amendAngle = _angle;
            }
            else
            {
                amendDis = 1 + (distance - Mathf.Abs(radiusDel)) / 100;
                amendAngle = (_arcCen - _c1).Angle();
            }

            _amend = amendDis * BattleData.Instance.GetSpeed(amendAngle);
            return true;
        }

        _point1Angle = (int)Mathf.Repeat(_point1Angle, 360);
        var _point1 = _arcCen + _arcRadius / 100 * BattleData.Instance.GetSpeed(_point1Angle);

        GameVector2 _outAmend1;
        var _result1 = CollidePointAndCircle(_c1, _point1, _radius1, out _outAmend1);
        if (_result1)
        {
            _amend = _outAmend1;
            return _result1;
        }

        _point2Angle = (int)Mathf.Repeat(_point2Angle, 360);
        var _point2 = _arcCen + _arcRadius / 100 * BattleData.Instance.GetSpeed(_point2Angle);

        GameVector2 _outAmend2;
        var _result2 = CollidePointAndCircle(_c1, _point2, _radius1, out _outAmend2);
        if (_result2)
        {
            _amend = _outAmend2;
            return _result2;
        }

        return false;
    }

    // 圆和正圆弧的碰撞,不带位置修正
    public static bool CollideCircleAndArc(GameVector2 _c1, int _radius1, GameVector2 _arcCen, int _arcRadius,
        int _arcAngle, int _arcAngleSize)
    {
        var radiusSum = _radius1 + _arcRadius;
        long sqrRadiusSum = radiusSum * radiusSum;
        var sqrDistance = _c1.sqrMagnitude(_arcCen);
        if (sqrDistance >= sqrRadiusSum) return false;
        var radiusDel = _radius1 - _arcRadius;
        long sqrRadiusDel = radiusDel * radiusDel;
        if (sqrDistance <= sqrRadiusDel) return false;
        var _angle = (_c1 - _arcCen).Angle();
        if (Mathf.Abs(_angle - _arcAngle) <= _arcAngleSize) return true;
        var _point1Angle = (int)Mathf.Repeat(_arcAngle + _arcAngleSize, 360);
        var _point1 = _arcCen + _arcRadius / 100 * BattleData.Instance.GetSpeed(_point1Angle);
        var _result1 = CollidePointAndCircle(_c1, _point1, _radius1);
        if (_result1) return _result1;
        var _point2Angle = (int)Mathf.Repeat(_arcAngle - _arcAngleSize, 360);
        var _point2 = _arcCen + _arcRadius / 100 * BattleData.Instance.GetSpeed(_point2Angle);
        var _result2 = CollidePointAndCircle(_c1, _point2, _radius1);
        if (_result2) return _result2;
        return false;
    }

    /// <summary>
    ///     圆和正圆扇面的碰撞,带位置修正
    /// </summary>
    /// <returns><c>true</c>, if circle and arc was collided, <c>false</c> otherwise.</returns>
    /// <param name="_c1">圆心.</param>
    /// <param name="_radius1">圆半径.</param>
    /// <param name="_arcCen">扇面圆心.</param>
    /// <param name="_arcRadius">扇面所在圆半径.</param>
    /// <param name="_arcAngle">角度.</param>
    /// <param name="_arcAngleSize">角度范围.</param>
    /// <param name="_amend">修正值.</param>
    public static bool CollideCircleAndArcArea(GameVector2 _c1, int _radius1, GameVector2 _arcCen, int _arcRadius,
        int _arcAngle, int _arcAngleSize, out GameVector2 _amend)
    {
        _amend = GameVector2.zero;
        var radiusSum = _radius1 + _arcRadius;
        long sqrRadiusSum = radiusSum * radiusSum;
        var sqrDistance = _c1.sqrMagnitude(_arcCen);
        if (sqrDistance >= sqrRadiusSum) return false;
        var _angle = (_c1 - _arcCen).Angle();
        var _point1Angle = _arcAngle + _arcAngleSize;
        var _point2Angle = _arcAngle - _arcAngleSize;
        bool inArc;
        if (_point2Angle < 0)
        {
            var _angle1 = (int)Mathf.Repeat(_angle - _point2Angle, 360f);
            if (_angle1 >= 0 && _angle1 <= _point1Angle - _point2Angle)
                inArc = true;
            else
                inArc = false;
        }
        else if (_point1Angle > 360)
        {
            var _delA = _point1Angle - 360;
            var _angle1 = (int)Mathf.Repeat(_angle - _delA, 360f);
            if (_angle1 >= _point2Angle - _delA && _angle1 <= 360)
                inArc = true;
            else
                inArc = false;
        }
        else
        {
            if (_angle >= _point2Angle && _angle <= _point1Angle)
                inArc = true;
            else
                inArc = false;
        }

        if (inArc)
        {
            //在圆弧内,需要修正
            var distance = (int)Mathf.Sqrt(sqrDistance);
            var amendDis = 1 + (radiusSum - distance) / 100;
            _amend = amendDis * BattleData.Instance.GetSpeed(_angle);
            return true;
        }

        //线段修正
        var _circleCenter = ChangeGameVectorToVector3(_c1);
        var _arcAreaCenter = ChangeGameVectorToVector3(_arcCen);

        _point1Angle = (int)Mathf.Repeat(_point1Angle, 360);
        var _point1 = _arcCen + _arcRadius / 100 * BattleData.Instance.GetSpeed(_point1Angle);

        _point2Angle = (int)Mathf.Repeat(_point2Angle, 360);
        var _point2 = _arcCen + _arcRadius / 100 * BattleData.Instance.GetSpeed(_point2Angle);

        var _line1Point = ChangeGameVectorToVector3(_point1);
        var _line2Point = ChangeGameVectorToVector3(_point2);
        //线段1修正
        var amendline1 = false;
        var amendLine1P = GameVector2.zero;
        var _circleVec = _circleCenter - _arcAreaCenter;
        var _line = _line1Point - _arcAreaCenter;
        var _cross = Vector3.Cross(_circleVec, _line);
        if (_cross.z <= 0)
        {
            //在线段的左侧,即多边形的外侧
            var vecProj = Vector3.Project(_circleVec, _line); //投影点
            var disLine = _line.magnitude;
            var proj_begin = vecProj.magnitude;
            var proj_end = (vecProj - _line).magnitude;
            var projlengh = proj_begin + proj_end;
            if (disLine + 1 >= projlengh)
            {
                //投影在线段上
                var dis = (int)Mathf.Sqrt(_circleVec.sqrMagnitude - vecProj.sqrMagnitude);
                var disRadius = _radius1 / 100;
                if (dis < disRadius)
                {
                    //需要修正
                    amendline1 = true;
                    var amendDis = 1 + (_radius1 / 100 - dis);
                    var angelVec = _circleVec - vecProj;
                    var angle1 = Mathf.Atan2(angelVec.y, angelVec.x) * Mathf.Rad2Deg;
                    var amendAngle = (int)Mathf.Repeat(angle1, 360f);
                    amendLine1P = amendDis * BattleData.Instance.GetSpeed(amendAngle);
                }
            }
        }

        //线段2修正
        var amendline2 = false;
        var amendLine2P = GameVector2.zero;
        var _circleVec2 = _circleCenter - _line2Point;
        var _line2 = _arcAreaCenter - _line2Point;
        var _cross2 = Vector3.Cross(_circleVec2, _line2);
        if (_cross2.z <= 0)
        {
            //在线段的左侧,即多边形的外侧
            var vecProj = Vector3.Project(_circleVec2, _line2); //投影点
            var disLine = _line2.magnitude;
            var proj_begin = vecProj.magnitude;
            var proj_end = (vecProj - _line2).magnitude;
            var projlengh = proj_begin + proj_end;
            if (disLine + 1 >= projlengh)
            {
                //投影在线段上

                var dis = (int)Mathf.Sqrt(_circleVec2.sqrMagnitude - vecProj.sqrMagnitude);
                var disRadius = _radius1 / 100;
                if (dis < disRadius)
                {
                    //需要修正
                    amendline2 = true;
                    var amendDis = 1 + (_radius1 / 100 - dis);
                    var angelVec = _circleVec2 - vecProj;
                    var angle1 = Mathf.Atan2(angelVec.y, angelVec.x) * Mathf.Rad2Deg;
                    var amendAngle = (int)Mathf.Repeat(angle1, 360f);
                    amendLine2P = amendDis * BattleData.Instance.GetSpeed(amendAngle);
                }
            }
        }

        if (amendline1 || amendline2)
        {
            _amend = amendLine1P + amendLine2P;
            return true;
        }

        //对点修正
        GameVector2 _outAmend1;
        var _result1 = CollidePointAndCircle(_c1, _arcCen, _radius1, out _outAmend1);
        if (_result1)
        {
            _amend = _outAmend1;
            return true;
        }

        GameVector2 _outAmend2;
        var _result2 = CollidePointAndCircle(_c1, _point1, _radius1, out _outAmend2);

        GameVector2 _outAmend3;
        var _result3 = CollidePointAndCircle(_c1, _point2, _radius1, out _outAmend3);

        if (_result2 || _result3)
        {
            _amend = _outAmend2 + _outAmend3;
            return true;
        }

        return false;
    }

    // 圆和正圆扇面的碰撞,不带位置修正
    public static bool CollideCircleAndArcArea(GameVector2 _c1, int _radius1, GameVector2 _arcCen, int _arcRadius,
        int _arcAngle, int _arcAngleSize)
    {
        var radiusSum = _radius1 + _arcRadius;
        long sqrRadiusSum = radiusSum * radiusSum;
        var sqrDistance = _c1.sqrMagnitude(_arcCen);

        if (sqrDistance >= sqrRadiusSum) return false;
        var _angle = (_c1 - _arcCen).Angle();
        var _point1Angle = _arcAngle + _arcAngleSize;
        var _point2Angle = _arcAngle - _arcAngleSize;

        bool inArc;
        if (_point2Angle < 0)
        {
            var _angle1 = (int)Mathf.Repeat(_angle - _point2Angle, 360f);
            if (_angle1 >= 0 && _angle1 <= _point1Angle - _point2Angle)
                inArc = true;
            else
                inArc = false;
        }
        else if (_point1Angle > 360)
        {
            var _delA = _point1Angle - 360;
            var _angle1 = (int)Mathf.Repeat(_angle - _delA, 360f);
            if (_angle1 >= _point2Angle - _delA && _angle1 <= 360)
                inArc = true;
            else
                inArc = false;
        }
        else
        {
            if (_angle >= _point2Angle && _angle <= _point1Angle)
                inArc = true;
            else
                inArc = false;
        }

        if (inArc) return true;
        return false;
    }
}