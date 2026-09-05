using Newtonsoft.Json.Linq;
using SFB;
using Steamworks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Profiling.Memory.Experimental;
using UnityEngine.UI;

public class ChartChooseManager : MonoBehaviour
{
    public static ChartChooseManager ChartChooseInstance;
    [SerializeField] private Button importChartButton;
    [SerializeField] private Button returnMainMenuButton;

    public event Action<ChartGameplayRecordButtonBehavior> OnChartRecordButtonClicked;
    public event Action<ChartButtonBehaviorContents, int> OnChartButtonClicked;
    public event Action<ChartButtonBehaviorContents> OnChartButtonNeededAdd;

    /// <summary>
    /// Returns the current selected chart button and it's associated content ID as a tuple.
    /// </summary>
    public event Func<(ChartButtonBehaviorContents, int)> OnRequestCurrentSelectedChartButton;
    public event Action<string> OnChartDeleted;

    public event Action<ChartButtonSortOptions> OnSortOptionSelected;
    public event Action<ChartSortOrder> OnSortOrderChanged;

    public int CurrentSelectChartContentID { get; private set; }

    private void Awake()
    {
        ChartChooseInstance = this;
    }

    private void Start()
    {
        SteamManager.SteamInstance.OnChartInstalledInSteamStorage += SteamInstance_OnChartInstalledInSteamStorage;
        SteamManager.SteamInstance.OnSteamWorkshopUnsubscribed += SteamInstance_OnChartDeletedInSteamStorage;
    }

    private void SteamInstance_OnChartDeletedInSteamStorage(string STEAM_folderPath)
    {
        List<string> validSteamFiles = GetChartFilesFromSteamFolders(STEAM_folderPath);

        for (int i = 0; i < validSteamFiles.Count; i++)
        {
            RemoveChartWithPath(validSteamFiles[i]);
        }
    }

    private void SteamInstance_OnChartInstalledInSteamStorage(string folderPath_STEAM)
    {
        List<string> validSteamFiles = GetChartFilesFromSteamFolders(folderPath_STEAM);

        for (int i = 0; i < validSteamFiles.Count; i++)
        {
            AddChartButton(validSteamFiles[i]);
        }
    }

    private void OnDestroy()
    {
        ChartChooseInstance = null;
    }

    private void OnDisable()
    {
        SteamManager.SteamInstance.OnChartInstalledInSteamStorage -= SteamInstance_OnChartInstalledInSteamStorage;
        SteamManager.SteamInstance.OnSteamWorkshopUnsubscribed -= SteamInstance_OnChartDeletedInSteamStorage;
    }

    public void InitializeChartButtonsFromFile()
    {
        GamePersistenceManager.ReadEditorChartsInGameStorage(out string[] allLocalPaths);
        string[] allSteamFolderPaths = SteamManager.SteamInstance.RequestChartsInLocalSteamStorage();

        List<string> validSteamFiles = GetChartFilesFromSteamFolders(allSteamFolderPaths);
        List<string> allFiles = new List<string>(allLocalPaths.Length + validSteamFiles.Count);

        allFiles.AddRange(allLocalPaths);
        allFiles.AddRange(validSteamFiles);

        for (int i = 0; i < allFiles.Count; i++)
        {
            AddChartButton(allFiles[i]);
        }
    }

    private List<string> GetChartFilesFromSteamFolders(string[] allSteamFolders)
    {
        List<string> result = new List<string>(allSteamFolders.Length);
        for (int i = 0; i < allSteamFolders.Length; i++)
        {
            result.AddRange(GetChartFilesFromSteamFolders(allSteamFolders[i]));
        }

        return result;
    }
    private List<string> GetChartFilesFromSteamFolders(string allSteamFolders)
    {
        List<string> result = new List<string>();
        string path = allSteamFolders;

        if (string.IsNullOrWhiteSpace(path))
        {
            return result;
        }

        if (!Directory.Exists(path))
        {
            return result;
        }

        string[] files = Directory.GetFiles(path);

        for (int j = 0; j < files.Length; j++)
        {
            if (Path.GetExtension(files[j]).TrimStart('.').ToLowerInvariant() != GameManager.k_FILEEXTENSION)
            {
                continue;
            }

            result.Add(files[j]);
        }

        return result;
    }

