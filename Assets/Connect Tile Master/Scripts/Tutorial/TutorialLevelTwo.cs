using System;
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

public class TutorialLevelTwo : Singleton<TutorialLevelTwo>
{
    [Header("Unity Prefab References")]
    public SkeletonGraphic tutorialHand;
    public CanvasGroup tutorialContainer;
    public TMP_Text description;
    public CanvasGroup continueButtonCanvasGroup;
    public Button continueButton;

    public Canvas gameplayCanvas;
    public Image blackOverlay;

    public List<BaseTile> tilesToSelect;
    public int step;

    private ItemCategory theme1;
    private ItemCategory theme2;
    private Coroutine tutorialCoroutine;
    private Coroutine dragLoopCoroutine;
    private int lastForcedStep = 0;

    public void StartTutorial()
    {
        step = 0;
        lastForcedStep = 0;
        if (tutorialCoroutine != null)
        {
            StopCoroutine(tutorialCoroutine);
        }
        tutorialCoroutine = StartCoroutine(RunLevelTwoTutorialCoroutine());
    }

    public void HideTutorial()
    {
        StopDragLoop();
        if (tutorialCoroutine != null)
        {
            StopCoroutine(tutorialCoroutine);
            tutorialCoroutine = null;
        }

        if (TileManager.Ins != null)
        {
            TileManager.Ins.isCurrentLevelTutorialActive = false;
        }

        if (tutorialContainer != null) tutorialContainer.gameObject.SetActive(false);
        if (blackOverlay != null) blackOverlay.gameObject.SetActive(false);
        gameObject.SetActive(false);
    }

    private IEnumerator RunLevelTwoTutorialCoroutine()
    {
        // Step 0: Đợi chia bài ban đầu hoàn thành
        yield return new WaitUntil(() => TileManager.Ins.isLevelActive && !TileManager.Ins.IsDealingCards);
        yield return new WaitForSeconds(0.2f);

        if (TileManager.Ins.TileSlotContainers.Count < 2)
        {
            Debug.LogError("[Tutorial] TileSlotContainers count is less than 2!");
            HideTutorial();
            yield break;
        }

        theme1 = TileManager.Ins.TileSlotContainers[0].ItemCategory;
        theme2 = TileManager.Ins.TileSlotContainers[1].ItemCategory;

        TileSlotContainer container1 = TileManager.Ins.TileSlotContainers[0];
        TileSlotContainer container2 = TileManager.Ins.TileSlotContainers[1];

        Image upToAll = TileManager.Ins.UpToAll;
        if (upToAll == null)
        {
            Debug.LogError("[Tutorial] upToAll is null!");
            HideTutorial();
            yield break;
        }

        Transform textImageTrans = upToAll.transform.Find("Image");
        TMP_Text tutText = null;
        Text legacyText = null;
        if (textImageTrans != null)
        {
            tutText = textImageTrans.GetComponentInChildren<TMP_Text>(true);
            legacyText = textImageTrans.GetComponentInChildren<Text>(true);
        }

        Action<string> showText = (textStr) =>
        {
            if (textImageTrans != null)
            {
                textImageTrans.gameObject.SetActive(true);
            }
            if (tutText != null)
            {
                tutText.text = textStr;
            }
            else if (legacyText != null)
            {
                legacyText.text = textStr;
            }
        };

        // --- BƯỚC 1 ---
        step = 1;
        BaseTile tile1 = TileManager.Ins.tiles.FirstOrDefault(t => !t.IsOnSlot && !t.IsOutside && !t.IsDone && t.ItemCategory == theme1 && t.IsText);

        if (tile1 != null)
        {
            // Kích hoạt overlay
            upToAll.gameObject.SetActive(true);
            upToAll.color = new Color(upToAll.color.r, upToAll.color.g, upToAll.color.b, 0f);
            Tween.Alpha(upToAll, 0.87f, duration: 0.25f);

            showText("Match text cards too!");

            // Highlight container và tile
            container1.SetHighlight(true);
            tile1.SetHighlight(true);

            // Đưa lên trên overlay
            Transform origContainerParent = container1.transform.parent;
            int origContainerSiblingIndex = container1.transform.GetSiblingIndex();
            container1.transform.SetParent(upToAll.transform, true);
            container1.transform.SetAsLastSibling();

            Transform origTileParent = tile1.transform.parent;
            int origTileSiblingIndex = tile1.transform.GetSiblingIndex();
            tile1.transform.SetParent(upToAll.transform, true);
            tile1.transform.SetAsLastSibling();

            // Chạy bàn tay kéo lặp lại
            StartDragLoop(tile1.transform, container1.transform);

            bool clicked = false;
            tile1.onInteract = () =>
            {
                if (clicked) return;
                if (textImageTrans != null) textImageTrans.gameObject.SetActive(false);
                StopDragLoop();
                clicked = true;
            };

            yield return new WaitUntil(() => tile1 == null || tile1.IsDone);

            // Khôi phục parent của container sau khi tile đã vào slot
            if (container1 != null && origContainerParent != null)
            {
                container1.transform.SetParent(origContainerParent, true);
                container1.transform.SetSiblingIndex(origContainerSiblingIndex);
                container1.SetHighlight(false);
            }
        }

        // Đợi lá bài mới được chia từ bộ bài xong
        yield return StartCoroutine(WaitForCardsDealt());

        // --- BƯỚC 2 ---
        // Highlight: Theme: Gaming. Hand kéo. Text: Images and words can both match!
        step = 2;
        BaseTile tile2 = TileManager.Ins.tiles.FirstOrDefault(t => !t.IsOnSlot && !t.IsOutside && !t.IsDone && t.ItemCategory == theme2 && t.IsText);
        if (tile2 != null)
        {
            // Kích hoạt overlay
            upToAll.gameObject.SetActive(true);
            upToAll.color = new Color(upToAll.color.r, upToAll.color.g, upToAll.color.b, 0f);
            Tween.Alpha(upToAll, 0.87f, duration: 0.25f);

            showText("Images and words can both match!");

            // Highlight container và tile
            container2.SetHighlight(true);
            tile2.SetHighlight(true);

            // Đưa lên trên overlay
            Transform origContainerParent = container2.transform.parent;
            int origContainerSiblingIndex = container2.transform.GetSiblingIndex();
            container2.transform.SetParent(upToAll.transform, true);
            container2.transform.SetAsLastSibling();

            Transform origTileParent = tile2.transform.parent;
            int origTileSiblingIndex = tile2.transform.GetSiblingIndex();

            tile2.transform.SetParent(upToAll.transform, true);
            tile2.transform.SetAsLastSibling();

            // Chạy bàn tay kéo lặp lại
            StartDragLoop(tile2.transform, container2.transform);

            bool clicked2 = false;
            tile2.onInteract = () =>
            {
                if (clicked2) return;

                // Ẩn text và bàn tay
                if (textImageTrans != null) textImageTrans.gameObject.SetActive(false);
                StopDragLoop();
                clicked2 = true;
            };

            yield return new WaitUntil(() => clicked2);
            yield return new WaitUntil(() => tile2 == null || tile2.IsDone);

            if (container2 != null && origContainerParent != null)
            {
                container2.transform.SetParent(origContainerParent, true);
                container2.transform.SetSiblingIndex(origContainerSiblingIndex);
                container2.SetHighlight(false);
            }
        }

        // --- BƯỚC 3 ---
        step = 3;

        upToAll.gameObject.SetActive(true);
        upToAll.color = new Color(upToAll.color.r, upToAll.color.g, upToAll.color.b, 0f);
        Tween.Alpha(upToAll, 0.87f, duration: 0.25f);

        showText("Keep matching!");

        // Wait 2 seconds then hide text
        yield return new WaitForSeconds(2f);
        if (textImageTrans != null) textImageTrans.gameObject.SetActive(false);
        Tween.Alpha(upToAll, 0f, duration: 0.25f).OnComplete(() => upToAll.gameObject.SetActive(false));

        // Kết thúc tutorial hoàn toàn
        step = 4;
        if (TileManager.Ins.levelItem != null)
        {
            TileManager.Ins.levelItem.isTutorial = false;
        }
        HideTutorial();
    }

