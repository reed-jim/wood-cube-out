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

public class TutorialLevelSixteen : Singleton<TutorialLevelSixteen>
{
    [Header("Unity Prefab References")]
    public SkeletonGraphic tutorialHand;
    public CanvasGroup tutorialContainer;
    public CanvasGroup continueButtonCanvasGroup;
    public Button continueButton;

    public Canvas gameplayCanvas;
    public Image blackOverlay;

    private Coroutine tutorialCoroutine;
    private int step = 0;
    
    public void StartTutorial()
    {
        if (tutorialCoroutine != null)
        {
            StopCoroutine(tutorialCoroutine);
        }
        
        step = 0;
        if (tutorialContainer != null) tutorialContainer.gameObject.SetActive(false);
        if (blackOverlay != null) blackOverlay.gameObject.SetActive(false);
        if (tutorialHand != null) tutorialHand.gameObject.SetActive(false);
        
        tutorialCoroutine = StartCoroutine(RunLevelSixteenTutorialCoroutine());
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
        if (TileManager.Ins != null && TileManager.Ins.UpToAll != null)
        {
            TileManager.Ins.UpToAll.gameObject.SetActive(false);
        }
        gameObject.SetActive(false);
    }

    private IEnumerator RunLevelSixteenTutorialCoroutine()
    {
        // Step 0: Đợi chia bài ban đầu hoàn thành
        yield return new WaitUntil(() => TileManager.Ins.isLevelActive && !TileManager.Ins.IsDealingCards);
        yield return new WaitForSeconds(0.2f);

        Image upToAll = TileManager.Ins.UpToAll;
        if (upToAll == null)
        {
            Debug.LogError("[Tutorial] upToAll is null!");
            HideTutorial();
            yield break;
        }
        
        BoosterUI boosterUI = FindObjectOfType<BoosterUI>();
        if (boosterUI == null || boosterUI.UnlockThemeButton == null)
        {
            Debug.LogError("[Tutorial] BoosterUI or UnlockThemeButton is null!");
            HideTutorial();
            yield break;
        }
        
        Button unlockThemeBtn = boosterUI.UnlockThemeButton;
        BoosterItemUI unlockItemUI = unlockThemeBtn.GetComponentInParent<BoosterItemUI>();

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
        
        upToAll.gameObject.SetActive(true);
        upToAll.color = new Color(upToAll.color.r, upToAll.color.g, upToAll.color.b, 0f);
        Tween.Alpha(upToAll, 0.7f, duration: 0.25f);
        
        // Đẩy text lên trên để tránh bị vướng nút booster ở dưới
        if (textImageTrans != null)
        {
            textImageTrans.localPosition += new Vector3(0, 250, 0);
        }
        showText("Add a new theme slot!");

        // Highlight nút UnlockTheme và theme thứ 3 (đang bị khóa)
        // Theme thứ 3 bị khóa là tileSlotContainers[2]
        TileSlotContainer targetContainer = null;
        if (TileManager.Ins.TileSlotContainers.Count > 2)
        {
            targetContainer = TileManager.Ins.TileSlotContainers[2];
        }

        Transform origUnlockBtnParent = unlockThemeBtn.transform.parent;
        int origUnlockBtnSiblingIndex = unlockThemeBtn.transform.GetSiblingIndex();
        
        unlockThemeBtn.transform.SetParent(upToAll.transform, true);
        unlockThemeBtn.transform.SetAsLastSibling();

        Transform origContainerParent = null;
        int origContainerSiblingIndex = -1;
        if (targetContainer != null)
        {
            origContainerParent = targetContainer.transform.parent;
            origContainerSiblingIndex = targetContainer.transform.GetSiblingIndex();
            targetContainer.transform.SetParent(upToAll.transform, true);
            // Để targetContainer xếp dưới unlockThemeBtn
            targetContainer.transform.SetSiblingIndex(unlockThemeBtn.transform.GetSiblingIndex());

            if (targetContainer.GlowUnlockTheme != null)
            {
                targetContainer.GlowUnlockTheme.gameObject.SetActive(true);
                Color c = targetContainer.GlowUnlockTheme.color;
                c.a = 1f;
                targetContainer.GlowUnlockTheme.color = c;
            }
        }
        
        // Bàn tay tap chỉ vào nút UnlockTheme
        StartTapLoop(unlockThemeBtn.transform);
        
        bool unlockUsed = false;
        UnityEngine.Events.UnityAction onUnlockClicked = () =>
        {
            unlockUsed = true;
        };
        unlockThemeBtn.onClick.AddListener(onUnlockClicked);
        
        yield return new WaitUntil(() => unlockUsed);
        unlockThemeBtn.onClick.RemoveListener(onUnlockClicked);
        
        StopDragLoop();

        if (targetContainer != null && targetContainer.GlowUnlockTheme != null)
        {
            targetContainer.GlowUnlockTheme.gameObject.SetActive(false);
        }

        // Chuẩn bị cho anim unlock: Thêm Canvas với overrideSorting để boosterIcon vẽ đè lên upToAll mà không đổi parent
        Canvas iconCanvas = null;
        if (unlockItemUI != null && unlockItemUI.boosterIcon != null)
        {
            iconCanvas = unlockItemUI.boosterIcon.gameObject.AddComponent<Canvas>();
            iconCanvas.overrideSorting = true;
            iconCanvas.sortingOrder = 30000; // Đặt giá trị lớn để hiển thị trên overlay
        }

        // Khôi phục nút UnlockThemeButton về parent ban đầu ngay sau khi click
        if (unlockThemeBtn != null && origUnlockBtnParent != null)
        {
            unlockThemeBtn.transform.SetParent(origUnlockBtnParent, true);
            unlockThemeBtn.transform.SetSiblingIndex(origUnlockBtnSiblingIndex);
        }

        // Đợi kết thúc anim bay và unlock theme.
        yield return new WaitForSeconds(0.1f);
        yield return new WaitUntil(() => !boosterUI.IsInTransition);

        // Hủy Canvas của BoosterIcon sau khi bay xong
        if (iconCanvas != null)
        {
            Destroy(iconCanvas);
        }

        // Khôi phục theme container parent
        if (targetContainer != null && origContainerParent != null)
        {
            targetContainer.transform.SetParent(origContainerParent, true);
            targetContainer.transform.SetSiblingIndex(origContainerSiblingIndex);
        }

        // --- BƯỚC 2 ---
        step = 2;
        
        // Trả vị trí text về bình thường
        if (textImageTrans != null)
        {
            textImageTrans.localPosition -= new Vector3(0, 250, 0);
        }
        showText("Match even more cards!");

        // Highlight theme đã mở khóa và các lá bài cùng theme với theme thứ 3 bằng glow
        ItemCategory targetCategory = ItemCategory.None;
        if (targetContainer != null)
        {
            targetCategory = targetContainer.ItemCategory;
        }

        var originalParents = new Dictionary<Transform, Transform>();
        var originalSiblingIndices = new Dictionary<Transform, int>();

        // 1. Highlight theme slot đã mở khóa
        if (targetContainer != null)
        {
            originalParents[targetContainer.transform] = targetContainer.transform.parent;
            originalSiblingIndices[targetContainer.transform] = targetContainer.transform.GetSiblingIndex();

            targetContainer.transform.SetParent(upToAll.transform, true);
            targetContainer.transform.SetAsLastSibling();
            targetContainer.SetHighlight(true);
        }

        // 2. Highlight các lá bài cùng theme thứ 3
        var tilesToHighlight = new List<BaseTile>();
        if (targetCategory != ItemCategory.None)
        {
            foreach (var tile in TileManager.Ins.tiles)
            {
                if (tile != null && !tile.IsOnSlot && !tile.IsOutside && !tile.IsDone && tile.ItemCategory == targetCategory)
                {
                    tilesToHighlight.Add(tile);

                    originalParents[tile.transform] = tile.transform.parent;
                    originalSiblingIndices[tile.transform] = tile.transform.GetSiblingIndex();

                    tile.transform.SetParent(upToAll.transform, true);
                    tile.transform.SetAsLastSibling();
                    tile.SetHighlight(true);
                }
            }
        }

        // Đợi 3 giây
        yield return new WaitForSeconds(3f);

        // Khôi phục trạng thái highlight và parent
        if (targetContainer != null)
        {
            targetContainer.SetHighlight(false);
            if (originalParents.ContainsKey(targetContainer.transform) && originalParents[targetContainer.transform] != null)
            {
                targetContainer.transform.SetParent(originalParents[targetContainer.transform], true);
                targetContainer.transform.SetSiblingIndex(originalSiblingIndices[targetContainer.transform]);
            }
        }

        foreach (var tile in tilesToHighlight)
        {
            if (tile != null)
            {
                tile.SetHighlight(false);
                if (originalParents.ContainsKey(tile.transform) && originalParents[tile.transform] != null)
                {
                    tile.transform.SetParent(originalParents[tile.transform], true);
                    tile.transform.SetSiblingIndex(originalSiblingIndices[tile.transform]);
                }
            }
        }

        // Ẩn overlay và text
        Tween.Alpha(upToAll, 0f, duration: 0.25f).OnComplete(() => upToAll.gameObject.SetActive(false));
        if (textImageTrans != null) textImageTrans.gameObject.SetActive(false);

        // Kết thúc tutorial hoàn toàn
        step = 3;
        if (TileManager.Ins.levelItem != null)
        {
            TileManager.Ins.levelItem.isTutorial = false;
        }
        HideTutorial();
    }

