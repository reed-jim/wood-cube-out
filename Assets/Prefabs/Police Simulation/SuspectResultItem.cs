using TMPro;
using UnityEngine;

public class SuspectResultItem : MonoBehaviour
{
    [SerializeField] private TMP_Text suspectName;
    [SerializeField] private TMP_Text verdict;
    [SerializeField] private TMP_Text score;

    public void ShowResult(Suspect suspect)
    {
        suspectName.text = $"{suspect.suspectName}";
        verdict.text = $"{suspect.decision}";
        score.text = $"{SuspectManager.Instance.GetScore(suspect)}";
    }
}
