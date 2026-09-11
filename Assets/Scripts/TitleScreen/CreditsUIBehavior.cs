using System;
using System.Linq;
using TMPro;
using UnityEngine;

/// <summary>
/// A script to control the credits behavior. <br></br>
/// This is generated each time when the title screen scene is loaded. using the information defined by <see cref="CreditsInfo"/> to allow for flexible addition of new credit fields. <br></br>
/// </summary>
public class CreditsUIBehavior : MonoBehaviour
{
    [SerializeField] private CreditsInfo[] allCredits;

    [SerializeField] private TMP_Text textPrefab_Field;
    [SerializeField] private TMP_Text textPrefab_Names;
    [SerializeField] private RectTransform creditsRectTransform;

    private void Start()
    {
        InstantiateCreditText();
    }

    private void InstantiateCreditText()
    {
        for (int i = 0; i < allCredits.Length; i++)
        {
            GameObject.Instantiate(textPrefab_Field, creditsRectTransform, false).SetText(allCredits[i].CreditTitle);

            string nameText = string.Join(", ", allCredits[i].CreditNames);
            GameObject.Instantiate(textPrefab_Names, creditsRectTransform, false).SetText(nameText);
        }
    }
}

/// <summary>
/// A struct to define what a credit is. <br></br>
/// </summary>
/// 
[Serializable]
public struct CreditsInfo
{
    [SerializeField] private string creditTitle;
    [SerializeField] private string[] creditNames;
    public string CreditTitle { get => creditTitle; }
    public string[] CreditNames { get => creditNames; }
}
