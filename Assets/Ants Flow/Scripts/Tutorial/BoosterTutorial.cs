using System;
using System.Collections;
using System.Collections.Generic;
using AntsFlow;
using Lean.Localization;
using NamPhuThuy.GamePlay;
using PrimeTween;
using Saferio.Util.SaferioTween;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static GameEnum;

public class BoosterTutorial : Singleton<BoosterTutorial>
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TMP_Text description;
    [SerializeField] private LeanLocalizedTextMeshProUGUI localizedDescription;
    [SerializeField] private SkeletonGraphic tutorialHand;
    [SerializeField] private SkeletonAnimation worldTutorialHand;
    [SerializeField] private Button[] useBoosterButtons;
    [SerializeField] private SkeletonGraphic[] boosterSkeletonGraphic;

    public int[] boosterTutorialLevels;

    [SerializeField] private List<BoosterType> allboosterTypes;

    [SerializeField] private IntVariable currentLevel;

    public static event Action<BoosterType> useTutorialBoosterEvent;

    private int step;
    private BoosterType boosterType;

    protected override void MoreActionOnAwake()
    {
        LevelLoader.startLevelEvent += OnLevelStarted;
        WashingMachineItem.selectWashingMachineEvent += OnAntBoxSelected;
        WaterFlowGameInput.useClearOneColorBoosterEvent += OnClearOneColorBoosterUsed;

        for (int i = 0; i < useBoosterButtons.Length; i++)
        {
            useBoosterButtons[i].onClick.AddListener(OnBoosterUsed);
        }
    }

    void OnDestroy()
    {
        LevelLoader.startLevelEvent -= OnLevelStarted;
        WashingMachineItem.selectWashingMachineEvent -= OnAntBoxSelected;
        WaterFlowGameInput.useClearOneColorBoosterEvent -= OnClearOneColorBoosterUsed;
    }

    private IEnumerator RunTutorial()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(1f / 30);

        int maxStep = 1;

        for (int i = 0; i < useBoosterButtons.Length; i++)
        {
            useBoosterButtons[i].gameObject.SetActive(false);
        }

        int boosterIndex = GetBoosterIndex(boosterType);

        useBoosterButtons[boosterIndex].gameObject.SetActive(true);
        boosterSkeletonGraphic[boosterIndex].color = Color.white;

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

        int boosterIndex = GetBoosterIndex(boosterType);

        useBoosterButtons[boosterIndex].gameObject.SetActive(true);
        boosterSkeletonGraphic[boosterIndex].color = Color.white;

        while (step < maxStep)
        {
            if (step == 1)
            {
                localizedDescription.TranslationName = "Now, you can select any ant box!";

                worldTutorialHand.transform.position = WashingMachineManager.Ins.washingMachineStartSlots[6].transform.position.AddZ(-5f);

                SaferioTween.ScaleUpWithPunch(worldTutorialHand.transform, 1.1f);
                SaferioTween.AlphaHide(canvasGroup);
                Tween.Custom(1, 0, duration: 0.3f, onValueChange: newVal =>
                {
                    tutorialHand.color = ColorUtil.WithAlpha(Color.white, newVal);
                    boosterSkeletonGraphic[boosterIndex].color = ColorUtil.WithAlpha(Color.white, newVal);
                });

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

        int boosterIndex = GetBoosterIndex(boosterType);

        useBoosterButtons[boosterIndex].gameObject.SetActive(true);
        boosterSkeletonGraphic[boosterIndex].color = Color.white;

        while (step < maxStep)
        {
            if (step == 1)
            {
                localizedDescription.TranslationName = "Select a color to clear all matching tiles.";

                worldTutorialHand.transform.position = AntsFlow.TileManager.Ins.GetTilePosition(AntsFlow.TileManager.Ins.tiles[0]).ChangeZ(-6.1f);

                SaferioTween.ScaleUpWithPunch(worldTutorialHand.transform, 1.1f);
                SaferioTween.AlphaHide(canvasGroup);
                Tween.Custom(1, 0, duration: 0.3f, onValueChange: newVal =>
                {
                    tutorialHand.color = ColorUtil.WithAlpha(Color.white, newVal);
                    boosterSkeletonGraphic[boosterIndex].color = ColorUtil.WithAlpha(Color.white, newVal);
                });

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

    public void EndTutorial()
    {
        int boosterIndex = GetBoosterIndex(boosterType);

        if (canvasGroup.gameObject.activeInHierarchy)
        {
            SaferioTween.AlphaHide(canvasGroup);
            Tween.Custom(1, 0, duration: 0.3f, onValueChange: newVal =>
            {
                tutorialHand.color = ColorUtil.WithAlpha(Color.white, newVal);
                boosterSkeletonGraphic[boosterIndex].color = ColorUtil.WithAlpha(Color.white, newVal);
            });
        }
    }

    private void OnBoosterUsed()
    {
        NextStep();

        useTutorialBoosterEvent?.Invoke(boosterType);
    }

    private void OnLevelStarted()
    {
        bool isTutorial = false;

        foreach (var boosterType in allboosterTypes)
        {
            int boosterIndex = GetBoosterIndex(boosterType);

            if (currentLevel.Value == boosterTutorialLevels[boosterIndex])
            {
                this.boosterType = boosterType;

                Transform boosterButton = useBoosterButtons[boosterIndex].transform;

                tutorialHand.transform.SetParent(boosterButton);
                tutorialHand.transform.localPosition = Vector3.zero.AddY(60);

                isTutorial = true;

                break;
            }
        }

        if (isTutorial)
        {
            if (DataUtility.Load($"is tutorial {boosterType} done", false))
            {
                isTutorial = false;
            }
            else
            {
                DataUtility.Save($"is tutorial {boosterType} done", true);
            }
        }

        if (!isTutorial)
        {
            EndTutorial();

            return;
        }

        canvasGroup.gameObject.SetActive(true);
        canvasGroup.alpha = 1;
        worldTutorialHand.gameObject.SetActive(false);

        tutorialHand.color = Color.white;

        localizedDescription.TranslationName = "Press the booster button!";

        step = 0;

        if (boosterType == BoosterType.BreakObject)
        {
            StartCoroutine(RunEnableAllAntBoxesBoosterTutorial());
        }
        else if (boosterType == BoosterType.ClearOneColor)
        {
            StartCoroutine(RunClearOneColorBoosterTutorial());
        }
        else
        {
            localizedDescription.TranslationName = "Use the booster to shuffle all ant boxes";

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
