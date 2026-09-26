using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UICollapsablePanel : BaseUIPopupContent
{
    [SerializeField] private string PanelName;

    [SerializeField] private Button collapseButton;
    [SerializeField] private TMP_Text panelNameDisplayText;
    [SerializeField] private RectTransform collapsablePanelContent;
    private bool activeState;
    protected override void Start()
    {
        base.Start();

        activeState = false;
        collapsablePanelContent.gameObject.SetActive(false);
        collapseButton.onClick.AddListener(ToggleActiveState);
        panelNameDisplayText.SetText(PanelName);
    }

    private void ToggleActiveState()
    {
        activeState = !activeState;

        if (activeState)
        {
            collapsablePanelContent.gameObject.SetActive(true);
        }
        else
        {
            collapsablePanelContent.gameObject.SetActive(false);
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        collapseButton.onClick.RemoveAllListeners();
    }
}
