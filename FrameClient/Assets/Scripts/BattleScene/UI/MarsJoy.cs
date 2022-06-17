using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MarsJoy : MonoBehaviour
{
    private Canvas _Canvas;
    private EventTrigger _EventTri;
    private RectTransform bgRect;
    private Image btnImage;

    private RectTransform joyBgRectTran;
    private Vector2 joyCenterPos;
    private Vector2 joyCenterPosMax;
    private Vector2 joyCenterPosMin;
    private float joyRadius;
    private RectTransform joyRectTran;
    private Vector2 joySpeed;

    private void Start()
    {
        _Canvas = transform.parent.GetComponent<Canvas>();
        _EventTri = GetComponent<EventTrigger>();
        btnImage = GetComponent<Image>();

        var _canvasRect = _Canvas.transform as RectTransform;
        bgRect = transform as RectTransform;
        bgRect.sizeDelta = new Vector2(_canvasRect.sizeDelta.x * 0.5f, _canvasRect.sizeDelta.y * 1.0f);

        joyBgRectTran = transform.Find("ImageJoyBG") as RectTransform;
        joyRectTran = joyBgRectTran.Find("ImageJoy") as RectTransform;

        joyRadius = joyBgRectTran.sizeDelta.x * 0.5f;

        var dis = joyRadius + 10f; //边距
        joyCenterPosMin = new Vector2(dis, dis);
        joyCenterPosMax = new Vector2(bgRect.sizeDelta.x - dis, bgRect.sizeDelta.y - dis);

        HideJoy();
    }

    public void EnableButton()
    {
        _EventTri.enabled = true;
        btnImage.raycastTarget = true;
    }

    public void DisableButton()
    {
        _EventTri.enabled = false;
        btnImage.raycastTarget = false;
    }

    private void ShowJoy()
    {
        joyBgRectTran.gameObject.SetActive(true);
        joySpeed = Vector2.zero;
        joyRectTran.anchoredPosition = Vector2.zero;
    }

    private void HideJoy()
    {
        joyBgRectTran.gameObject.SetActive(false);
    }

    public void OnClickDown(BaseEventData _data)
    {
        ShowJoy();
        var _pointData = (PointerEventData)_data;

        Vector2 _localPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(bgRect, _pointData.position, _Canvas.worldCamera,
            out _localPos);
        joyBgRectTran.anchoredPosition = _localPos;

        var _px = Mathf.Clamp(joyBgRectTran.anchoredPosition.x, joyCenterPosMin.x, joyCenterPosMax.x);
        var _py = Mathf.Clamp(joyBgRectTran.anchoredPosition.y, joyCenterPosMin.y, joyCenterPosMax.y);
        joyBgRectTran.anchoredPosition = new Vector2(_px, _py);

        joyCenterPos = RectTransformUtility.WorldToScreenPoint(_Canvas.worldCamera, joyBgRectTran.position);
    }

    public void OnClickUp()
    {
        HideJoy();

        BattleData.Instance.UpdateMoveDir(121);
    }

    public void DragBegin(BaseEventData _data)
    {
    }

    public void Drag(BaseEventData _data)
    {
        var _pointData = (PointerEventData)_data;

        var _joyPos = _pointData.position - joyCenterPos;
        joyRectTran.anchoredPosition = Vector3.ClampMagnitude(_joyPos, joyRadius);

        joySpeed = joyRectTran.anchoredPosition.normalized;

        var angle = Vector2.SignedAngle(Vector2.right, joySpeed);

        if (angle < 0) angle += 360f;

        var upDir = (int)(angle / 3f);

        BattleData.Instance.UpdateMoveDir(upDir);
//		Vector2 _ttVec = new Vector2 (Mathf.Cos(angle * Mathf.Deg2Rad),Mathf.Sin(angle * Mathf.Deg2Rad));
    }

    public void DragEnd(BaseEventData _data)
    {
//		Debug.Log (joySpeed);
    }
}