    private IEnumerator WaitForCardsDealt()
    {
        // Chờ tối đa 1.5 giây để bài bắt đầu chia (IsDealingCards = true)
        float timeout = 1.5f;
        while (!TileManager.Ins.IsDealingCards && timeout > 0)
        {
            timeout -= Time.deltaTime;
            yield return null;
        }

        // Đợi chia bài xong (IsDealingCards = false)
        yield return new WaitUntil(() => !TileManager.Ins.IsDealingCards);
        yield return new WaitForSeconds(0.2f);
    }

    public ItemCategory GetTutorialForcedCategory(ItemCategory defaultCategory)
    {
        return defaultCategory;
    }

    private void StartDragLoop(Transform from, Transform to)
    {
        StopDragLoop();
        dragLoopCoroutine = StartCoroutine(DragLoop(from, to));
    }

    private void StopDragLoop()
    {
        if (dragLoopCoroutine != null)
        {
            StopCoroutine(dragLoopCoroutine);
            dragLoopCoroutine = null;
        }
        if (tutorialHand != null)
        {
            tutorialHand.gameObject.SetActive(false);
        }
    }

    private IEnumerator DragLoop(Transform from, Transform to)
    {
        if (tutorialHand == null) yield break;

        tutorialHand.gameObject.SetActive(true);
        tutorialHand.transform.SetParent(TileManager.Ins.UpToAll.transform, true);
        tutorialHand.transform.SetAsLastSibling();
        tutorialHand.transform.localScale = Vector3.one;
        tutorialHand.AnimationState.SetAnimation(0, "begin", true);

        while (true)
        {
            if (from == null || to == null) yield break;

            tutorialHand.transform.position = from.position;

            Tween.Alpha(tutorialHand, 1f, duration: 0.25f);
            yield return new WaitForSeconds(0.25f);

            float dragDuration = 1.0f;
            Tween.Position(tutorialHand.transform, to.position, duration: dragDuration, ease: Ease.InOutQuad);
            yield return new WaitForSeconds(dragDuration);

            Tween.Alpha(tutorialHand, 0f, duration: 0.25f);
            yield return new WaitForSeconds(0.4f);
        }
    }
}
