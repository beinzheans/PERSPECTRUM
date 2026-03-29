using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class EditorManager : MonoBehaviour
{
    public static EditorManager EditorInstance;
    private GameManager gameInstance;
    private PlayerInputActions inputAction;

    public Vector2 EditorMousePosition { get; private set; }

    public event Action<double> OnPreviewUpdated;

    public event Action OnEditorInstantiate;

    public event Action<IRenderable> OnRenderRenderable;
    public event Action<IRenderable> OnUnrenderRenderable;

    public List<IRenderable> EditorRenderables { get; private set; }
    // HITBOX EVENTS
    public event Action<EditorHitbox> OnEditorHitboxPlaced;
    public event Action<EditorHitbox> OnEditorHitboxSelected;
    public event Action<EditorHitbox> OnEditorHitboxDeleted;

    public event Action<ISelectable> OnEditorSelectedSelectable;
    public event Action<ISelectable> OnEditorDeselectedSelectable;

    public EditorChart CurrentEditorChart { get; private set; }
    public double EditorPreviewTime { get; private set; }

    [SerializeField] private double scrollSensitivity;
    [SerializeField] private double lookAheadTime;

    public double ScrollSensitivity { get => scrollSensitivity; }
    public double LookAheadTime { get => lookAheadTime; }

    private Vector2 snappedMouseCoordinate;
    private bool mouseSnapX = false;
    private bool mouseSnapY = false;

    private List<IRenderable> currentSelectedRenderables = new();
    public List<IRenderable> CurrentSelectedRenderables { get => currentSelectedRenderables; }
    private List<EditorHitbox> currentSelectedHitboxes = new();
    public List<EditorHitbox> CurrentSelectedHitboxes { get => currentSelectedHitboxes; }
    private void Awake()
    {
        if (EditorInstance != null)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            EditorInstance = this;
            DontDestroyOnLoad(EditorInstance);
        }
    }
    private void Start()
    {
        gameInstance = GameManager.GameInstance;
        inputAction = gameInstance.InputActions;

        inputAction.Editor.ScrollEditorTime.performed += ScrollEditorTime_performed;
        inputAction.Editor.MouseSnapAlongX.performed += MouseSnapAlongX_performed;
        inputAction.Editor.MouseSnapAlongY.performed += MouseSnapAlongY_performed;
        StartEditor();
    }

    private void MouseSnapAlongY_performed(InputAction.CallbackContext obj)
    {
        snappedMouseCoordinate = gameInstance.GameNormalizedMousePosition;
        mouseSnapY = !mouseSnapY;
        mouseSnapX = false;
    }

    private void MouseSnapAlongX_performed(InputAction.CallbackContext obj)
    {
        snappedMouseCoordinate = gameInstance.GameNormalizedMousePosition;
        mouseSnapX = !mouseSnapX;
        mouseSnapY = false;
    }

    private void StartEditor()
    {
        Debug.Log($"Starting editor");
        CurrentEditorChart = new(
            new List<EditorHitbox>()
            {
                new EditorHitbox(new Vector2(0.5f, 0.5f), 100, HitboxType.A, 1d),
                new EditorHitbox(new Vector2(0.55f, 0.55f), 50, HitboxType.B, 2d),
                new EditorHitbox(new Vector2(0.6f, 0.6f), 100, HitboxType.A, 3d),
                new EditorHitbox(new Vector2(0.65f, 0.65f), 50, HitboxType.B, 4d),
                new EditorHitbox(new Vector2(0.7f, 0.7f), 100, HitboxType.A, 5d),
                new EditorHitbox(new Vector2(0.75f, 0.25f), 50, HitboxType.B, 6d),
                new EditorHitbox(new Vector2(0.75f, 0.30f), 50, HitboxType.B, 6.05d),
                new EditorHitbox(new Vector2(0.75f, 0.35f), 50, HitboxType.B, 6.10d),
                new EditorHitbox(new Vector2(0.75f, 0.40f), 50, HitboxType.B, 6.15d),
                new EditorHitbox(new Vector2(0.75f, 0.45f), 50, HitboxType.B, 6.20d),
                new EditorHitbox(new Vector2(0.75f, 0.50f), 50, HitboxType.B, 6.25d),
                new EditorHitbox(new Vector2(0.75f, 0.55f), 50, HitboxType.B, 6.30d),
                new EditorHitbox(new Vector2(0.75f, 0.60f), 50, HitboxType.B, 6.35d),
                new EditorHitbox(new Vector2(0.75f, 0.65f), 50, HitboxType.B, 6.40d),
                new EditorHitbox(new Vector2(0.75f, 0.70f), 50, HitboxType.B, 6.45d),
                new EditorHitbox(new Vector2(0.75f, 0.75f), 50, HitboxType.B, 6.50d)
            },
            new List<EditorPoint>()
            {
                new EditorPoint(new Vector2(0.5f, 0.5f), 10d),
                new EditorPoint(new Vector2(0.6f, 0.6f), 11d),
                new EditorPoint(new Vector2(0.7f, 0.7f), 12d),
            });

        EditorRenderables = new();
        OnEditorInstantiate?.Invoke();
    }

    private void Update()
    {
        Vector2 normalizedMousePosition = gameInstance.GameNormalizedMousePosition;

        if (mouseSnapX)
        {
            EditorMousePosition = new Vector2(normalizedMousePosition.x, snappedMouseCoordinate.y);
        }
        else if (mouseSnapY)
        {
            EditorMousePosition = new Vector2(snappedMouseCoordinate.x, normalizedMousePosition.y);
        }
        else
        {
            EditorMousePosition = normalizedMousePosition;
        }
    }

    private void ScrollEditorTime_performed(InputAction.CallbackContext obj)
    {
        double delta = scrollSensitivity;
        if (obj.ReadValue<float>() > 0f)
        {
            delta *= 1d;
        }
        else
        {
            delta *= -1d;
        }

        UpdateEditorPreviewTimeByDelta(delta);
    }
    public void UpdateEditorPreviewTime(double newTime)
    {
        EditorPreviewTime = Math.Max(0d, newTime);
        InvokeEditorPreviewUpdateEvent();
    }

    public void UpdateEditorPreviewTimeByDelta(double deltaTime)
    {
        EditorPreviewTime = Math.Max(0d, EditorPreviewTime + deltaTime);
        InvokeEditorPreviewUpdateEvent();
    }

    public void UpdateEditorRenderableList(List<IRenderable> renderables)
    {
        EditorRenderables = renderables;
        InvokeEditorPreviewUpdateEvent();
    }

    public void PlaceEditorHitbox(EditorHitbox h)
    {
        CurrentEditorChart.PlaceHitbox(h);
        OnEditorHitboxPlaced?.Invoke(h);
        InvokeEditorPreviewUpdateEvent();
    }

    public void DeleteSelectedEditorHitboxList()
    {
        for (int i = 0; i < currentSelectedHitboxes.Count; i++)
        {
            EditorHitbox h = currentSelectedHitboxes[i];
            CurrentEditorChart.DeleteHitbox(h);
            OnEditorHitboxDeleted?.Invoke(h);
        }

        currentSelectedHitboxes = new();
        InvokeEditorPreviewUpdateEvent();
    }

    public void InvokeEditorPreviewUpdateEvent() => OnPreviewUpdated?.Invoke(EditorPreviewTime);
    public void InvokeRenderRenderableEvent(IRenderable r) => OnRenderRenderable?.Invoke(r);
    public void InvokeUnrenderRenderableEvent(IRenderable r) => OnUnrenderRenderable?.Invoke(r);
    public void InvokeSelectSelectableEvent(ISelectable s)
    {
        OnEditorSelectedSelectable?.Invoke(s);
        InvokeEditorPreviewUpdateEvent();
    }
    public void InvokeDeselectSelectableEvent(ISelectable s)
    {
        OnEditorDeselectedSelectable?.Invoke(s);
        InvokeEditorPreviewUpdateEvent();
    }
}


public class EditorChart
{
    public EditorChart(List<EditorHitbox> hitboxes, List<EditorPoint> points)
    {
        Hitboxes = hitboxes;
        Points = points;
    }

    public List<EditorHitbox> Hitboxes;
    public List<EditorPoint> Points;
    public void PlaceHitbox(EditorHitbox h)
    {
        h.OnPlace(ref Hitboxes);
    }
    public void PlacePoint(EditorPoint p)
    {
        p.OnPlace(ref Points);
    }

    public void DeleteHitbox(EditorHitbox h)
    {
        h.OnDelete(ref Hitboxes);
    }
    public void DeletePoint(EditorPoint p)
    {
        p.OnDelete(ref Points);
    }
}
