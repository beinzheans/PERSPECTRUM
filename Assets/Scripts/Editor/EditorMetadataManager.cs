using System;
using TMPro;
using UnityEngine;

public class EditorMetadataManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField chartName;
    [SerializeField] private TMP_InputField chartMapper;
    [SerializeField] private TMP_InputField chartDifficulty;
    [SerializeField] private TMP_InputField songName;
    [SerializeField] private TMP_InputField songArtist;

    private void Start()
    {
        EditorManager.EditorInstance.OnRequestBaseChartMetadata += EditorInstance_OnRequestChartMetadata;
        EditorManager.EditorInstance.OnChartMetadataLoaded += EditorInstance_OnChartMetadataLoaded;
    }

    private const string k_NOCHARTNAMESTRING = "Unnamed Chart";
    private const string k_NOCHARTMAPPERSTRING = "Unknown Mapper(s)";
    private const int k_NOCHARTDIFFICULTYASSIGNEDINT = -1;
    private const string k_NOSONGNAMESTRING = "Unknown Song";
    private const string k_NOSONGARTISTSTRING = "Unknown Artist(s)";
    private void EditorInstance_OnChartMetadataLoaded(EditorChartMetadata obj)
    {
        if (obj == null)
        {
            chartName.text = k_NOCHARTNAMESTRING;
            chartMapper.text = k_NOCHARTMAPPERSTRING;
            chartDifficulty.text = k_NOCHARTDIFFICULTYASSIGNEDINT.ToString();
            songName.text = k_NOSONGNAMESTRING;
            songArtist.text = k_NOSONGARTISTSTRING;
            return;
        }

        chartName.text = obj.BaseMetadata.ChartName;
        chartMapper.text = obj.BaseMetadata.ChartMapper;
        chartDifficulty.text = obj.BaseMetadata.ChartDifficulty.ToString();
        songName.text = obj.BaseMetadata.SongName;
        songArtist.text = obj.BaseMetadata.SongArtist;
    }

    private BaseChartMetadata EditorInstance_OnRequestChartMetadata()
    {
        string c_name = string.IsNullOrWhiteSpace(chartName.text) ? k_NOCHARTNAMESTRING : chartName.text;
        string c_mapper = string.IsNullOrWhiteSpace(chartMapper.text) ? k_NOCHARTMAPPERSTRING : chartMapper.text;
        string s_name = string.IsNullOrWhiteSpace(songName.text) ? k_NOSONGNAMESTRING : songName.text;
        string s_artist = string.IsNullOrWhiteSpace(songArtist.text) ? k_NOSONGARTISTSTRING : songArtist.text;
        string GUID = Guid.NewGuid().ToString();

        BaseChartMetadata baseChartMetadata;

        bool difficultyParseResult = int.TryParse(chartDifficulty.text, out int c_difficulty);
        if (!difficultyParseResult || c_difficulty < 0)
        {
            baseChartMetadata = new BaseChartMetadata(c_name, c_mapper, s_name, s_artist, k_NOCHARTDIFFICULTYASSIGNEDINT, GameManager.GameInstance.CurrentVersion, GUID);
        }
        else
        {
            baseChartMetadata = new BaseChartMetadata(c_name, c_mapper, s_name, s_artist, c_difficulty, GameManager.GameInstance.CurrentVersion, GUID);
        }

        return baseChartMetadata;
    }
}
