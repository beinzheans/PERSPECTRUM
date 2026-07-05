using System;
using System.Collections.Generic;
using System.Linq;
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

    [SerializeField] private TMP_Dropdown groupDropdown;

    [SerializeField] private Button groupButton;

    public void SetGroupData(PauseMenuGroupData groupData)
    {
        PauseMenuGroupData = groupData;
        hasAssignedGroupData = true;
        groupLabel.text = PauseMenuGroupData.GroupLabel;

        groupInputField.gameObject.SetActive(false);
        groupSlider.gameObject.SetActive(false);
        groupToggle.gameObject.SetActive(false);
        groupDropdown.gameObject.SetActive(false);
        groupButton.gameObject.SetActive(false);
    }

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
            groupInputField.SetTextWithoutNotify(inputFieldValue);
            groupInputField.onEndEdit.AddListener(x => inputFieldAction?.Invoke(x));
        }
    }

    /// <summary>
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
            groupSlider.SetValueWithoutNotify(sliderValue);
            groupSlider.onValueChanged.AddListener(x => groupSliderAction?.Invoke(x));
        }
    }

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
            groupToggle.SetIsOnWithoutNotify(toggleValue);
            groupToggle.onValueChanged.AddListener(x => groupToggleAction?.Invoke(x));
        }
    }

    /// <summary>
    /// Sets the dropdown menu given a generic enum type. <br></br>
    /// Keep in mind that the dropdown uses 0 index while <typeparamref name="T"/> may not.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="groupDropdownAction"></param>
    /// <param name="defaultDropdownValue"></param>
    public void SetGroupAction_Dropdown<T>(Action<int> groupDropdownAction, T defaultDropdownValue) where T : struct, Enum
    {
        if (!hasAssignedGroupData)
        {
            Debug.LogWarning($"Group {gameObject.name} has not assigned group data yet!", gameObject);
            return;
        }

        if (PauseMenuGroupData.GroupType.HasFlag(PauseMenuGroupType.DROP_DOWN))
        {
            groupDropdown.ClearOptions();
            groupDropdown.gameObject.SetActive(true);

            List<string> enumOptions = Enum.GetNames(typeof(T)).ToList();
            groupDropdown.AddOptions(enumOptions);

            string defaultOption = Enum.GetName(typeof(T), defaultDropdownValue);
            for (int i = 0; i < enumOptions.Count; i++)
            {
                if (string.Equals(enumOptions[i], defaultOption, StringComparison.Ordinal))
                {
                    groupDropdown.SetValueWithoutNotify(i);
                    break;
                }
            }

            groupDropdown.onValueChanged.AddListener(x => groupDropdownAction?.Invoke(x));

        }

    }

    /// <summary>
    /// Sets the dropdown menu given a list of options.
    /// </summary>
    /// <param name="options"></param>
    /// <param name="groupDropdownAction"></param>
    /// <param name="defaultDropdownValue"></param>
    public void SetGroupAction_Dropdown(List<string> options, Action<int> groupDropdownAction, string defaultDropdownValue)
    {
        if (!hasAssignedGroupData)
        {
            Debug.LogWarning($"Group {gameObject.name} has not assigned group data yet!", gameObject);
            return;
        }

        if (PauseMenuGroupData.GroupType.HasFlag(PauseMenuGroupType.DROP_DOWN))
        {
            groupDropdown.ClearOptions();
            groupDropdown.gameObject.SetActive(true);
            groupDropdown.AddOptions(options);

            for (int i = 0; i < options.Count; i++)
            {
                if (string.Equals(options[i], defaultDropdownValue, StringComparison.Ordinal))
                {
                    groupDropdown.SetValueWithoutNotify(i);
                    break;
                }
            }
            
            groupDropdown.onValueChanged.AddListener(x => groupDropdownAction?.Invoke(x));

        }
    }

    public void SetGroupAction_Button(Action groupButtonAction)
    {
        if (!hasAssignedGroupData)
        {
            Debug.LogWarning($"Group {gameObject.name} has not assigned group data yet!", gameObject);
            return;
        }

        if (PauseMenuGroupData.GroupType.HasFlag(PauseMenuGroupType.CLICK_BUTTON))
        {
            groupButton.gameObject.SetActive(true);
            groupButton.onClick.AddListener(() => groupButtonAction?.Invoke());
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
    /// <summary>
    /// Adds an input field to the group. The input field will allow for any string, so add validation for specific use-cases.
    /// </summary>
    INPUT_FIELD = 1,
    /// <summary>
    /// Adds a slider to the group. The slider will have a range of [0, 1].
    /// </summary>
    SLIDER = 2,
    /// <summary>
    /// Adds a toggle button to the group. To detect button presses, use <see cref="CLICK_BUTTON"/>.
    /// </summary>
    TOGGLE_BUTTON = 4,
    /// <summary>
    /// Adds an drop down menu to the group. The drop down menu is set based on a provided <see cref="Enum"/>.
    /// </summary>
    DROP_DOWN = 8,
    /// <summary>
    /// Adds a button to the group. To toggle between states, use <see cref="TOGGLE_BUTTON"/>.
    /// </summary>
    CLICK_BUTTON = 16
}
