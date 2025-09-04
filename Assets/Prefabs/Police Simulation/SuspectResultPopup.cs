using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SuspectResultPopup : BasePopup
{
    [SerializeField] private TMP_Text totalScore;
    [SerializeField] private TMP_Text status;
    [SerializeField] private Button finishButton;

    [SerializeField] private GameObject suspectResultItemPrefab;
    [SerializeField] private Transform suspectResultContainer;

    protected override void RegisterMoreEvent()
    {
        finishButton.onClick.AddListener(Finish);
    }

    void OnEnable()
    {
        ShowResult();
    }

    private void ShowResult()
    {
        int totalScoreValue = SuspectManager.Instance.GetTotalScore();
        int maxScoreValue = SuspectManager.Instance.GetMaxScore();

        GamePersistentVariable.judgmentScore += totalScoreValue / maxScoreValue / 2f;

        Debug.Log("SAFERIO " + GamePersistentVariable.judgmentScore + "/" + totalScoreValue + "/" + maxScoreValue);

        totalScore.text = $"{totalScoreValue}";

        if (totalScoreValue < 5)
        {
            status.text = $"Failed";
        }
        else if (totalScoreValue == 5)
        {
            status.text = $"OK";
        }
        else
        {
            status.text = $"Successful";
        }

        Vector2 itemSize = suspectResultItemPrefab.GetComponent<RectTransform>().sizeDelta;

        for (int i = 0; i < SuspectManager.Instance.suspects.Length; i++)
        {
            GameObject suspectResultItem = Instantiate(suspectResultItemPrefab, suspectResultContainer);

            suspectResultItem.transform.localPosition = new Vector3(0, -1.1f * itemSize.y * (i + 1), 0);

            suspectResultItem.GetComponent<SuspectResultItem>().ShowResult(SuspectManager.Instance.suspects[i]);
        }
    }

    private void Finish()
    {
        Hide(() =>
        {
            UnpauseDayTime();
        });
    }
}
