using UnityEngine;

public abstract class EditorToolManager : MonoBehaviour
{
    protected EditorManager editorInstance;
    protected bool IsActive;
    [SerializeField] protected string[] toolLabels;
    [SerializeField] protected bool[] isAlwaysActiveTool = new bool[9];
    [SerializeField] protected bool[] isToolToggle = new bool[9];
    protected bool[] toolActiveStates = new bool[9];

    protected virtual void Start()
    {
        if (toolLabels.Length < 0 || toolLabels.Length > 9)
        {
            Debug.LogWarning($"Invalid range of functions defined");
        }

        editorInstance = EditorManager.EditorInstance;

        editorInstance.OnEditorPlaceDeleteTypeChanged += EditorInstance_OnEditorPlaceDeleteTypeChanged;
        editorInstance.OnEditorToolkitButtonPressed += EditorInstance_OnEditorToolkitButtonPressed;
        GameManager.GameInstance.InputActions.Editor.ToolPositiveNegativeInput.performed += ToolPositiveNegativeInput_performed;
    }

    private void ToolPositiveNegativeInput_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        if (!IsActive)
        {
            return;
        }

        OnPositiveNegativeInput(obj.ReadValue<float>());
    }

    /// <summary>
    /// Implementation of custom events when positive/negative axis is pressed
    /// </summary>
    /// <param name="input"></param>
    protected abstract void OnPositiveNegativeInput(float input);
    /// <summary>
    /// Implementation of custom events when a tool button is pressed
    /// </summary>
    /// <param name="buttonIndex"></param>
    protected abstract void OnButtonPressedEvent(int buttonIndex);

    protected void EditorInstance_OnEditorToolkitButtonPressed(int buttonIndex)
    {
        if (!IsActive)
        {
            return;
        }

        if (isToolToggle[buttonIndex])
        {
            toolActiveStates[buttonIndex] = !toolActiveStates[buttonIndex];
            if (toolActiveStates[buttonIndex])
            {
                editorInstance.Buttons[buttonIndex].image.color = Color.yellow;
            }
            else
            {
                editorInstance.Buttons[buttonIndex].image.color = Color.white;
            }
        }

        OnButtonPressedEvent(buttonIndex);
    }

    protected void EditorInstance_OnEditorPlaceDeleteTypeChanged(ObjectPlaceDeleteType obj)
    {
        SetupToolkitPanel(obj);
    }

    /// <summary>
    /// Implementation of custom checks to determine if the tool is active.
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="validResult"></param>
    protected abstract void CheckForToolActiveState(ObjectPlaceDeleteType obj, out bool validResult);

    protected void SetupToolkitPanel(ObjectPlaceDeleteType obj)
    {
        CheckForToolActiveState(obj, out IsActive);

        if (!IsActive)
        {
            return;
        }

        for (int i = 0; i < 9; i++)
        {
            toolActiveStates[i] = isAlwaysActiveTool[i];
            editorInstance.Buttons[i].image.color = isAlwaysActiveTool[i] ? Color.yellow : Color.white;
            editorInstance.InputFields[i].text = "";

            if (i >= toolLabels.Length)
            {
                editorInstance.TextLabels[i].text = EditorManager.k_DEFAULTTOOLTEXTSTRING;
                editorInstance.Buttons[i].interactable = false;
                continue;
            }

            editorInstance.TextLabels[i].text = toolLabels[i];
            editorInstance.Buttons[i].interactable = !isAlwaysActiveTool[i];
        }

    }
}
