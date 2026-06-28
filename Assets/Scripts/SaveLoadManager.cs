using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


public static class SaveLoadManager
{
    public static readonly byte[] audioEncryptionBytes = Encoding.UTF8.GetBytes("PleaseDontCrackThisKey");

    /// <summary>
    /// Saves a chart file to a file destination given the JSON and audio byte array information 
    /// </summary>
    /// <param name="fullFilePath"></param>
    /// <param name="chartJson"></param>
    /// <param name="audioByte"></param>
    public static void SaveAsChartFile(string fullFilePath, string chartJson, string metadataJson, byte[] audioByte)
    {
        MemoryStream memoryStream = new MemoryStream();

        ZipArchive archive = new ZipArchive(memoryStream, ZipArchiveMode.Create);

        ZipArchiveEntry chartJsonEntry = archive.CreateEntry(GameManager.k_CHARTFILENAME);

        StreamWriter jsonWriter = new StreamWriter(chartJsonEntry.Open());
        jsonWriter.Write(chartJson);
        jsonWriter.Close();

        ZipArchiveEntry metadataJsonEntry = archive.CreateEntry(GameManager.k_METADATAFILENAME);

        StreamWriter metadataWriter = new StreamWriter(metadataJsonEntry.Open());
        metadataWriter.Write(metadataJson);
        metadataWriter.Close();

        ZipArchiveEntry audioEntry = archive.CreateEntry(GameManager.k_AUDIOFILENAME);

        Stream audioWriter = audioEntry.Open();
        audioWriter.Write(audioByte);
        audioWriter.Close();

        archive.Dispose();

        byte[] archiveBytes = memoryStream.ToArray();

        File.WriteAllBytes(fullFilePath, XorProcesser(archiveBytes));
        memoryStream.Close();
    }

    public static byte[] XorProcesser(byte[] bytes)
    {
        byte[] result = new byte[bytes.Length];

        for (int i = 0; i < bytes.Length; i++)
        {
            result[i] = (byte)(bytes[i] ^ audioEncryptionBytes[i % audioEncryptionBytes.Length]);
        }

        return result;
    }

    /// <summary>
    /// Converts a chart file to JSON and audio byte array information if possible. <br></br>
    /// Returns false and empty chart information if no JSON nor audio byte array is valid.
    /// </summary>
    /// <param name="fullFilePath"></param>
    /// <param name="chartJson"></param>
    /// <param name="audioByte"></param>
    /// <returns></returns>
    public static void LoadChartFile(string fullFilePath, out string chartJson, out string metadataJson, out byte[] audioByte)
    {
        bool isValid = GameArchiveValidator.GetArchiveFileBytes(fullFilePath, out byte[] archiveBytes);

        if (!isValid)
        {
            chartJson = "";
            metadataJson = "";
            audioByte = new byte[0];
            return;
        }

        MemoryStream stream = new MemoryStream(archiveBytes);
        ZipArchive archive = new ZipArchive(stream, ZipArchiveMode.Read);

        ZipArchiveEntry jsonEntry = archive.GetEntry(GameManager.k_CHARTFILENAME);

        if (jsonEntry == null)
        {
            chartJson = "";
        }
        else
        {
            StreamReader jsonReader = new StreamReader(jsonEntry.Open());

            chartJson = jsonReader.ReadToEnd();
            jsonReader.Close();
        }

        ZipArchiveEntry metadataEntry = archive.GetEntry(GameManager.k_METADATAFILENAME);

        if (metadataEntry == null)
        {
            metadataJson = "";
        }
        else
        {
            StreamReader metadataReader = new StreamReader(metadataEntry.Open());

            metadataJson = metadataReader.ReadToEnd();
            metadataReader.Close();
        }

        ZipArchiveEntry audioEntry = archive.GetEntry(GameManager.k_AUDIOFILENAME);

        if (audioEntry == null)
        {
            audioByte = new byte[0];
        }
        else
        {
            Stream audioReader = audioEntry.Open();
            MemoryStream memoryStream = new MemoryStream();

            audioReader.CopyTo(memoryStream);
            audioByte = memoryStream.ToArray();

            memoryStream.Close();
            audioReader.Close();
        }

        archive.Dispose();
        stream.Close();

    }

