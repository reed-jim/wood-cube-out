using System.Collections;
using System.Collections.Generic;
using NamPhuThuy.GamePlay;
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

    [SerializeField] private List<BoosterType> allboosterTypes;

    [SerializeField] private IntVariable currentLevel;

    private int step;
    private BoosterType boosterType;

    void Awake()
    {
        LevelLoader.startLevelEvent += OnLevelStarted;

        for (int i = 0; i < useBoosterButtons.Length; i++)
        {
            useBoosterButtons[i].onClick.AddListener(OnBoosterUsed);
        }
    }

    void OnDestroy()
    {
        LevelLoader.startLevelEvent -= OnLevelStarted;
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
        foreach (var boosterType in allboosterTypes)
        {
            if (currentLevel.Value == GameConstants.GetLevelToUnlockBooster(boosterType))
            {
                this.boosterType = boosterType;

                Transform boosterButton = useBoosterButtons[(int)boosterType].transform;

                tutorialHand.transform.SetParent(boosterButton);
                tutorialHand.transform.localPosition = Vector3.zero;

                break;
            }
        }

        StartCoroutine(RunTutorial());
    }
}
