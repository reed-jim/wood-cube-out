using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NamPhuThuy.GamePlay;
using PrimeTween;
using Saferio.Util.SaferioTween;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialLevelOne : Singleton<TutorialLevelOne>
{
    public SkeletonGraphic tutorialHand;
    public CanvasGroup tutorialContainer;
    public TMP_Text description;
    public CanvasGroup continueButtonCanvasGroup;
    public Button continueButton;

    public Canvas gameplayCanvas;
    public Image blackOverlay;

    public List<BaseTile> tilesToSelect;

    public int step;
    private int gameplayCanvasOriginalSortingOrder;

    protected override void MoreActionOnAwake()
    {
        base.MoreActionOnAwake();

        BaseTile.selectTileEvent += OnTileSelected;

        continueButton.onClick.AddListener(Continue);

        continueButton.gameObject.SetActive(false);
    }

    void OnDestroy()
    {
        BaseTile.selectTileEvent -= OnTileSelected;
    }

    public void StartTutorial()
    {
        tilesToSelect = TileManager.Ins.tiles.Where(item => item.ItemCategory == ItemCategory.School).Take(4).ToList();

        gameplayCanvasOriginalSortingOrder = gameplayCanvas.sortingOrder;
        gameplayCanvas.sortingOrder = 98;

        SaferioTween.AlphaShow(blackOverlay, endAlpha: 230f / 255);

        StartCoroutine(ProgressingTutorial());
    }

    private IEnumerator ProgressingTutorial()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(0.2f);

        int maxStep = 3;

        description.text = $"Select the tiles in the <color=#FF5252>School</color> category.";

        Tween.Position(tutorialHand.transform, tilesToSelect[0].transform.position.AddZ(-1), duration: 0.3f);

        while (step < maxStep)
        {
            if (step == 0)
            {
                if (tilesToSelect.Count == 0)
                {
                    step++;
                }
            }
            else if (step == 1)
            {
                description.text = $"When all tiles of a box are filled, the box is completed";

                SaferioTween.AlphaShow(continueButtonCanvasGroup, onCompleteAction: () =>
                {
                    continueButton.interactable = true;
                });
                Tween.Alpha(tutorialHand, 0, duration: 0.3f);

                step++;
            }

            yield return waitForSeconds;
        }
    }

    private void Continue()
    {
        step++;

        if (step == 3)
        {
            EndTutorial();
        }

        // if (step == 1)
        // {
        //     description.text = $"Select the tiles in the <color=#FF5252>Food</color> category.";

        //     SaferioTween.AlphaShow(tutorialHand);

        //     Tween.Position(tutorialHand.transform, tilesToSelect[0].transform.position.AddZ(-1), duration: 0.3f);

        //     continueButton.interactable = false;
        //     Tween.Alpha(continueButtonCanvasGroup, 0, duration: 0.3f);
        // }
        // else if (step == 3)
        // {
        //     EndTutorial();
        // }
    }

    private void OnTileSelected(BaseTile tile)
    {
        tilesToSelect.Remove(tile);

        if (tilesToSelect.Count > 0)
        {
            Tween.Position(tutorialHand.transform, tilesToSelect[0].transform.position.AddZ(-1), duration: 0.3f);
        }
    }

    private void EndTutorial()
    {
        gameplayCanvas.sortingOrder = gameplayCanvasOriginalSortingOrder;

        Tween.Alpha(tutorialContainer, 0, duration: 0.3f)
        .OnComplete(() =>
        {
            tutorialContainer.gameObject.SetActive(false);
        });
        Tween.Alpha(tutorialHand, 0, duration: 0.3f);
        Tween.Alpha(blackOverlay, 0, duration: 0.3f)
        .OnComplete(() =>
        {
            blackOverlay.gameObject.SetActive(false);
        });
    }
}
