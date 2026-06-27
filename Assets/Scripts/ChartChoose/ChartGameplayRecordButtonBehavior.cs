using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A class to define the behavior of the play record buttons.
/// </summary>
public class ChartGameplayRecordButtonBehavior : MonoBehaviour
{
    [SerializeField] private TMP_Text timestampText;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text rankText;

    [SerializeField] private Button watchReplayButton;
    public GameplayStatisticRecord AssociatedRecord { get; private set; }

    public void AssignStatisticRecordToBehavior(GameplayStatisticRecord record)
    {
        AssociatedRecord = record;

        timestampText.text = MathHelper.ConvertTimestampToHumanReadableTime(AssociatedRecord.RecordTimestamp);
        scoreText.text = ((int)math.round(record.FinalScore)).ToString();

        GameplayResultRank rank = MathHelper.ConvertOverallScoreToRank(record.FinalScore);
        rankText.text = MathHelper.ConvertRankToString(rank);

        watchReplayButton.onClick.AddListener(() => GameManager.GameInstance.RequestReplayChartEvent(ChartChooseManager.ChartChooseInstance.CurrentSelectedChartButton.AssociatedFullFilePath, AssociatedRecord));
    }

    public void OnDestroy()
    {
        watchReplayButton.onClick.RemoveAllListeners();
    }
}
