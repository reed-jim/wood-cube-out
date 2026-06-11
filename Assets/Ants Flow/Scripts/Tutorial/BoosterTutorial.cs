using System;
using System.Collections;
using System.Collections.Generic;
using AntsFlow;
using NamPhuThuy.GamePlay;
using PrimeTween;
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
    [SerializeField] private SkeletonAnimation worldTutorialHand;
    [SerializeField] private Button[] useBoosterButtons;

    public int[] boosterTutorialLevels;

    [SerializeField] private List<BoosterType> allboosterTypes;

    [SerializeField] private IntVariable currentLevel;

    public static event Action<BoosterType> useTutorialBoosterEvent;

    private int step;
    private BoosterType boosterType;

    void Awake()
    {
        LevelLoader.startLevelEvent += OnLevelStarted;
        WashingMachineItem.selectWashingMachineEvent += OnAntBoxSelected;
        AntsFlow.BaseTile.useClearOneColorBoosterEvent += OnClearOneColorBoosterUsed;

        for (int i = 0; i < useBoosterButtons.Length; i++)
        {
            useBoosterButtons[i].onClick.AddListener(OnBoosterUsed);
        }
    }

    void OnDestroy()
    {
        LevelLoader.startLevelEvent -= OnLevelStarted;
        WashingMachineItem.selectWashingMachineEvent -= OnAntBoxSelected;
        AntsFlow.BaseTile.useClearOneColorBoosterEvent -= OnClearOneColorBoosterUsed;
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

    #region  EnableAllAntBoxesBooster
    private IEnumerator RunEnableAllAntBoxesBoosterTutorial()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(1f / 30);

        int maxStep = 3;

        for (int i = 0; i < useBoosterButtons.Length; i++)
        {
            useBoosterButtons[i].gameObject.SetActive(false);
        }

        useBoosterButtons[(int)boosterType].gameObject.SetActive(true);

        while (step < maxStep)
        {
            if (step == 1)
            {
                description.text = "Now, you can select any ant box!";

                SaferioTween.ScaleUpWithPunch(worldTutorialHand.transform, 1.1f);
                Tween.Position(worldTutorialHand.transform,
                    WashingMachineManager.Ins.washingMachineStartSlots[6].transform.position.AddZ(-5f), duration: 0.3f);
                SaferioTween.AlphaHide(canvasGroup);

                step++;
            }
            yield return waitForSeconds;
        }

        SaferioTween.ScaleDownWithPunch(worldTutorialHand.transform);

        EndTutorial();
    }

    private void OnAntBoxSelected(WashingMachineItem antBox)
    {
        NextStep();
    }
    #endregion

    #region  EnableClearOneColorBooster
    private IEnumerator RunClearOneColorBoosterTutorial()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(1f / 30);

        int maxStep = 3;

        for (int i = 0; i < useBoosterButtons.Length; i++)
        {
            useBoosterButtons[i].gameObject.SetActive(false);
        }

        useBoosterButtons[(int)boosterType].gameObject.SetActive(true);

        while (step < maxStep)
        {
            if (step == 1)
            {
                description.text = "Now, you can select any ant box!";

                SaferioTween.ScaleUpWithPunch(worldTutorialHand.transform, 1.1f);
                Tween.Position(worldTutorialHand.transform,
                    TileManager.Ins.tiles[0].transform.position.AddZ(-5f), duration: 0.3f);

                step++;
            }
            yield return waitForSeconds;
        }

        SaferioTween.ScaleDownWithPunch(worldTutorialHand.transform);

        EndTutorial();
    }

    private void OnClearOneColorBoosterUsed(Color faction)
    {
        NextStep();
    }
    #endregion

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

        useTutorialBoosterEvent?.Invoke(boosterType);
    }

    private void OnLevelStarted()
    {
        foreach (var boosterType in allboosterTypes)
        {
            if (currentLevel.Value == boosterTutorialLevels[GetBoosterIndex(boosterType)])
            {
                this.boosterType = boosterType;

                Transform boosterButton = useBoosterButtons[(int)boosterType].transform;

                tutorialHand.transform.SetParent(boosterButton);
                tutorialHand.transform.localPosition = Vector3.zero;

                break;
            }
        }

        worldTutorialHand.gameObject.SetActive(false);

        description.text = "Press the booster button!";

        if (boosterType == BoosterType.BreakObject)
        {
            StartCoroutine(RunEnableAllAntBoxesBoosterTutorial());
        }
        else
        {
            StartCoroutine(RunTutorial());
        }
    }

    private int GetBoosterIndex(BoosterType boosterType)
    {
        int boosterIndex = 0;

        if (boosterType == BoosterType.BreakObject)
        {
            boosterIndex = 1;
        }
        else if (boosterType == BoosterType.ClearScrewPorts)
        {
            boosterIndex = 2;
        }
        else
        {
            boosterIndex = 3;
        }

        return boosterIndex;
    }
}
