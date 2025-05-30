using UnityEngine;
using UnityEngine.UI;

public class StarChestProgressBar : MonoBehaviour
{
    [SerializeField] private Image fill;

    void Awake()
    {
        SetProgress();
    }

    private void SetProgress()
    {
        StarLevelData data = DataUtility.Load(GameConstants.STAR_LEVEL_DATA, new StarLevelData());

        fill.fillAmount = data.starCollected % 10 / 10f;
    }
}
