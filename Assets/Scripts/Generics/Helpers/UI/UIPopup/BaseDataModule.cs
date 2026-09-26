using System;
using UnityEngine;

/// <summary>
/// A class to represent the base module of a <see cref="BaseUIPopupContent"/>. <br></br>
/// Each module will define what settings can be changed, defined by <see cref="BaseModuleData"/>.
/// </summary>
public abstract class BaseDataModule : MonoBehaviour
{
    /// <summary>
    /// An array storing the data for this data module. Note the ordering matters.
    /// </summary>
    [SerializeField] protected BaseModuleData[] moduleDataGroupInfo = new BaseModuleData[0];
    [SerializeField] protected string moduleName;
    public string ModuleName { get => moduleName; }
    /// <summary>
    /// The prefab that defines the group of this module.
    /// </summary>
    [SerializeField] private ModuleDataGroupObject moduleDataGroupPrefab;

    /// <summary>
    /// The transform where the group will be displayed.
    /// </summary>
    [SerializeField] private RectTransform groupContentRectTransform;
    protected ModuleDataGroupObject[] moduleDataGroups;

    private bool isModuleActive;

    private void Awake()
    {
        InstantiateDataGroups();
        OnModuleAwake();
    }
    private void InstantiateDataGroups()
    {
        moduleDataGroups = new ModuleDataGroupObject[moduleDataGroupInfo.Length];
        for (int i = 0; i < moduleDataGroupInfo.Length; i++)
        {
            moduleDataGroups[i] = Instantiate(moduleDataGroupPrefab, groupContentRectTransform, false);
            moduleDataGroups[i].SetGroupData(moduleDataGroupInfo[i]);
            moduleDataGroups[i].gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Custom implementation of events when the module is awake. Use this to cache calculation results.
    /// </summary>
    protected abstract void OnModuleAwake();
    public void InitializeModule()
    {
        if (isModuleActive)
        {
            OnModuleInitialized(); // only reset the values
            return;
        }

        for (int i = 0; i < moduleDataGroupInfo.Length; i++)
        {
            moduleDataGroups[i].gameObject.SetActive(true);
        }

        isModuleActive = true;
        OnModuleInitialized();
    }

    /// <summary>
    /// Custom implementation of events when the module is initialized. You should add callbacks to the setting buttons here.
    /// </summary>
    /// <param name="index"></param>
    protected abstract void OnModuleInitialized();
    public void DeactiviateModule()
    {
        if (!isModuleActive)
        {
            return;
        }

        for (int i = 0; i < moduleDataGroupInfo.Length; i++)
        {
            moduleDataGroups[i].RemoveAllListeners();
            moduleDataGroups[i].gameObject.SetActive(false);
        }

        isModuleActive = false;
    }
}