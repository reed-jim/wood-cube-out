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
    [SerializeField] private Button[] useBoosterButtons;

    [SerializeField] private BoosterType boosterType;

    [SerializeField] private IntVariable currentLevel;

    private int step;

    void Awake()
    {
        for (int i = 0; i < useBoosterButtons.Length; i++)
        {
            useBoosterButtons[i].onClick.AddListener(OnBoosterUsed);
        }
    }

    void Start()
    {
        StartCoroutine(RunTutorial());
    }

    private IEnumerator RunTutorial()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(1f / 30);

        int maxStep = 1;

        for (int i = 0; i < useBoosterButtons.Length; i++)
        {
            useBoosterButtons[i].gameObject.SetActive(false);
        }

        useBoosterButtons[(int)boosterType].gameObject.SetActive(true);

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

    private void OnLevelStarted()
    {
        if (currentLevel.Value == GameConstants.GetLevelToUnlockBooster(boosterType))
        {
            
        }
    }
}
