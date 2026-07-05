using UnityEngine;

/// <summary>
/// A class to represent the base module of the pause section. <br></br>
/// Each module will define what settings can be changed.
/// </summary>
public abstract class BasePauseModule : MonoBehaviour
{
    /// <summary>
    /// An array storing the data for this pause module. Note the ordering matters.
    /// </summary>
    [SerializeField] protected PauseMenuGroupData[] pauseMenuGroupInfo = new PauseMenuGroupData[0];
    [SerializeField] protected string moduleName;
    public string ModuleName { get => moduleName; }
    /// <summary>
    /// The prefab that defines the group of this module.
    /// </summary>
    [SerializeField] private PauseMenuGroupObject pauseMenuGroupPrefab;

    /// <summary>
    /// The transform where the group will be displayed.
    /// </summary>
    [SerializeField] private RectTransform groupContentRectTransform;
    protected PauseMenuGroupObject[] pauseMenuGroups;

    private void Awake()
    {
        InstantiatePauseGroups();
        OnModuleAwake();
    }
    private void InstantiatePauseGroups()
    {
        pauseMenuGroups = new PauseMenuGroupObject[pauseMenuGroupInfo.Length];
        for (int i = 0; i < pauseMenuGroupInfo.Length; i++)
        {
            pauseMenuGroups[i] = Instantiate(pauseMenuGroupPrefab, groupContentRectTransform, false);
            pauseMenuGroups[i].SetGroupData(pauseMenuGroupInfo[i]);
            pauseMenuGroups[i].gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Custom implementation of events when the module is awake. Use this to cache calculation results.
    /// </summary>
    protected abstract void OnModuleAwake();
    public void InitializeModule()
    {
        for (int i = 0; i < pauseMenuGroupInfo.Length; i++)
        {
            pauseMenuGroups[i].gameObject.SetActive(true);
        }

        OnModuleInitialized();
    }

    /// <summary>
    /// Custom implementation of events when the module is initialized. You should add callbacks to the setting buttons here.
    /// </summary>
    /// <param name="index"></param>
    protected abstract void OnModuleInitialized();
    public void DeactiviateModule()
    {
        for (int i = 0; i < pauseMenuGroupInfo.Length; i++)
        {
            pauseMenuGroups[i].RemoveAllListeners();
            pauseMenuGroups[i].gameObject.SetActive(false);
        }
    }
}