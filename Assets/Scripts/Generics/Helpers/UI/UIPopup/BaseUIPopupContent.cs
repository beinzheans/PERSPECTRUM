using System;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// The base behavior to allow for UI elements to be created onto a panel that pops up using a data-driven script. <br></br>
/// This base behavior allows for multiple kinds of input options (see <see cref="BaseModuleDataType"/> for the avaliable list of input options).
/// </summary>
public class BaseUIPopupContent : MonoBehaviour
{
    /// <summary>
    /// The number of modules we attach to this popup content, allowing for multiple variety of content using the same panel organised by modules.
    /// </summary>
    [SerializeField] protected BaseDataModule[] dataModules = new BaseDataModule[0];

    /// <summary>
    /// The prefab used to render the module buttons.
    /// </summary>
    [SerializeField] private Button dataModuleButtonPrefab;

    private Button[] spawnedDataModuleButtons;

    /// <summary>
    /// The parent that the module buttons will be in.
    /// </summary>
    [SerializeField] protected RectTransform dataModuleButtonRectTransform;

    [SerializeField] private UIPopupType popupType = UIPopupType.DISPLAY_MODULE_NAME;
    protected virtual void Start()
    {
        SetupDataModules();
    }

    private void SetupDataModules()
    {
        if (dataModules.Length <= 0)
        {
            Debug.Log($"No data module assigned to UI popup.", gameObject);
            return;
        }

        switch (popupType)
        {
            case UIPopupType.DISPLAY_MODULE_NAME:
            default:
                spawnedDataModuleButtons = new Button[dataModules.Length];
                for (int i = 0; i < dataModules.Length; i++)
                {
                    int index = i;
                    spawnedDataModuleButtons[index] = Instantiate(dataModuleButtonPrefab, dataModuleButtonRectTransform, false);
                    spawnedDataModuleButtons[index].GetComponentInChildren<TMP_Text>().text = dataModules[index].ModuleName; // this will be fine, we do it once only!
                    spawnedDataModuleButtons[index].onClick.AddListener(() => SwitchActiveDataModuleAtIndex(index));
                }

                break;
            case UIPopupType.DO_NOT_DISPLAY_MODULES:
                if (dataModules.Length >= 2)
                {
                    Debug.LogWarning($"UI popup does not display module buttons, yet more than 1 modules were assigned! Only the first module is visible.", gameObject);
                }

                break;
        }

        SwitchActiveDataModuleAtIndex(0);
    }

    protected void SwitchActiveDataModuleAtIndex(int index)
    {
        for (int i = 0; i < dataModules.Length; i++)
        {
            dataModules[i].DeactiviateModule(); // remove all listeners, since it could be stale.

            if (i == index)
            {
                dataModules[index].InitializeModule();
                if (popupType == UIPopupType.DO_NOT_DISPLAY_MODULES)
                {
                    continue;
                }

                spawnedDataModuleButtons[i].image.color = Color.yellow;
            }
            else
            {
                if (popupType == UIPopupType.DO_NOT_DISPLAY_MODULES)
                {
                    continue;
                }

                spawnedDataModuleButtons[i].image.color = Color.white;
            }
        }
    }

    protected virtual void OnDestroy()
    {
        for (int i = 0; i < dataModules.Length; i++)
        {
            dataModules[i].DeactiviateModule();

            if (popupType == UIPopupType.DO_NOT_DISPLAY_MODULES)
            {
                continue;
            }

            spawnedDataModuleButtons[i].onClick.RemoveAllListeners();
        }
    }
}

/// <summary>
/// An enum to define what the type of the UI popup is.
/// </summary>
[Serializable]
public enum UIPopupType
{
    /// <summary>
    /// Displays the entire module as a new interactable button. <see cref="BaseUIPopupContent.dataModuleButtonPrefab"/> must be set for this to be valid.
    /// </summary>
    DISPLAY_MODULE_NAME = 0,
    /// <summary>
    /// Does not display the modules as interactable buttons. Note this means that switching between modules is impossible, unless if it is handled elsewhere.
    /// </summary>
    DO_NOT_DISPLAY_MODULES = 1
}