using TMPro;
using UnityEngine;

public class SuspectResultPopup : BasePopup
{
    [SerializeField] private TMP_Text totalScore;
    [SerializeField] private TMP_Text status;

    [SerializeField] private GameObject suspectResultItemPrefab;
    [SerializeField] private Transform suspectResultContainer;

    void OnEnable()
    {
        ShowResult();
    }

    private void ShowResult()
    {
        int totalScoreValue = SuspectManager.Instance.GetTotalScore();

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
}
