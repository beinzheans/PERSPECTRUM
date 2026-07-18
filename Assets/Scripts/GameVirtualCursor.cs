using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

/// <summary>
/// A class to create a virtual cursor based on the hardware mouse.
/// </summary>
public class GameVirtualCursor : MonoBehaviour
{
    public static GameVirtualCursor GameVirtualCursorInstance;
    [SerializeField] private UIElasticColor mouseCursorElasticUI;
    [SerializeField] private Color mouseCursorPulseColor;

    [SerializeField] private Canvas mouseCanvas;
    private RectTransform mouseCanvasRectTransform;

    public const string k_VIRTUALMOUSEKEY = "VirtualMouse";

    /// <summary>
    /// The tag used to identify the virtual mouse. This is used inside the Input Action Asset
    /// </summary>
    public const string k_VIRTUALMOUSE_TAG = "VirtualMouseTag";
    public Mouse VirtualMouse { get; private set; }
    private Mouse hardwareMouse;

    public Vector2 VirtualMousePosition { get; private set; }
    public bool MouseVisibleState { get; private set; }

    public event Action<GameObject> OnVirtualCursorClickedUIElement;

    private readonly Vector2 k_VIRTUALCURSORCLICKEDELASTICSIZE = new Vector2(0.75f, 1.25f);
    private const double k_VIRTUALCURSORELASTICTIME = 0.25d;

    public Vector2 MouseDisplacement { get; private set; }
    private void Awake()
    {
        if (GameVirtualCursorInstance == null)
        {
            GameVirtualCursorInstance = this;
            DontDestroyOnLoad(gameObject);
            return;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (VirtualMouse == null) // nothing to remove
        {
            return;
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        InputSystem.RemoveDevice(VirtualMouse);
        InputSystem.RemoveDeviceUsage(VirtualMouse, k_VIRTUALMOUSE_TAG);
    }
    private bool wasLeftButtonClicked = false; // we do this because of some bs not exposing an event for left button

    private void Start()
    {
        hardwareMouse = Mouse.current; // cache hardware mouse, since virtual mouse & hardware mouse may conflict

        VirtualMouse = InputSystem.AddDevice<Mouse>(k_VIRTUALMOUSEKEY);

        InputSystem.AddDeviceUsage(VirtualMouse, k_VIRTUALMOUSE_TAG);

        mouseCursorElasticUI.RectTransform.anchorMin = mouseCursorElasticUI.RectTransform.anchorMax = Vector2.zero; // set to bottom-left anchor, so (0, 0) is the bottom-left corner
        mouseCanvasRectTransform = mouseCanvas.GetComponent<RectTransform>();
        Vector2 centre = MathHelper.GetScreenPointFromNormalizedPointInsideReferenceUI(new Vector2(0.5f, 0.5f), mouseCanvasRectTransform);
        mouseCursorElasticUI.RectTransform.anchoredPosition = centre;
        VirtualMousePosition = centre;

        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = false;


        ShowVirtualMouse();
    }

    private void Update()
    {
        UpdateVirtualMouse();
    }

    private const float k_DEFAULTMOUSESENSITIVITY = 1f;
    private void UpdateVirtualMouse()
    {
        Vector2 delta = hardwareMouse.delta.ReadValue();

        float sensitivity = GameManager.GameInstance.GlobalSettings.MouseSensitivityScaleFactor * k_DEFAULTMOUSESENSITIVITY; // even though the execution order is earlier, everything is already instantiatied in GameManager.

        MouseDisplacement = delta * sensitivity;

        VirtualMousePosition += MouseDisplacement;

        VirtualMousePosition = MathHelper.ClampVectorByComponent(VirtualMousePosition, 0f, Screen.width, 0f, Screen.height);

        SetEventSystem();
        UpdateVirtualCursorPosition();
    }

    private void SetEventSystem()
    {
        bool leftMouseButton = hardwareMouse.leftButton.isPressed;
        bool rightMouseButton = hardwareMouse.rightButton.isPressed;
        bool middleMouseButton = hardwareMouse.middleButton.isPressed;
        bool forwardMouseButton = hardwareMouse.forwardButton.isPressed;
        bool backMouseButton = hardwareMouse.backButton.isPressed;

        int clickCount = hardwareMouse.clickCount.ReadValue();
        Vector2 scroll = hardwareMouse.scroll.ReadValue();
        MouseState mouseState = new MouseState()
        {
            position = VirtualMousePosition,
            clickCount = (ushort)clickCount,
            scroll = scroll
        };

        mouseState.WithButton(MouseButton.Left, leftMouseButton);
        mouseState.WithButton(MouseButton.Right, rightMouseButton);
        mouseState.WithButton(MouseButton.Middle, middleMouseButton);
        mouseState.WithButton(MouseButton.Forward, forwardMouseButton);
        mouseState.WithButton(MouseButton.Back, backMouseButton);

        InputSystem.QueueStateEvent(VirtualMouse, mouseState);

        if (leftMouseButton && !wasLeftButtonClicked)
        {
            StartCoroutine(GetEventSystemLastRaycastEvent());
        }

        wasLeftButtonClicked = leftMouseButton;
    }

    private void UpdateVirtualCursorPosition()
    {
        MathHelper.GetNormalizedPointInsideReferenceUI(VirtualMousePosition, mouseCanvasRectTransform, out Vector2 normalizedPoint);
        mouseCursorElasticUI.RectTransform.anchorMax = mouseCursorElasticUI.RectTransform.anchorMin = normalizedPoint;
        mouseCursorElasticUI.RectTransform.anchoredPosition = Vector2.zero;
    }

    private IEnumerator GetEventSystemLastRaycastEvent()
    {
        // because the input system will actually process the state event in the next Update(), we need to wait until then to correctly get the last raycast result.

        yield return null;
        yield return new WaitForEndOfFrame();

        if (EventSystem.current == null)
        {
            yield break;
        }

        if (EventSystem.current.currentInputModule is not InputSystemUIInputModule uiInputModule)
        {
            yield break;
        }

        RaycastResult result = uiInputModule.GetLastRaycastResult(VirtualMouse.deviceId);

        if (!result.isValid)
        {
            yield break;
        }

        if (!result.gameObject.TryGetComponent<Selectable>(out Selectable selectable))
        {
            yield break;
        }

        if (!selectable.IsInteractable())
        {
            yield break;
        }

        mouseCursorElasticUI.PulseElasticSize(k_VIRTUALCURSORCLICKEDELASTICSIZE, k_VIRTUALCURSORELASTICTIME);
        mouseCursorElasticUI.PulseGraphicColor(mouseCursorPulseColor, k_VIRTUALCURSORELASTICTIME);
        OnVirtualCursorClickedUIElement?.Invoke(result.gameObject);
    }

    public void HideVirtualMouse()
    {
        if (mouseCursorElasticUI == null)
        {
            return;
        }

        mouseCursorElasticUI.gameObject.SetActive(false);
        MouseVisibleState = false;
    }

    public void ShowVirtualMouse()
    {
        if (mouseCursorElasticUI == null)
        {
            return;
        }

        mouseCursorElasticUI.gameObject.SetActive(true);
        MouseVisibleState = true;
    }
}
