using System.Collections;
using Saferio.Util.SaferioTween;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static GameEnum;

public class BoosterTutorial : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TMP_Text description;
    [SerializeField] private SkeletonGraphic tutorialHand;
    [SerializeField] private Button useBoosterButton;

    [SerializeField] private BoosterType boosterType;

    private int step;

    void Awake()
    {
        useBoosterButton.onClick.AddListener(OnBoosterUsed);
    }

    void Start()
    {
        StartCoroutine(RunTutorial());
    }

    private IEnumerator RunTutorial()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(1f / 30);

        int maxStep = 1;

        while (step < maxStep)
        {
            yield return waitForSeconds;
        }

        EndTutorial();
    }

    private void NextStep()
    {
        step++;
    }

    private void EndTutorial()
    {
        SaferioTween.AlphaHide(canvasGroup);
    }

    private void OnBoosterUsed()
    {
        NextStep();
    }
}
