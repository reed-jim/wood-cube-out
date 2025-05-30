using UnityEngine;
using UnityEngine.UI;

public class LevelChestProgressBar : MonoBehaviour
{
    [SerializeField] private Image fill;

    [SerializeField] private IntVariable currentLevel;

    void Awake()
    {
        SetProgress();
    }

    private void SetProgress()
    {
        fill.fillAmount = currentLevel.Value % 10 / 10f;
    }
}
