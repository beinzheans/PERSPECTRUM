using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A class to represent interactable UI buttons to an enum.
/// </summary>
public abstract class EditorUIBehavior : MonoBehaviour
{
    [SerializeField] protected Button[] buttons;
    [SerializeField] protected int[] enumIndices;

    protected virtual void Start()
    {
        InitializeButtons();
    }

    protected virtual void InitializeButtons()
    {
        if (enumIndices.Length != buttons.Length)
        {
            return;
        }

        for (int i = 0; i < enumIndices.Length; i++)
        {
            int index = i;
            buttons[index].onClick.AddListener(() => UI_OnButtonPress(enumIndices[index]));
        }
    }

    /// <summary>
    /// Custom implementation of events when a tool button is clicked, defined by an index.
    /// </summary>
    /// <param name="index"></param>
    protected abstract void UI_OnButtonPress(int index);

    /// <summary>
    /// Assigns the state of the button corresponding to the provided enumIndex
    /// </summary>
    /// <param name="enumIndex"></param>
    /// <param name="state"></param>
    protected void SetButtonState(int enumIndex, bool state)
    {
        int buttonIndex = -1;
        for (int i = 0; i < buttons.Length; i++)
        {
            if (enumIndices[i] == enumIndex)
            {
                buttonIndex = i;
                break;
            }
        }

        if (buttonIndex == -1)
        {
            Debug.LogWarning($"Can not find cooresponding button index");
            return;
        }

        if (state == true)
        {
            buttons[buttonIndex].image.color = Color.yellow;
        }
        else
        {
            buttons[buttonIndex].image.color = Color.white;
        }
    }
}
