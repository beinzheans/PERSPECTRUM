using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public static GameManager GameInstance;
    [SerializeField] private RectTransform gameUIContainer;

    public RectTransform GameUIContainer { get => gameUIContainer; }
    public Vector2 GameNormalizedMousePosition { get; private set; }
    public PlayerInputActions InputActions { get; private set; }

    private void Awake()
    {
        if (GameInstance != null)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            GameInstance = this;
            DontDestroyOnLoad(gameObject);
        }

        InputActions = new();
        InputActions.Enable();
    }

    private void Update()
    {
        Vector2 mousePosition = InputActions.Gameplay.MousePosition.ReadValue<Vector2>();
        GameNormalizedMousePosition = MathHelper.GetNormalizedPointInsideReferenceUI(mousePosition, gameUIContainer);
    }

}
