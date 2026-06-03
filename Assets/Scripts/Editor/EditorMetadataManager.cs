using TMPro;
using UnityEngine;

public class EditorMetadataManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField chartName;
    [SerializeField] private TMP_InputField chartMapper;
    [SerializeField] private TMP_InputField songName;
    [SerializeField] private TMP_InputField songArtist;

    private void Start()
    {
        EditorManager.EditorInstance.OnRequestChartMetadata += EditorInstance_OnRequestChartMetadata;
        EditorManager.EditorInstance.OnChartMetadataLoaded += EditorInstance_OnChartMetadataLoaded;
    }

    private const string k_NOCHARTNAMESTRING = "Unnamed Chart";
    private const string k_NOCHARTMAPPERSTRING = "Unknown Mapper(s)";
    private const string k_NOSONGNAMESTRING = "Unknown Song";
    private const string k_NOSONGARTISTSTRING = "Unknown Artist(s)";
    private void EditorInstance_OnChartMetadataLoaded(EditorChartMetadata obj)
    {
        if (obj == null)
        {
            chartName.text = k_NOCHARTNAMESTRING;
            chartMapper.text = k_NOCHARTMAPPERSTRING;
            songName.text = k_NOSONGNAMESTRING;
            songArtist.text = k_NOSONGARTISTSTRING;
            return;
        }

        chartName.text = obj.ChartName;
        chartMapper.text = obj.ChartMapper;
        songName.text = obj.SongName;
        songArtist.text = obj.SongArtist;
    }

    private EditorChartMetadata EditorInstance_OnRequestChartMetadata()
    {
        string c_name = string.IsNullOrWhiteSpace(chartName.text) ? k_NOCHARTNAMESTRING : chartName.text;
        string c_mapper = string.IsNullOrWhiteSpace(chartMapper.text) ? k_NOCHARTMAPPERSTRING : chartMapper.text;
        string s_name = string.IsNullOrWhiteSpace(songName.text) ? k_NOSONGNAMESTRING : songName.text;
        string s_artist = string.IsNullOrWhiteSpace(songArtist.text) ? k_NOSONGARTISTSTRING : songArtist.text;

        return new EditorChartMetadata(c_name, c_mapper, s_name, s_artist, GameManager.GameInstance.CurrentVersion);
    }
}
