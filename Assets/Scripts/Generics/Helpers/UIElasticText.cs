using TMPro;
using UnityEngine;

/// <summary>
/// A class that handles elastic text by inheriting <see cref="UIElastic"/>.
/// </summary>
[RequireComponent(typeof(TMP_Text))]
public class UIElasticText : UIElastic
{
    public TMP_Text UIText { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        UIText = GetComponent<TMP_Text>();
    }

    public void SetText(string text, Vector2 elasticSize, double elasticTime)
    {
        base.SetElasticTimer(elasticSize, elasticTime);
        UIText.SetText(text);
    }

    public void SetTextWithoutElastic(string text)
    {
        UIText.SetText(text);
    }
}
