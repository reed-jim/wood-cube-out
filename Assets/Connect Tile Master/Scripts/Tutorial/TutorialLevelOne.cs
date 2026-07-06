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
    private int lastForcedStep = 0;

    public void StartTutorial()
    {
        step = 0;
        lastForcedStep = 0;
        if (tutorialCoroutine != null)
        {
            StopCoroutine(tutorialCoroutine);
        }

        IntVariable levelVar = Resources.FindObjectsOfTypeAll<IntVariable>().FirstOrDefault(v => v.name.Equals("currentLevel", StringComparison.OrdinalIgnoreCase) || v.name.Contains("Level"));
        int levelNum = 1;
        if (levelVar != null)
        {
            levelVar.Load();
            levelNum = levelVar.Value;
        }

        if (levelNum == 1)
        {
            tutorialCoroutine = StartCoroutine(RunLevelOneTutorialCoroutine());
        }
        else if (levelNum == 13)
        {
            tutorialCoroutine = StartCoroutine(RunLevelThirteenTutorialCoroutine());
        }
        else
        {
            HideTutorial();
        }
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

    private IEnumerator RunLevelOneTutorialCoroutine()
    {
        // Step 0: Đợi chia bài ban đầu hoàn thành
        yield return new WaitUntil(() => TileManager.Ins.isLevelActive && !TileManager.Ins.IsDealingCards);

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
            Tween.Alpha(upToAll, 0.87f, duration: 0.25f);

            showText("Match cards with the <color=#FBC201>same theme!</color>");

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

                // Ẩn text và bàn tay
                if (textImageTrans != null) textImageTrans.gameObject.SetActive(false);
                StopDragLoop();
                clicked = true;
            };

            yield return new WaitUntil(() => clicked);
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
        step = 2;
        BaseTile tile2 = TileManager.Ins.tiles.FirstOrDefault(t => !t.IsOnSlot && !t.IsOutside && !t.IsDone && t.ItemCategory == theme1);
        if (tile2 != null)
        {
            // Kích hoạt overlay
            upToAll.gameObject.SetActive(true);
            upToAll.color = new Color(upToAll.color.r, upToAll.color.g, upToAll.color.b, 0f);
            Tween.Alpha(upToAll, 0.87f, duration: 0.25f);

            showText("Match cards with the <color=#FBC201>same theme!</color>");

            // Highlight container và tile
            container1.SetHighlight(true);
            tile2.SetHighlight(true);

            // Đưa lên trên overlay
            Transform origContainerParent = container1.transform.parent;
            int origContainerSiblingIndex = container1.transform.GetSiblingIndex();
            container1.transform.SetParent(upToAll.transform, true);
            container1.transform.SetAsLastSibling();

            Transform origTileParent = tile2.transform.parent;
            int origTileSiblingIndex = tile2.transform.GetSiblingIndex();
            tile2.transform.SetParent(upToAll.transform, true);
            tile2.transform.SetAsLastSibling();

            // Chạy bàn tay kéo lặp lại
            StartDragLoop(tile2.transform, container1.transform);

            bool clicked = false;
            tile2.onInteract = () =>
            {
                if (clicked) return;

                // Ẩn text và bàn tay
                if (textImageTrans != null) textImageTrans.gameObject.SetActive(false);
                StopDragLoop();
                clicked = true;
            };

            yield return new WaitUntil(() => clicked);
            yield return new WaitUntil(() => tile2 == null || tile2.IsDone);

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

        // --- BƯỚC 3 ---
        step = 3;
        BaseTile tile3 = TileManager.Ins.tiles.FirstOrDefault(t => !t.IsOnSlot && !t.IsOutside && !t.IsDone && t.ItemCategory == theme2);
        if (tile3 != null)
        {
            // Kích hoạt overlay
            upToAll.gameObject.SetActive(true);
            upToAll.color = new Color(upToAll.color.r, upToAll.color.g, upToAll.color.b, 0f);
            Tween.Alpha(upToAll, 0.87f, duration: 0.25f);

            showText("Match the <color=#FBC201>card next!</color>");

            // Highlight container và tile
            container2.SetHighlight(true);
            tile3.SetHighlight(true);

            // Đưa lên trên overlay
            Transform origContainerParent = container2.transform.parent;
            int origContainerSiblingIndex = container2.transform.GetSiblingIndex();
            container2.transform.SetParent(upToAll.transform, true);
            container2.transform.SetAsLastSibling();

            Transform origTileParent = tile3.transform.parent;
            int origTileSiblingIndex = tile3.transform.GetSiblingIndex();
            tile3.transform.SetParent(upToAll.transform, true);
            tile3.transform.SetAsLastSibling();

            // Chạy bàn tay kéo lặp lại
            StartDragLoop(tile3.transform, container2.transform);

            bool clicked = false;
            tile3.onInteract = () =>
            {
                if (clicked) return;

                // Ẩn text và bàn tay
                if (textImageTrans != null) textImageTrans.gameObject.SetActive(false);
                StopDragLoop();
                clicked = true;
            };

            yield return new WaitUntil(() => clicked);
            yield return new WaitUntil(() => tile3 == null || tile3.IsDone);

            // Khôi phục parent của container sau khi tile đã vào slot
            if (container2 != null && origContainerParent != null)
            {
                container2.transform.SetParent(origContainerParent, true);
                container2.transform.SetSiblingIndex(origContainerSiblingIndex);
                container2.SetHighlight(false);
            }
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
            Tween.Alpha(upToAll, 0.87f, duration: 0.25f);

            showText("Keep Matching!");

            // Highlight container và tile
            container2.SetHighlight(true);
            tile4.SetHighlight(true);

            // Đưa lên trên overlay
            Transform origContainerParent = container2.transform.parent;
            int origContainerSiblingIndex = container2.transform.GetSiblingIndex();
            container2.transform.SetParent(upToAll.transform, true);
            container2.transform.SetAsLastSibling();

            Transform origTileParent = tile4.transform.parent;
            int origTileSiblingIndex = tile4.transform.GetSiblingIndex();
            tile4.transform.SetParent(upToAll.transform, true);
            tile4.transform.SetAsLastSibling();

            // Chạy bàn tay kéo lặp lại
            StartDragLoop(tile4.transform, container2.transform);

            bool clicked = false;
            tile4.onInteract = () =>
            {
                if (clicked) return;

                // Ẩn text và bàn tay
                if (textImageTrans != null) textImageTrans.gameObject.SetActive(false);
                StopDragLoop();
                clicked = true;
            };

            yield return new WaitUntil(() => clicked);
            yield return new WaitUntil(() => tile4 == null || tile4.IsDone);

            // Khôi phục parent của container sau khi tile đã vào slot
            if (container2 != null && origContainerParent != null)
            {
                container2.transform.SetParent(origContainerParent, true);
                container2.transform.SetSiblingIndex(origContainerSiblingIndex);
                container2.SetHighlight(false);
            }
        }

        // Ẩn overlay hoàn toàn sau khi bài đã vào slot
        Tween.Alpha(upToAll, 0f, duration: 0.25f).OnComplete(() => upToAll.gameObject.SetActive(false));

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
        ItemCategory forcedCategory = defaultCategory;
        if (step == 1 && lastForcedStep < 1) // Vừa hoàn tất click bài bước 1, chuẩn bị chia bài cho bước 2
        {
            forcedCategory = theme1;
            lastForcedStep = 1;
        }
        else if (step == 2 && lastForcedStep < 2) // Vừa hoàn tất click bài bước 2, chuẩn bị chia bài cho bước 3
        {
            forcedCategory = theme2;
            lastForcedStep = 2;
        }
        else if (step == 3 && lastForcedStep < 3) // Vừa hoàn tất click bài bước 3, chuẩn bị chia bài cho bước 4
        {
            forcedCategory = theme2;
            lastForcedStep = 3;
        }

        if (forcedCategory != defaultCategory)
        {
            if (TileManager.Ins.itemCategoryPool.Contains(forcedCategory))
            {
                TileManager.Ins.itemCategoryPool.Add(defaultCategory);
                TileManager.Ins.itemCategoryPool.Remove(forcedCategory);
            }
            else
            {
                forcedCategory = defaultCategory;
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
        tutorialHand.transform.SetAsLastSibling();
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

    private void StartPointLoop(Transform target)
    {
        StopDragLoop();
        dragLoopCoroutine = StartCoroutine(PointLoop(target));
    }

    private IEnumerator PointLoop(Transform target)
    {
        if (tutorialHand == null || target == null) yield break;

        tutorialHand.gameObject.SetActive(true);
        tutorialHand.transform.SetParent(target, false);
        tutorialHand.transform.localPosition = Vector3.zero;
        tutorialHand.transform.localScale = Vector3.one;

        while (true)
        {
            Tween.Alpha(tutorialHand, 1f, duration: 0.25f);
            yield return new WaitForSeconds(0.25f);

            Tween.Scale(tutorialHand.transform, 0.9f, duration: 0.2f);
            yield return new WaitForSeconds(0.2f);
            Tween.Scale(tutorialHand.transform, 1.0f, duration: 0.2f);
            yield return new WaitForSeconds(0.6f);
        }
    }

    private IEnumerator RunLevelThirteenTutorialCoroutine()
    {
        // Step 0: Đợi game bắt đầu hoạt động
        yield return new WaitUntil(() => TileManager.Ins.isLevelActive && !TileManager.Ins.IsDealingCards);

        BoosterUI boosterUI = FindObjectOfType<BoosterUI>();
        Button freezeBtn = null;
        if (boosterUI != null)
        {
            freezeBtn = boosterUI.FreezeTimeButton;
        }
        if (freezeBtn == null)
        {
            GameObject freezeObj = GameObject.Find("Freeze");
            if (freezeObj != null)
            {
                freezeBtn = freezeObj.GetComponent<Button>();
            }
        }

        if (freezeBtn == null)
        {
            Debug.LogError("[Tutorial] Freeze booster button not found!");
            HideTutorial();
            yield break;
        }

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

        // Kích hoạt overlay
        upToAll.gameObject.SetActive(true);
        upToAll.color = new Color(upToAll.color.r, upToAll.color.g, upToAll.color.b, 0f);
        Tween.Alpha(upToAll, 0.87f, duration: 0.25f);

        if (textImageTrans != null)
        {
            textImageTrans.localPosition += new Vector3(0, 250, 0);
        }
        showText("Freeze <color=#FBC201>stops</color> the <color=#FBC201>timer!</color>");

        // Highlight nút Freeze bằng cách đưa lên overlay
        Transform origParent = freezeBtn.transform.parent;
        int origSiblingIndex = freezeBtn.transform.GetSiblingIndex();
        freezeBtn.transform.SetParent(upToAll.transform, true);
        freezeBtn.transform.SetAsLastSibling();

        // Chạy hiệu ứng bàn tay chỉ vào nút Freeze
        StartPointLoop(freezeBtn.transform);

        bool clicked = false;
        UnityEngine.Events.UnityAction onClickAction = () => clicked = true;
        freezeBtn.onClick.AddListener(onClickAction);

        yield return new WaitUntil(() => clicked);

        freezeBtn.onClick.RemoveListener(onClickAction);

        // Khôi phục parent của nút Freeze ngay lập tức
        if (freezeBtn != null && origParent != null)
        {
            freezeBtn.transform.SetParent(origParent, true);
            freezeBtn.transform.SetSiblingIndex(origSiblingIndex);
        }

        // Tắt bàn tay chỉ dẫn
        StopDragLoop();

        if (textImageTrans != null)
        {
            textImageTrans.localPosition -= new Vector3(0, 250, 0);
        }

        // Hiển thị text mới
        showText("Use it when <color=#FBC201>time</color> is <color=#FBC201>low</color>!");

        // Đợi 1.5s
        yield return new WaitForSeconds(1.5f);

        // Ẩn overlay
        Tween.Alpha(upToAll, 0f, duration: 0.25f).OnComplete(() => upToAll.gameObject.SetActive(false));
        if (textImageTrans != null) textImageTrans.gameObject.SetActive(false);

        // Kết thúc tutorial hoàn toàn
        if (TileManager.Ins.levelItem != null)
        {
            TileManager.Ins.levelItem.isTutorial = false;
        }
        HideTutorial();
    }
}
