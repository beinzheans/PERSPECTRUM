using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// A class to represent a group of the pause section within <see cref="BasePauseModule"/>.
/// </summary>
public class PauseMenuGroupObject : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public PauseMenuGroupData PauseMenuGroupData { get; private set; }
    private bool hasAssignedGroupData = false;
    [SerializeField] private TMP_Text groupLabel;
    [SerializeField] private TMP_InputField groupInputField;
    /// <summary>
    /// Note the slider will be from 0 to 1 by default.
    /// </summary>
    [SerializeField] private Slider groupSlider;

    [SerializeField] private Toggle groupToggle;

    public void SetGroupData(PauseMenuGroupData groupData)
    {
        PauseMenuGroupData = groupData;
        hasAssignedGroupData = true;
        groupLabel.text = PauseMenuGroupData.GroupLabel;

        groupInputField.gameObject.SetActive(false);
        groupSlider.gameObject.SetActive(false);
        groupToggle.gameObject.SetActive(false);
    }

    /// <summary>
    /// Sets the current group with actions and optional default values for the group.
    /// </summary>
    public void SetGroupAction_InputField(Action<string> inputFieldAction, string inputFieldValue = "")
    {
        if (!hasAssignedGroupData)
        {
            Debug.LogWarning($"Group {gameObject.name} has not assigned group data yet!", gameObject);
            return;
        }

        if (PauseMenuGroupData.GroupType.HasFlag(PauseMenuGroupType.INPUT_FIELD))
        {
            groupInputField.gameObject.SetActive(true);
            groupInputField.text = inputFieldValue;
            groupInputField.onEndEdit.AddListener(x => inputFieldAction?.Invoke(x));
        }
    }

    /// <summary>
    /// Sets the current group with actions and optional default values for the group. <br></br>
    /// Note the slider by default has values within [0, 1].
    /// </summary>

    public void SetGroupAction_Slider(Action<float> groupSliderAction, float sliderValue = 0f)
    {
        if (!hasAssignedGroupData)
        {
            Debug.LogWarning($"Group {gameObject.name} has not assigned group data yet!", gameObject);
            return;
        }

        if (PauseMenuGroupData.GroupType.HasFlag(PauseMenuGroupType.SLIDER))
        {
            groupSlider.gameObject.SetActive(true);
            groupSlider.value = sliderValue;
            groupSlider.onValueChanged.AddListener(x => groupSliderAction?.Invoke(x));
        }
    }

    /// <summary>
    /// Sets the current group with actions and optional default values for the group.
    /// </summary>
    public void SetGroupAction_Toggle(Action<bool> groupToggleAction, bool toggleValue = false)
    {
        if (!hasAssignedGroupData)
        {
            Debug.LogWarning($"Group {gameObject.name} has not assigned group data yet!", gameObject);
            return;
        }

        if (PauseMenuGroupData.GroupType.HasFlag(PauseMenuGroupType.TOGGLE_BUTTON))
        {
            groupToggle.gameObject.SetActive(true);
            groupToggle.isOn = toggleValue;
            groupToggle.onValueChanged.AddListener(x => groupToggleAction?.Invoke(x));
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!string.IsNullOrWhiteSpace(PauseMenuGroupData.GroupDescription))
        {
            GameManager.GameInstance.InvokeGamePauseDescriptionChanged(PauseMenuGroupData.GroupDescription);
        }
        else
        {
            GameManager.GameInstance.InvokeGamePauseDescriptionChanged(GamePauseManager.k_PAUSEMENUNODESCRIPTIONPROVIDED);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        GameManager.GameInstance.InvokeGamePauseDescriptionChanged(GamePauseManager.k_PAUSEMENUDEFAULTDESCRIPTION);
    }

    public void RemoveAllListeners()
    {
        groupInputField.onEndEdit.RemoveAllListeners();
        groupSlider.onValueChanged.RemoveAllListeners();
        groupToggle.onValueChanged.RemoveAllListeners();
    }
}
/// <summary>
/// A struct to represent the data of a group of the pause section within <see cref="BasePauseModule"/>.
/// </summary>

[Serializable]
public struct PauseMenuGroupData : IEquatable<PauseMenuGroupData>
{
    [SerializeField] private string groupLabel;
    
    public string GroupLabel { get => groupLabel; }

    [SerializeField] private PauseMenuGroupType groupType;
    public PauseMenuGroupType GroupType { get => groupType; }

    [TextArea]
    [SerializeField] private string groupDescription;

    public string GroupDescription { get => groupDescription; }

    public override bool Equals(object obj)
    {
        return obj is PauseMenuGroupData data && Equals(data);
    }

    public bool Equals(PauseMenuGroupData other)
    {
        return GroupLabel == other.GroupLabel &&
               GroupType == other.GroupType &&
               GroupDescription == other.GroupDescription;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(GroupLabel, GroupType, GroupDescription);
    }
}

/// <summary>
/// An enum (flag) to represent what kind of type <see cref="PauseMenuGroupData"/> is.
/// </summary>
[Flags]
public enum PauseMenuGroupType
{
    NONE = 0,
    INPUT_FIELD = 1,
    SLIDER = 2,
    TOGGLE_BUTTON = 4,

}
