using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Spine;
using Spine.Unity;

public class StartLoading : MonoBehaviour
{
    public TMP_Text txtLoading;
    public Image progressBar;
    public bool clearData;

    [Header("SPINE ANIMATION")]
    [SerializeField] private SkeletonGraphic loadingSpine;
    [SpineAnimation] public string idleAnimation;
    [SpineAnimation] public string appearAnimation;

    [Header("SCROLLING ANT BOXES")]
    [SerializeField] private RectTransform[] scrollingAntBoxes;
    private Vector3[] scrollingAntBoxInitialPositions;
    private Coroutine _scrollingCoroutine;

    private void Awake()
    {
        if (scrollingAntBoxes != null && scrollingAntBoxes.Length > 0)
        {
            scrollingAntBoxInitialPositions = new Vector3[scrollingAntBoxes.Length];
            for (int i = 0; i < scrollingAntBoxes.Length; i++)
            {
                if (scrollingAntBoxes[i] != null)
                {
                    scrollingAntBoxInitialPositions[i] = scrollingAntBoxes[i].localPosition;
                }
            }
        }
    }

    private void Start()
    {
        if (scrollingAntBoxes != null && scrollingAntBoxes.Length > 0)
        {
            if (_scrollingCoroutine != null)
            {
                StopCoroutine(_scrollingCoroutine);
            }
            _scrollingCoroutine = StartCoroutine(ScrollingBackground());
        }

        if (loadingSpine != null)
        {
            loadingSpine.gameObject.SetActive(false);
        }
    }

    public void Init()
    {
        Application.targetFrameRate = 60;
        progressBar.fillAmount = 0f;

        StartCoroutine(ChangeScene());
        StartCoroutine(LoadingText());
    }

    public void PlaySpineAnimation()
    {
        if (loadingSpine == null) return;

        loadingSpine.gameObject.SetActive(true);
        loadingSpine.Skeleton.SetToSetupPose();
        loadingSpine.AnimationState.ClearTracks();
        TrackEntry entry = loadingSpine.AnimationState.SetAnimation(0, appearAnimation, false);
        loadingSpine.AnimationState.Apply(loadingSpine.Skeleton);
        loadingSpine.Skeleton.UpdateWorldTransform();
        loadingSpine.AnimationState.Complete += OnAnimationComplete;

        void OnAnimationComplete(TrackEntry trackEntry)
        {
            loadingSpine.AnimationState.Complete -= OnAnimationComplete;

            if (trackEntry != null && trackEntry.Animation != null && trackEntry.Animation.Name == appearAnimation)
            {
                loadingSpine.AnimationState.SetAnimation(0, idleAnimation, false);
            }
        }
    }

    IEnumerator ChangeScene()
    {
        string sceneLoad = SceneName.HOME_SCENE;
        if (UseProfile.CurrentLevel <= RemoteConfigController.GetIntConfig(FirebaseConfig.START_LEVEL_BACK_TO_HOME, 5))
            sceneLoad = SceneName.GAME_PLAY;

        Application.backgroundLoadingPriority = ThreadPriority.Low;

        var asyncOperation = SceneManager.LoadSceneAsync(sceneLoad);
        asyncOperation.allowSceneActivation = false;

        float minWaitTime = 2f;
        float elapsed = 0f;
        bool spineStarted = false;

        while (!asyncOperation.isDone)
        {
            elapsed += Time.deltaTime;
            progressBar.fillAmount = Mathf.Clamp01(asyncOperation.progress / 0.9f);

            if (!spineStarted && asyncOperation.progress >= 0.9f)
            {
                spineStarted = true;
                PlaySpineAnimation();
            }

            if (asyncOperation.progress >= 0.9f && elapsed >= minWaitTime)
            {
                progressBar.fillAmount = 1f;

                // Bàn giao cho Fader: fade in (tối dần) → activate scene → fade out khi SceneTransition xong.
                // ChangeScene không cần làm gì thêm, Fader tiếp quản toàn bộ.
                Initiate.FadeWithExistingOp(Color.black, 3f, asyncOperation);
                yield break;
            }

            yield return null;
        }

        Application.backgroundLoadingPriority = ThreadPriority.Normal;
    }


    IEnumerator LoadingText()
    {
        var wait = new WaitForSeconds(1f);
        while (true)
        {
            txtLoading.text = Localization.Get("s_loading") + " .";
            yield return wait;

            txtLoading.text = Localization.Get("s_loading") + "..";
            yield return wait;

            txtLoading.text = Localization.Get("s_loading") + "...";
            yield return wait;

        }
    }

    private IEnumerator ScrollingBackground()
    {
        if (scrollingAntBoxes == null || scrollingAntBoxes.Length == 0) yield break;

        float moveSpeed = 300f;
        Vector2 resetOffset = scrollingAntBoxes[0].sizeDelta;
        Vector2 direction = new Vector2(1f, 1f).normalized;

        for (int i = 0; i < scrollingAntBoxes.Length; i++)
        {
            if (scrollingAntBoxes[i] != null)
            {
                scrollingAntBoxes[i].localPosition = scrollingAntBoxInitialPositions[i];
            }
        }

        while (true)
        {
            for (int i = 0; i < scrollingAntBoxes.Length; i++)
            {
                RectTransform box = scrollingAntBoxes[i];
                if (box == null) continue;

                // Move toward top-right
                box.anchoredPosition += direction * moveSpeed * Time.deltaTime;

                // If it has moved too far, wrap it back
                if (box.anchoredPosition.x > resetOffset.x ||
                    box.anchoredPosition.y > resetOffset.y)
                {
                    box.anchoredPosition -= resetOffset;
                }
            }

            yield return null;
        }
    }
}
