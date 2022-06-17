using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MarsButton : MonoBehaviour
{
    private Canvas _Canvas;
    private EventTrigger _EventTri;
    private Image btnImage;
    private Image countDownImage;
    private GameObject joyBgObj;
    private Vector2 joyCenterPos;
    private float joyRadius;
    private RectTransform joyRectTran;
    private Vector2 joySpeed;

    private void Start()
    {
        _Canvas = transform.parent.GetComponent<Canvas>();

        _EventTri = GetComponent<EventTrigger>();
        btnImage = GetComponent<Image>();
        countDownImage = transform.Find("countDown").GetComponent<Image>();

        joyBgObj = transform.Find("ImageJoyBG").gameObject;
        joyRectTran = joyBgObj.transform.Find("ImageJoy") as RectTransform;

        joyCenterPos = RectTransformUtility.WorldToScreenPoint(_Canvas.worldCamera, joyBgObj.transform.position);

        var _joyBgRect = joyBgObj.transform as RectTransform;
        joyRadius = _joyBgRect.sizeDelta.x * 0.5f;

        HideCountDown();
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
        joyBgObj.SetActive(true);
        joySpeed = Vector2.zero;
    }

    private void HideJoy()
    {
        joyBgObj.SetActive(false);
    }

    public void ShowCountDown()
    {
        countDownImage.gameObject.SetActive(true);
    }

    public void HideCountDown()
    {
        countDownImage.gameObject.SetActive(false);
    }

    public void OnClickDown()
    {
        ShowJoy();
    }

    public void OnClickUp()
    {
        HideJoy();
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
    }

    public void DragEnd(BaseEventData _data)
    {
        Debug.Log("end:" + joySpeed);
    }
}