    private Coroutine dragLoopCoroutine;
    private Coroutine tapLoopCoroutine;

    private void StartDragLoop(Transform from, Transform to)
    {
        StopDragLoop();
        dragLoopCoroutine = StartCoroutine(DragLoop(from, to));
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
    
    private void StartTapLoop(Transform target)
    {
        StopDragLoop();
        tapLoopCoroutine = StartCoroutine(TapLoop(target));
    }

    private IEnumerator TapLoop(Transform target)
    {
        if (tutorialHand == null) yield break;
        
        tutorialHand.gameObject.SetActive(true);
        tutorialHand.transform.SetParent(TileManager.Ins.UpToAll.transform, true);
        tutorialHand.transform.SetAsLastSibling();
        tutorialHand.transform.localScale = Vector3.one;
        
        while (true)
        {
            if (target == null) yield break;
            
            tutorialHand.transform.position = target.position;
            tutorialHand.AnimationState.SetAnimation(0, "action", false);
            
            yield return new WaitForSeconds(1f);
        }
    }

    private void StopDragLoop()
    {
        if (dragLoopCoroutine != null)
        {
            StopCoroutine(dragLoopCoroutine);
            dragLoopCoroutine = null;
        }
        if (tapLoopCoroutine != null)
        {
            StopCoroutine(tapLoopCoroutine);
            tapLoopCoroutine = null;
        }
        if (tutorialHand != null)
        {
            tutorialHand.gameObject.SetActive(false);
        }
    }
}
