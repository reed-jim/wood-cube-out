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

public class TutorialLevelTen : Singleton<TutorialLevelTen>
{
    [Header("Unity Prefab References")]
    public SkeletonGraphic tutorialHand;
    public CanvasGroup tutorialContainer;
    public TMP_Text description;
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
        
        tutorialCoroutine = StartCoroutine(RunLevelTenTutorialCoroutine());
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

    private IEnumerator RunLevelTenTutorialCoroutine()
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
        if (boosterUI == null || boosterUI.ShuffleButton == null)
        {
            Debug.LogError("[Tutorial] BoosterUI or ShuffleButton is null!");
            HideTutorial();
            yield break;
        }
        
        Button shuffleBtn = boosterUI.ShuffleButton;

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
        showText("Shuffle mixes the board!");

        Transform origShuffleParent = shuffleBtn.transform.parent;
        int origShuffleSiblingIndex = shuffleBtn.transform.GetSiblingIndex();
        
        shuffleBtn.transform.SetParent(upToAll.transform, true);
        shuffleBtn.transform.SetAsLastSibling();
        
        // Bàn tay tap
        StartTapLoop(shuffleBtn.transform);
        
        bool shuffleUsed = false;
        
        // Capture shuffle usage
        UnityEngine.Events.UnityAction onShuffleClicked = () =>
        {
            shuffleUsed = true;
        };
        shuffleBtn.onClick.AddListener(onShuffleClicked);
        
        yield return new WaitUntil(() => shuffleUsed);
        shuffleBtn.onClick.RemoveListener(onShuffleClicked);
        
        StopDragLoop();
        
        // Khôi phục ShuffleButton ngay lập tức
        if (shuffleBtn != null && origShuffleParent != null)
        {
            shuffleBtn.transform.SetParent(origShuffleParent, true);
            shuffleBtn.transform.SetSiblingIndex(origShuffleSiblingIndex);
        }
        
        // Tắt upToAll tạm thời để người chơi xem animation shuffle
        Tween.Alpha(upToAll, 0f, duration: 0.25f).OnComplete(() => {
            upToAll.gameObject.SetActive(false);
        });
        
        // --- BƯỚC 2 ---
        step = 2;
        
        // Ẩn text đi lúc đang shuffle
        if (textImageTrans != null) textImageTrans.gameObject.SetActive(false);
        if (textImageTrans != null)
        {
            textImageTrans.localPosition -= new Vector3(0, 250, 0);
        }
        
        // Chờ TileManager thực hiện Shuffle (IsShuffling sẽ true rồi false)
        // Đợi một chút để IsShuffling được set = true (trong UseShuffleBooster)
        yield return new WaitForSeconds(0.2f);
        yield return new WaitUntil(() => !TileManager.Ins.IsShuffling);
        
        // Sau khi shuffle xong, bật lại text hướng dẫn "Find new matches!"
        upToAll.gameObject.SetActive(true);
        Tween.Alpha(upToAll, 0.7f, duration: 0.25f);
        
        showText("Find new matches!");
        
        // Hiện text trong 2 giây
        yield return new WaitForSeconds(2f);
        
        // Kết thúc tutorial hoàn toàn
        step = 3;
        if (TileManager.Ins.levelItem != null)
        {
            TileManager.Ins.levelItem.isTutorial = false;
        }
        
        if (textImageTrans != null) textImageTrans.gameObject.SetActive(false);
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
            
            tutorialHand.transform.position = target.position + new Vector3(30f, -30f, 0f);
            tutorialHand.AnimationState.SetAnimation(0, "tap", false);
            
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
