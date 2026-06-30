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

public class TutorialLevelOne : Singleton<TutorialLevelOne>
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
    
    private int gameplayCanvasOriginalSortingOrder;
    
    private ItemCategory theme1;
    private ItemCategory theme2;
    private Coroutine tutorialCoroutine;
    private Coroutine dragLoopCoroutine;

    public void StartTutorial()
    {
        step = 0;
        if (tutorialCoroutine != null)
        {
            StopCoroutine(tutorialCoroutine);
        }
        tutorialCoroutine = StartCoroutine(RunLevelOneTutorialCoroutine());
    }

    public void HideTutorial()
    {
        StopDragLoop();
        if (tutorialCoroutine != null)
        {
            StopCoroutine(tutorialCoroutine);
            tutorialCoroutine = null;
        }

        if (tutorialContainer != null) tutorialContainer.gameObject.SetActive(false);
        if (blackOverlay != null) blackOverlay.gameObject.SetActive(false);
        gameObject.SetActive(false);
    }

    private IEnumerator RunLevelOneTutorialCoroutine()
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
        BaseTile tile1 = TileManager.Ins.tiles.FirstOrDefault(t => !t.IsOnSlot && !t.IsOutside && !t.IsDone && t.ItemCategory == theme1);
        if (tile1 != null)
        {
            // Kích hoạt overlay
            upToAll.gameObject.SetActive(true);
            upToAll.color = new Color(upToAll.color.r, upToAll.color.g, upToAll.color.b, 0f);
            Tween.Alpha(upToAll, 0.7f, duration: 0.25f);

            showText("Match cards with the same theme!");

            // Highlight container và tile
            container1.SetHighlight(true);
            tile1.SetHighlight(true);

            // Đưa lên trên overlay
            Transform origContainerParent = container1.transform.parent;
            int origContainerSiblingIndex = container1.transform.GetSiblingIndex();
            container1.transform.SetParent(upToAll.transform, true);

            Transform origTileParent = tile1.transform.parent;
            int origTileSiblingIndex = tile1.transform.GetSiblingIndex();
            tile1.transform.SetParent(upToAll.transform, true);

            // Chạy bàn tay kéo lặp lại
            StartDragLoop(tile1.transform, container1.transform);

            bool step1Done = false;
            tile1.onInteract = () =>
            {
                // Khôi phục parent
                if (container1 != null && origContainerParent != null)
                {
                    container1.transform.SetParent(origContainerParent, true);
                    container1.transform.SetSiblingIndex(origContainerSiblingIndex);
                    container1.SetHighlight(false);
                }
                if (tile1 != null && origTileParent != null)
                {
                    tile1.transform.SetParent(origTileParent, true);
                    tile1.transform.SetSiblingIndex(origTileSiblingIndex);
                }

                // Ẩn overlay và bàn tay
                Tween.Alpha(upToAll, 0f, duration: 0.25f).OnComplete(() => upToAll.gameObject.SetActive(false));
                if (textImageTrans != null) textImageTrans.gameObject.SetActive(false);
                StopDragLoop();
                step1Done = true;
            };

            yield return new WaitUntil(() => step1Done);
        }

        // Đợi lá bài mới được chia từ bộ bài xong
        yield return StartCoroutine(WaitForCardsDealt());

        // --- BƯỚC 2 ---
        step = 2;
        BaseTile tile2 = TileManager.Ins.tiles.FirstOrDefault(t => !t.IsOnSlot && !t.IsOutside && !t.IsDone && t.ItemCategory == theme1);
        if (tile2 != null)
        {
            // Kích hoạt overlay
            upToAll.gameObject.SetActive(true);
            upToAll.color = new Color(upToAll.color.r, upToAll.color.g, upToAll.color.b, 0f);
            Tween.Alpha(upToAll, 0.7f, duration: 0.25f);

            showText("Match cards with the same theme!");

            // Highlight container và tile
            container1.SetHighlight(true);
            tile2.SetHighlight(true);

            // Đưa lên trên overlay
            Transform origContainerParent = container1.transform.parent;
            int origContainerSiblingIndex = container1.transform.GetSiblingIndex();
            container1.transform.SetParent(upToAll.transform, true);

            Transform origTileParent = tile2.transform.parent;
            int origTileSiblingIndex = tile2.transform.GetSiblingIndex();
            tile2.transform.SetParent(upToAll.transform, true);

            // Chạy bàn tay kéo lặp lại
            StartDragLoop(tile2.transform, container1.transform);

            bool step2Done = false;
            tile2.onInteract = () =>
            {
                // Khôi phục parent
                if (container1 != null && origContainerParent != null)
                {
                    container1.transform.SetParent(origContainerParent, true);
                    container1.transform.SetSiblingIndex(origContainerSiblingIndex);
                    container1.SetHighlight(false);
                }
                if (tile2 != null && origTileParent != null)
                {
                    tile2.transform.SetParent(origTileParent, true);
                    tile2.transform.SetSiblingIndex(origTileSiblingIndex);
                }

                // Ẩn overlay và bàn tay
                Tween.Alpha(upToAll, 0f, duration: 0.25f).OnComplete(() => upToAll.gameObject.SetActive(false));
                if (textImageTrans != null) textImageTrans.gameObject.SetActive(false);
                StopDragLoop();
                step2Done = true;
            };

            yield return new WaitUntil(() => step2Done);
        }

        // Đợi lá bài mới được chia từ bộ bài xong
        yield return StartCoroutine(WaitForCardsDealt());

        // --- BƯỚC 3 ---
        step = 3;
        BaseTile tile3 = TileManager.Ins.tiles.FirstOrDefault(t => !t.IsOnSlot && !t.IsOutside && !t.IsDone && t.ItemCategory == theme2);
        if (tile3 != null)
        {
            // Kích hoạt overlay
            upToAll.gameObject.SetActive(true);
            upToAll.color = new Color(upToAll.color.r, upToAll.color.g, upToAll.color.b, 0f);
            Tween.Alpha(upToAll, 0.7f, duration: 0.25f);

            showText("Match the card next!");

            // Highlight container và tile
            container2.SetHighlight(true);
            tile3.SetHighlight(true);

            // Đưa lên trên overlay
            Transform origContainerParent = container2.transform.parent;
            int origContainerSiblingIndex = container2.transform.GetSiblingIndex();
            container2.transform.SetParent(upToAll.transform, true);

            Transform origTileParent = tile3.transform.parent;
            int origTileSiblingIndex = tile3.transform.GetSiblingIndex();
            tile3.transform.SetParent(upToAll.transform, true);

            // Chạy bàn tay kéo lặp lại
            StartDragLoop(tile3.transform, container2.transform);

            bool step3Done = false;
            tile3.onInteract = () =>
            {
                // Khôi phục parent
                if (container2 != null && origContainerParent != null)
                {
                    container2.transform.SetParent(origContainerParent, true);
                    container2.transform.SetSiblingIndex(origContainerSiblingIndex);
                    container2.SetHighlight(false);
                }
                if (tile3 != null && origTileParent != null)
                {
                    tile3.transform.SetParent(origTileParent, true);
                    tile3.transform.SetSiblingIndex(origTileSiblingIndex);
                }

                // Ẩn overlay và bàn tay
                Tween.Alpha(upToAll, 0f, duration: 0.25f).OnComplete(() => upToAll.gameObject.SetActive(false));
                if (textImageTrans != null) textImageTrans.gameObject.SetActive(false);
                StopDragLoop();
                step3Done = true;
            };

            yield return new WaitUntil(() => step3Done);
        }

        // Đợi lá bài mới được chia từ bộ bài xong
        yield return StartCoroutine(WaitForCardsDealt());

        // --- BƯỚC 4 ---
        step = 4;
        BaseTile tile4 = TileManager.Ins.tiles.FirstOrDefault(t => !t.IsOnSlot && !t.IsOutside && !t.IsDone && t.ItemCategory == theme2);
        if (tile4 != null)
        {
            // Kích hoạt overlay
            upToAll.gameObject.SetActive(true);
            upToAll.color = new Color(upToAll.color.r, upToAll.color.g, upToAll.color.b, 0f);
            Tween.Alpha(upToAll, 0.7f, duration: 0.25f);

            showText("Keep Matching!");

            // Highlight container và tile
            container2.SetHighlight(true);
            tile4.SetHighlight(true);

            // Đưa lên trên overlay
            Transform origContainerParent = container2.transform.parent;
            int origContainerSiblingIndex = container2.transform.GetSiblingIndex();
            container2.transform.SetParent(upToAll.transform, true);

            Transform origTileParent = tile4.transform.parent;
            int origTileSiblingIndex = tile4.transform.GetSiblingIndex();
            tile4.transform.SetParent(upToAll.transform, true);

            // Chạy bàn tay kéo lặp lại
            StartDragLoop(tile4.transform, container2.transform);

            bool step4Done = false;
            tile4.onInteract = () =>
            {
                // Khôi phục parent
                if (container2 != null && origContainerParent != null)
                {
                    container2.transform.SetParent(origContainerParent, true);
                    container2.transform.SetSiblingIndex(origContainerSiblingIndex);
                    container2.SetHighlight(false);
                }
                if (tile4 != null && origTileParent != null)
                {
                    tile4.transform.SetParent(origTileParent, true);
                    tile4.transform.SetSiblingIndex(origTileSiblingIndex);
                }

                // Ẩn overlay và bàn tay
                Tween.Alpha(upToAll, 0f, duration: 0.25f).OnComplete(() => upToAll.gameObject.SetActive(false));
                if (textImageTrans != null) textImageTrans.gameObject.SetActive(false);
                StopDragLoop();
                step4Done = true;
            };

            yield return new WaitUntil(() => step4Done);
        }

        // Kết thúc tutorial hoàn toàn
        step = 5;
        if (TileManager.Ins.levelItem != null)
        {
            TileManager.Ins.levelItem.isTutorial = false;
        }
        HideTutorial();
    }

    private IEnumerator WaitForCardsDealt()
    {
        yield return new WaitForSeconds(0.2f);
        yield return new WaitUntil(() => !TileManager.Ins.IsDealingCards);
        yield return new WaitForSeconds(0.2f);
    }

    public ItemCategory GetTutorialForcedCategory(ItemCategory defaultCategory)
    {
        ItemCategory forcedCategory = defaultCategory;
        if (step == 1) // Vừa hoàn tất click bài bước 1, chuẩn bị chia bài cho bước 2
        {
            forcedCategory = theme1;
        }
        else if (step == 2) // Vừa hoàn tất click bài bước 2, chuẩn bị chia bài cho bước 3
        {
            forcedCategory = theme2;
        }
        else if (step == 3) // Vừa hoàn tất click bài bước 3, chuẩn bị chia bài cho bước 4
        {
            forcedCategory = theme2;
        }

        if (forcedCategory != defaultCategory)
        {
            if (TileManager.Ins.itemCategoryPool.Contains(forcedCategory))
            {
                TileManager.Ins.itemCategoryPool.Add(defaultCategory);
                TileManager.Ins.itemCategoryPool.Remove(forcedCategory);
            }
        }

        return forcedCategory;
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
        tutorialHand.transform.localScale = Vector3.one;

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
