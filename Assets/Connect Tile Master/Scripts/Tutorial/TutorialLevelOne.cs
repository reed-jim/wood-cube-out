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

    public List<BaseTile> tilesToSelect;

    public int step;

    protected override void MoreActionOnAwake()
    {
        base.MoreActionOnAwake();

        BaseTile.selectTileEvent += OnTileSelected;
        LevelLoader.startLevelEvent += OnLevelStarted;

        continueButton.onClick.AddListener(Continue);
    }

    void OnDestroy()
    {
        BaseTile.selectTileEvent -= OnTileSelected;
        LevelLoader.startLevelEvent -= OnLevelStarted;
    }

    private void StartTutorial()
    {
        StartCoroutine(ProgressingTutorial());
    }

    private IEnumerator ProgressingTutorial()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(1);

        int maxStep = 3;

        while (step < maxStep)
        {
            if (step == 1)
            {
                if (tilesToSelect.Count == 0)
                {
                    step++;
                }
            }
            else if (step == 2)
            {
                description.text = $"When all tiles of a box are filled, the box is completed";

                Tween.Alpha(continueButtonCanvasGroup, 1, duration: 0.3f)
                .OnComplete(() =>
                {
                    continueButton.interactable = true;
                });
            }

            yield return waitForSeconds;
        }
    }

    private void Continue()
    {
        step++;

        if (step == 1)
        {
            description.text = $"Select the tiles in the <color=#FFB84D><b>Food</b></color> category.";

            Tween.Position(tutorialHand.transform, tilesToSelect[0].transform.position.AddZ(-1), duration: 0.3f);

            continueButton.interactable = false;
            Tween.Alpha(continueButtonCanvasGroup, 0, duration: 0.3f);
        }
        else if (step == 3)
        {
            EndTutorial();
        }
    }

    private void OnLevelStarted()
    {
        tilesToSelect = TileManager.Ins.tiles.Where(item => item.ItemCategory == ItemCategory.Food).ToList();

        StartTutorial();
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
        Tween.Alpha(tutorialContainer, 0, duration: 0.3f)
        .OnComplete(() =>
        {
            tutorialContainer.gameObject.SetActive(false);
        });
    }
}
