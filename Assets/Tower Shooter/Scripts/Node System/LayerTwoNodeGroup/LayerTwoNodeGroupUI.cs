using PrimeTween;
using Saferio.Util.SaferioTween;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LayerTwoNodeGroupUI : MonoBehaviour
{
    [SerializeField] private Image fill;
    [SerializeField] private TMP_Text globalMultiplierText;

    private Sequence punchTextSequence;

    public void SetProgress(float progress)
    {
        fill.fillAmount = progress;
    }

    public void SetGlobalMultiplierText(double multiplier)
    {
        globalMultiplierText.text = CommonUtil.FormatNumber(multiplier);

        if (!punchTextSequence.isAlive)
        {
            punchTextSequence = SaferioTween.Punch(globalMultiplierText.transform);
        }
    }
}