    private void AddChartButton(string path)
    {
        GamePersistenceManager.GetMetadataJsonOfEditorChartPath(path, out string metadataJson);

        if (string.IsNullOrWhiteSpace(metadataJson))
        {
            Debug.Log($"Removed chart due to null / empty JSON. Path:\n" +
                      $"{path}");

            File.Delete(path);
            return;
        }

        JObject metadataJObject = JObject.Parse(metadataJson);
        if (!GameVersionConverter.GetBaseDetailsFromMetadataJObject(metadataJObject, out BaseChartMetadata baseChartMetadata))
        {
            Debug.Log($"Removed chart due to unsupported file. Path:\n" +
                      $"{path}");
            GameManager.GameInstance.InvokeInformationDisplayNeeded("Ignored and deleted invalid chart. Check log.", 1d);
            File.Delete(path);
            return;
        }

        ChartButtonBehaviorContents contents = new ChartButtonBehaviorContents(baseChartMetadata, path);

        OnChartButtonNeededAdd?.Invoke(contents);
    }

    public void UI_ImportButtonClicked()
    {
        ConfirmAction confirmAction = new ConfirmAction(ImportChartFromLocalStorage, ImportChartFromSteamWorkshop, "Do you want to import from local storage or from Steam workshop?", "Local Storage", "Steam Workshop");
        GameManager.GameInstance.InvokeConfirmActionNeeded(confirmAction);
    }

    private void ImportChartFromLocalStorage()
    {
        string[] paths = StandaloneFileBrowser.OpenFilePanel("Import Chart", "", GameManager.k_FILEEXTENSION, false);

        if (paths.Length <= 0)
        {
            return;
        }

        if (!GamePersistenceManager.ImportEditorChartToGameStorage(paths[0], out string internalChartPath))
        {
            GameManager.GameInstance.InvokeInformationDisplayNeeded("Failed to import chart", 1d);
            return;
        }

        AddChartButton(internalChartPath);
    }

    private void ImportChartFromSteamWorkshop()
    {
        string link = SteamWorkshopManager.k_STEAM_WORKSHOP_FIXEDURL;
        SteamFriends.ActivateGameOverlayToWebPage(link);
    }

    public void UI_ReturnMainMenuButton()
    {
        SceneLoader.SceneLoaderInstance.LoadSceneByName(SceneLoader.k_TITLESCREENINDEX, () => Task.CompletedTask);
    }

    public void UI_SettingsButton()
    {
        GameManager.GameInstance.RequestOverrideGamePauseState(true);
    }

    public void RequestRemoveChart()
    {
        var request = OnRequestCurrentSelectedChartButton?.Invoke();
        if (request == null)
        {
            return;
        }

        ChartButtonBehaviorContents contentsToDelete = request.Value.Item1;
        if (contentsToDelete == null)
        {
            return;
        }

        RemoveChartWithPath(contentsToDelete.AssociatedFullFilePath);
    }

    private void RemoveChartWithPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        if (!File.Exists(path))
        {
            Debug.LogWarning($"The path does not exist, ignoring delete request!");
            return;
        }

        OnChartDeleted?.Invoke(path);
        File.Delete(path);
        Debug.Log($"Deleted {path}");
    }

    public void RequestPlayChart()
    {
        var request = OnRequestCurrentSelectedChartButton?.Invoke();

        if (request == null)
        {
            return;
        }

        ChartButtonBehaviorContents contents = request.Value.Item1;

        if (contents == null)
        {
            return;
        }

        GameManager.GameInstance.RequestPlayChartEvent(contents.AssociatedFullFilePath);
    }

    public void RequestReplayChart(GameplayStatisticRecord record)
    {
        var request = OnRequestCurrentSelectedChartButton?.Invoke();

        if (request == null)
        {
            return;
        }

        ChartButtonBehaviorContents contents = request.Value.Item1;

        if (contents == null)
        {
            return;
        }

        GameManager.GameInstance.RequestReplayChartEvent(contents.AssociatedFullFilePath, record);
    }
    public void InvokeOnChartButtonClickedEvent(ChartButtonBehaviorContents contents, int contentID)
    {
        CurrentSelectChartContentID = contentID;
        OnChartButtonClicked?.Invoke(contents, contentID);
    }
    public void InvokeOnChartRecordButtonClickedEvent(ChartGameplayRecordButtonBehavior recordButton)
    {
        OnChartRecordButtonClicked?.Invoke(recordButton);
    }

    public void InvokeOnChartSortingOptionSelectedEvent(ChartButtonSortOptions sortingOption)
    {
        OnSortOptionSelected?.Invoke(sortingOption);
    }

    public void InvokeOnChartOrderingOptionEvent(ChartSortOrder sortOrder)
    {
        OnSortOrderChanged?.Invoke(sortOrder);
    }
}