    private const string k_TEMPORARYFILENAME = "temporary_cache.mp3";
    /// <summary>
    /// Gets an audio clip from the bytes by using temporary file cache.
    /// </summary>
    /// <param name="audioByte"></param>
    /// <param name="clip"></param>
    /// <returns></returns>
    public static async Task<(bool, AudioClip, byte[])> GetAudioClipFromByteArray(byte[] audioByte)
    {
        if (audioByte == null || audioByte.Length <= 0)
        {
            return (false, null, new byte[0]);
        }

        string tempFilePath = Path.Combine(Application.temporaryCachePath, k_TEMPORARYFILENAME);

        try
        {
            File.WriteAllBytes(tempFilePath, audioByte);
            return await AudioEngine.AudioInstance.GetAudioClipFromLocalFile(tempFilePath);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Failed to create temporary cache file. Exception:\n" +
                             $"{e}");

            return (false, null, new byte[0]);
        }
        finally
        {
            if (File.Exists(tempFilePath)) // delete the cache after we're done
            {
                File.Delete(tempFilePath);
            }
        }
    }

    public static async Task<(bool, EditorChart, AudioClip)> ConvertFilesToEditorChart(string chartJson, byte[] audioBytes)
    {
        try
        {
            EditorChart editorChart = JsonConvert.DeserializeObject<EditorChart>(chartJson, GameManager.GameInstance.JsonSerializerSettings);

            (bool clipResult, AudioClip clip, byte[] _) = await GetAudioClipFromByteArray(audioBytes);

            if (!clipResult)
            {
                return (true, editorChart, null);
            }

            return (true, editorChart, clip);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Failed to convert files to editor chart. Exception: \n" +
                             $"{e}");
            return (false, null, null);
        }
    }

    public const string k_GameChartStorageFolderName = "Loaded_Charts";
    public static bool ImportEditorChartToGameStorage(string editorChartPath, out string internalChartPath)
    {
        if (!File.Exists(editorChartPath))
        {
            internalChartPath = "";
            return false;
        }

        if (Path.GetExtension(editorChartPath).TrimStart('.') != GameManager.k_FILEEXTENSION)
        {
            internalChartPath = "";
            return false;
        }

        string fileName = Path.GetFileNameWithoutExtension(editorChartPath);

        string gamePath = Path.Combine(Application.persistentDataPath, k_GameChartStorageFolderName, $"{fileName}.{GameManager.k_FILEEXTENSION}");

        int copyIndex = 0;
        while (File.Exists(gamePath))
        {
            copyIndex++;
            gamePath = Path.Combine(Application.persistentDataPath, k_GameChartStorageFolderName, $"{fileName}_{copyIndex}.{GameManager.k_FILEEXTENSION}");
        }

        // gamePath does not conflict anymore

        internalChartPath = gamePath;
        File.Copy(editorChartPath, gamePath);

        return true;
    }

    public static void ImportTutorialChartToGameStorage()
    {
        string streamingAssetPath = Path.Combine(Application.streamingAssetsPath, $"{GameManager.k_TUTORIALCHARTNAME}.{GameManager.k_FILEEXTENSION}");


        if (!File.Exists(streamingAssetPath))
        {
            return;
        }

        string fileName = Path.GetFileNameWithoutExtension(streamingAssetPath);
        string gameDirectory = Path.Combine(Application.persistentDataPath, k_GameChartStorageFolderName);

        if (!Directory.Exists(gameDirectory))
        {
            Directory.CreateDirectory(gameDirectory);
        }

        string gamePath = Path.Combine(gameDirectory, $"{fileName}.{GameManager.k_FILEEXTENSION}");

        if (File.Exists(gamePath)) // do not import again if we already imported the tutorial chart
        {
            return;
        }

        File.Copy(streamingAssetPath, gamePath);

        return;
    }

    public static void ReadEditorChartsInGameStorage(out string[] editorChartPaths)
    {
        string path = Path.Combine(Application.persistentDataPath, k_GameChartStorageFolderName);
        if (!Directory.Exists(path)) // create directory if it doesn't exist
        {
            Directory.CreateDirectory(path);
        }

        editorChartPaths = Directory.EnumerateFiles(path).Where(x => Path.GetExtension(x).TrimStart('.').ToLowerInvariant() == GameManager.k_FILEEXTENSION).OrderBy(x => x).ToArray(); // only get files with our extension and sort in ascending order
    }

    public static void GetMetadataOfEditorChartPath(string fullFilePath, out EditorChartMetadata metadata)
    {
        bool isValid = GameArchiveValidator.GetArchiveFileBytes(fullFilePath, out byte[] archiveBytes);

        if (!isValid)
        {
            metadata = null;
            return;
        }

        MemoryStream stream = new MemoryStream(archiveBytes);
        ZipArchive archive = new ZipArchive(stream, ZipArchiveMode.Read);

        ZipArchiveEntry metadataEntry = archive.GetEntry(GameManager.k_METADATAFILENAME);

        string metadataJson = "";
        if (metadataEntry == null)
        {
            metadata = null;

            archive.Dispose();
            stream.Close();
            return;
        }
        else
        {
            StreamReader metadataReader = new StreamReader(metadataEntry.Open());

            metadataJson = metadataReader.ReadToEnd();
            metadataReader.Close();
        }

        metadata = JsonConvert.DeserializeObject<EditorChartMetadata>(metadataJson, GameManager.GameInstance.JsonSerializerSettings);

        archive.Dispose();
        stream.Close();
    }

    public static void SaveGlobalSettingsToFile(GlobalSettings settings)
    {
        string path = Path.Combine(Application.persistentDataPath, GameManager.k_PLAYERSETTINGSFILENAME);

        string json = JsonConvert.SerializeObject(settings);

        File.WriteAllText(path, json);
    }

    public static bool LoadGlobalSettingsFromFile(out GlobalSettings settings)
    {
        string path = Path.Combine(Application.persistentDataPath, GameManager.k_PLAYERSETTINGSFILENAME);

        if (!File.Exists(path))
        {
            settings = GameManager.DefaultGlobalSettings;
            return false;
        }

        string json = File.ReadAllText(path);

        try
        {
            settings = JsonConvert.DeserializeObject<GlobalSettings>(json);

            return true;
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Failed to load player settings. Exception: \n" +
                             $"{e}");

            settings = GameManager.DefaultGlobalSettings;
            return false;
        }
    }

    public const string k_GAMEPLAYRECORDSDIRECTORY = "Play_Records";
    public static void SaveGameplayStatisticRecordToFile(GameplayStatisticRecord gameplay)
    {
        string directory = Path.Combine(Application.persistentDataPath, k_GAMEPLAYRECORDSDIRECTORY);

        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        string fileName = Path.Combine(directory, $"{gameplay.ChartMetadata.ChartMapper}-{gameplay.ChartMetadata.ChartName}-{gameplay.RecordTimestamp}.json"); // timestamp should ensure that no file collision, unless if someone wants to fuck around

        string jsonString = JsonConvert.SerializeObject(gameplay, GameManager.GameInstance.JsonSerializerSettings);

        File.WriteAllText(fileName, jsonString);
    }

    /// <summary>
    /// Loads all gameplay records into the game as a list.
    /// </summary>
    /// <param name="allRecords"></param>
    /// <returns></returns>
    public static void LoadAllGameplayStatisticRecordFile(out List<GameplayStatisticRecord> allRecords)
    {
        string directory = Path.Combine(Application.persistentDataPath, k_GAMEPLAYRECORDSDIRECTORY);

        GetAllGameplayStatisticRecordFilePaths(out string[] files);

        allRecords = new List<GameplayStatisticRecord>(files.Length);

        for (int i = 0; i < files.Length; i++)
        {
            try
            {
                LoadSpecificGameplayStatisticRecordFile(files[i], out GameplayStatisticRecord record);
                allRecords.Add(record);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Failed to load record at {files[i]}. Exception: \n" +
                                 $"{e.Message}");

            }
        }
    }

    public static void LoadSpecificGameplayStatisticRecordFile(string path, out GameplayStatisticRecord specificRecord)
    {
        try
        {
            string json = File.ReadAllText(path);

            specificRecord = JsonConvert.DeserializeObject<GameplayStatisticRecord>(json, GameManager.GameInstance.JsonSerializerSettings);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Failed to load record at {path}. Exception: \n" +
                             $"{e.Message}");
            specificRecord = new();
        }
    }

    public static void GetAllGameplayStatisticRecordFilePaths(out string[] paths)
    {
        string directory = Path.Combine(Application.persistentDataPath, k_GAMEPLAYRECORDSDIRECTORY);

        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        paths = Directory.EnumerateFiles(directory).Where(x => Path.GetExtension(x).TrimStart('.').ToLowerInvariant() == "json").ToArray(); // note we store our gameplay records as json
    }

    /// <summary>
    /// Creates a mapping f: Metadata -> set of records. This should be done at the beginning of the game load. <br></br>
    /// </summary>
    /// <param name="mapping"></param>
    public static void CreateMetadataToRecordsMapping(out Dictionary<EditorChartMetadata, List<GameplayStatisticRecord>> mapping)
    {
        mapping = new();
        LoadAllGameplayStatisticRecordFile(out List<GameplayStatisticRecord> records);

        for (int i = 0; i < records.Count; i++)
        {
            UpdateMetadataToRecordsMapping(records[i], mapping);
        }
    }

    /// <summary>
    /// Updates the mapping f: Metadata -> set of records while keeping the descending order for final scores.
    /// </summary>
    /// <param name="record"></param>
    /// <param name="mapping"></param>
    public static void UpdateMetadataToRecordsMapping(GameplayStatisticRecord record, Dictionary<EditorChartMetadata, List<GameplayStatisticRecord>> mapping)
    {
        EditorChartMetadata metadata = record.ChartMetadata;

        if (!mapping.TryGetValue(metadata, out List<GameplayStatisticRecord> recordsList))
        {
            recordsList = new List<GameplayStatisticRecord>() { record };
            mapping.Add(metadata, recordsList);
            return;
        }

        recordsList.Add(record);
        recordsList.Sort((x, y) => SortRecordsComparator(y, x));
    }

    /// <summary>
    /// A comparator to sort records by final score. The comparator assumes ascending order, swap the operands for descending order.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    private static int SortRecordsComparator(GameplayStatisticRecord x, GameplayStatisticRecord y)
    {
        if (x.FinalScore > y.FinalScore)
        {
            return 1;
        }
        else if (x.FinalScore < y.FinalScore)
        {
            return -1;
        }
        else return 0;
    }
}
