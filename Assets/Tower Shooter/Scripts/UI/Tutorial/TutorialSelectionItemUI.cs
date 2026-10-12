using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialSelectionItemUI : PageViewItem
{
    [SerializeField] private TMP_Text tutorialNameText;
    [SerializeField] private Button selectButton;

    private TutorialItemData tutorialItemData;

    protected override void MoreActionOnAwake()
    {
        base.MoreActionOnAwake();

        selectButton.onClick.AddListener(Select);
    }

    public override void Setup(int index)
    {
        base.Setup(index);

        tutorialItemData = (TutorialSelectionScreen.Ins as TutorialSelectionScreen).GetTutorialItemData(index);

        if (tutorialItemData == null)
        {
            gameObject.SetActive(false);
        }
        else
        {
            tutorialNameText.text = tutorialItemData.tutorialName;
        }
    }

    protected override void Select()
    {
        base.Select();

        tutorialItemData.tutorialComponent.StartTutorial();
    }
}
