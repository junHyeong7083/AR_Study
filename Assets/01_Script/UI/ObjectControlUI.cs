using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// 오브젝트 조작 UI (VR용 버튼)
/// VR 모드에서만 표시
/// 버튼 꾹 누르면 계속 동작
/// </summary>
public class ObjectControlUI : MonoBehaviour
{
    [Header("UI Panel")]
    [SerializeField] private GameObject controlPanel;

    [Header("Rotation Buttons")]
    [SerializeField] private Button rotateLeftButton;
    [SerializeField] private Button rotateRightButton;

    [Header("Scale Buttons")]
    [SerializeField] private Button scaleUpButton;
    [SerializeField] private Button scaleDownButton;

    [Header("Hold Settings")]
    [SerializeField] private float holdRepeatRate = 0.1f;  // 꾹 누를 때 반복 간격

    [Header("Target Object")]
    private ObjectInteraction targetObject;

    // 버튼 홀드 상태
    private bool isHoldingRotateLeft = false;
    private bool isHoldingRotateRight = false;
    private bool isHoldingScaleUp = false;
    private bool isHoldingScaleDown = false;
    private float holdTimer = 0f;

    private void Start()
    {
        // 버튼에 EventTrigger 추가 (누르고 있는 동안 감지)
        SetupButtonHold(rotateLeftButton, () => isHoldingRotateLeft = true, () => isHoldingRotateLeft = false);
        SetupButtonHold(rotateRightButton, () => isHoldingRotateRight = true, () => isHoldingRotateRight = false);
        SetupButtonHold(scaleUpButton, () => isHoldingScaleUp = true, () => isHoldingScaleUp = false);
        SetupButtonHold(scaleDownButton, () => isHoldingScaleDown = true, () => isHoldingScaleDown = false);

        // VR 모드에서만 패널 표시
        UpdatePanelVisibility();
    }

    private void SetupButtonHold(Button button, System.Action onDown, System.Action onUp)
    {
        if (button == null) return;

        EventTrigger trigger = button.gameObject.GetComponent<EventTrigger>();
        if (trigger == null)
            trigger = button.gameObject.AddComponent<EventTrigger>();

        // PointerDown
        EventTrigger.Entry entryDown = new EventTrigger.Entry();
        entryDown.eventID = EventTriggerType.PointerDown;
        entryDown.callback.AddListener((data) => onDown());
        trigger.triggers.Add(entryDown);

        // PointerUp
        EventTrigger.Entry entryUp = new EventTrigger.Entry();
        entryUp.eventID = EventTriggerType.PointerUp;
        entryUp.callback.AddListener((data) => onUp());
        trigger.triggers.Add(entryUp);

        // PointerExit (마우스가 버튼 밖으로 나가면 해제)
        EventTrigger.Entry entryExit = new EventTrigger.Entry();
        entryExit.eventID = EventTriggerType.PointerExit;
        entryExit.callback.AddListener((data) => onUp());
        trigger.triggers.Add(entryExit);
    }

    private void Update()
    {
        // 타겟 오브젝트 자동 찾기
        if (targetObject == null)
        {
            targetObject = FindObjectOfType<ObjectInteraction>();
        }

        // 버튼 홀드 처리
        if (targetObject != null)
        {
            holdTimer += Time.deltaTime;

            if (holdTimer >= holdRepeatRate)
            {
                holdTimer = 0f;

                if (isHoldingRotateLeft) targetObject.RotateLeft();
                if (isHoldingRotateRight) targetObject.RotateRight();
                if (isHoldingScaleUp) targetObject.ScaleUp();
                if (isHoldingScaleDown) targetObject.ScaleDown();
            }
        }
    }

    private void UpdatePanelVisibility()
    {
        if (controlPanel == null) return;

        // VR 모드에서만 표시
        if (XRPlatformManager.Instance != null)
        {
            controlPanel.SetActive(XRPlatformManager.Instance.IsVR);
        }
    }
}
