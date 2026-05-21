using System.Collections;
using Saferio.Util.SaferioTween;
using Spine.Unity;
using TMPro;
using UnityEngine;
using static GameEnum;

public class BoosterTutorial : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TMP_Text description;
    [SerializeField] private SkeletonGraphic tutorialHand;

    [SerializeField] private BoosterType boosterType;

    private IEnumerator RunTutorialBoosterAddSlot()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(1);

        yield return waitForSeconds;
    }

    private void EndTutorial()
    {
        SaferioTween.AlphaHide(canvasGroup);
    }

    private void OnBoosterUsed(BoosterType boosterType)
    {
        if (boosterType == this.boosterType)
        {

        }
    }
}
