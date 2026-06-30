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

public class TutorialLevelSeven : Singleton<TutorialLevelSeven>
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
        
        tutorialCoroutine = StartCoroutine(RunLevelSevenTutorialCoroutine());
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

    private IEnumerator RunLevelSevenTutorialCoroutine()
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
        if (boosterUI == null || boosterUI.HintButton == null)
        {
            Debug.LogError("[Tutorial] BoosterUI or HintButton is null!");
            HideTutorial();
            yield break;
        }
        
        Button hintBtn = boosterUI.HintButton;

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
        
        if (textImageTrans != null)
        {
            textImageTrans.localPosition += new Vector3(0, 250, 0);
        }
        showText("Use Hint to Reveal a match!");

        Transform origHintParent = hintBtn.transform.parent;
        int origHintSiblingIndex = hintBtn.transform.GetSiblingIndex();
        
        hintBtn.transform.SetParent(upToAll.transform, true);
        hintBtn.transform.SetAsLastSibling();
        
        // Bàn tay tap
        StartTapLoop(hintBtn.transform);
        
        bool hintUsed = false;
        
        // Capture hint usage
        UnityEngine.Events.UnityAction onHintClicked = () =>
        {
            hintUsed = true;
        };
        hintBtn.onClick.AddListener(onHintClicked);
        
        yield return new WaitUntil(() => hintUsed);
        hintBtn.onClick.RemoveListener(onHintClicked);
        
        StopDragLoop();
        
        // Khôi phục HintButton
        if (hintBtn != null && origHintParent != null)
        {
            hintBtn.transform.SetParent(origHintParent, true);
            hintBtn.transform.SetSiblingIndex(origHintSiblingIndex);
        }
        
        // --- BƯỚC 2 ---
        step = 2;
        
        if (textImageTrans != null)
        {
            textImageTrans.localPosition -= new Vector3(0, 250, 0);
        }
        showText("Try it now!");
        
        // Chờ TileManager xử lý logic UseHintBooster (khoảng 0.5s để hiệu ứng hoàn tất)
        yield return new WaitForSeconds(0.5f);
        
        // Tìm tile và container đang được highlight (TileManager đưa chúng vào upToAll)
        TileSlotContainer activeContainer = null;
        BaseTile highlightedTile = null;
        
        foreach (var container in TileManager.Ins.TileSlotContainers)
        {
            if (container.transform.parent == upToAll.transform)
            {
                activeContainer = container;
                break;
            }
        }
        
        foreach (var tile in TileManager.Ins.tiles)
        {
            if (tile.transform.parent == upToAll.transform)
            {
                highlightedTile = tile;
                break;
            }
        }
        
        if (activeContainer != null && highlightedTile != null)
        {
            StartDragLoop(highlightedTile.transform, activeContainer.transform);
            
            Action oldInteract = highlightedTile.onInteract;
            bool cardClicked = false;
            highlightedTile.onInteract = () =>
            {
                if (cardClicked) return;
                
                // Ẩn text và bàn tay
                if (textImageTrans != null) textImageTrans.gameObject.SetActive(false);
                StopDragLoop();
                cardClicked = true;

                oldInteract?.Invoke();
            };
            
            yield return new WaitUntil(() => cardClicked);
            yield return new WaitUntil(() => highlightedTile == null || highlightedTile.IsOnSlot);
        }
        else
        {
            // Fallback nếu có lỗi logic, tự kết thúc sớm
            yield return new WaitForSeconds(2f);
        }
        
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
