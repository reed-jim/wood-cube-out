using Saferio.Util.SaferioTween;
using TMPro;
using UnityEngine;

public class SpawningTileBox : MonoBehaviour
{
    [SerializeField] private TMP_Text numberText;

    public Color faction;
    public int totalNumber;
    public int numberLeft;

    private bool isAnimating;

    public void Setup(Color faction, int totalNumber)
    {
        this.faction = faction;
        this.totalNumber = totalNumber;

        numberLeft = totalNumber;
    }

    public void UpdateNumberText()
    {
        numberText.text = $"{numberLeft}";

        if (!isAnimating)
        {
            SaferioTween.Punch(numberText.transform, 1.1f, onCompleteAction: () =>
            {
                isAnimating = false;
            });

            isAnimating = true;
        }
    }
}
