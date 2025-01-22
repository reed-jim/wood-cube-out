using System;
using System.Collections;
using Dreamteck.Splines;
using Lean.Localization;
using PrimeTween;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelTutorial : MonoBehaviour
{
    [SerializeField] private Transform tutorialHand;
    [SerializeField] private SpriteRenderer tutorialHandSprite;
    [SerializeField] private Transform screw;
    [SerializeField] private Transform spline;
    [SerializeField] private SplinePositioner tutorialHandSplinePositioner;
    [SerializeField] private SkeletonGraphic pinchHand;

    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TMP_Text tutorialText;
    [SerializeField] private LeanLocalizedTextMeshProUGUI localizedTutorialText;

    [SerializeField] private Image rotateIcon;
    [SerializeField] private Image[] scaleIcons;
    [SerializeField] private SpriteRenderer scaleIconHand;

    [SerializeField] private string[] translationNames;
    [SerializeField] private Vector3 tutorialHandEulerAngles;
    [SerializeField] private Vector3 pinchTutorialHandPosition;

    #region PRIVATE FIELD
    private int _step;
    private Vector2 _screenSizeWorld;
    private bool _isConstraintHand;
    #endregion

    #region EVENT
    public static event Action<bool> enableSwipingEvent;
    public static event Action<bool> enablePinchingEvent;
    #endregion

    void Awake()
    {
        ScrewSelectionInput.selectScrewEvent += OnScrewSelected;
        ScrewSelectionInput.breakObjectEvent += OnScrewSelected;
        SwipeGesture.swipeGestureEvent += OnScreenSwiped;
        PinchGesture.pinchGestureEvent += OnScreenZoomed;

        StartCoroutine(TutorialStepOne());

        _screenSizeWorld = CommonUtil.GetScreenSizeWorld(Camera.main);

        enableSwipingEvent?.Invoke(false);
        enablePinchingEvent?.Invoke(false);

        rotateIcon.gameObject.SetActive(false);
        scaleIcons[0].gameObject.SetActive(false);
        scaleIcons[1].gameObject.SetActive(false);
        scaleIconHand.gameObject.SetActive(false);

        pinchHand.gameObject.SetActive(false);
    }

    void Update()
    {
        tutorialHand.rotation = Quaternion.Euler(tutorialHandEulerAngles);
        spline.rotation = Quaternion.Euler(new Vector3(30, 0, 0));

        if (_isConstraintHand)
        {
            tutorialHand.position = pinchTutorialHandPosition;
        }
    }

    void OnDestroy()
    {
        ScrewSelectionInput.selectScrewEvent -= OnScrewSelected;
        ScrewSelectionInput.breakObjectEvent -= OnScrewSelected;
        SwipeGesture.swipeGestureEvent -= OnScreenSwiped;
        PinchGesture.pinchGestureEvent -= OnScreenZoomed;
    }

    private IEnumerator TutorialStepOne()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(Time.deltaTime);

        tutorialHand.position = screw.position - 5f * screw.right;

        float lerpDelta = 0.08f;
        float lerpProgress = 0;
        Vector3 targetPosition;

        int phase = 0;

        while (_step == 0)
        {
            if (phase % 2 == 0)
            {
                targetPosition = screw.position - 0.5f * screw.right;
            }
            else
            {
                targetPosition = screw.position - 4 * screw.right;
                targetPosition.y -= 4;
            }

            targetPosition.y += 0.08f * _screenSizeWorld.y;
            targetPosition.z = -5;

            tutorialHand.position = Vector3.Lerp(tutorialHand.position, targetPosition, lerpDelta);

            lerpProgress += lerpDelta;

            if (lerpProgress >= 1)
            {
                phase++;
                lerpProgress = 0;
            }

            yield return waitForSeconds;
        }

        StartCoroutine(TutorialStepTwo());
    }

    private IEnumerator TutorialStepTwo()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(Time.deltaTime);

        float lerpDelta = 0.03f;
        float lerpProgress = 0;

        int phase = 0;

        localizedTutorialText.TranslationName = translationNames[1];

        rotateIcon.gameObject.SetActive(true);
        rotateIcon.color = ColorUtil.WithAlpha(rotateIcon.color, 0);

        Tween.Alpha(rotateIcon, 1, duration: 0.3f);

        tutorialHandSplinePositioner.enabled = true;
        // tutorialHandSplineFollower.follow = true;
        float percent = 0;

        while (_step == 1)
        {
            if (phase % 2 == 0)
            {
                percent = Mathf.Lerp(percent, 1, lerpDelta);
                tutorialHandSplinePositioner.SetPercent(percent);
                tutorialHand.rotation = Quaternion.Euler(tutorialHandEulerAngles);
                // tutorialHand.position = Vector3.Lerp(tutorialHand.position, new Vector3(-0.4f * _screenSizeWorld.x, tutorialHand.position.y, -5), lerpDelta);
            }
            else
            {
                percent = Mathf.Lerp(percent, 0, lerpDelta);
                tutorialHandSplinePositioner.SetPercent(percent);
                tutorialHand.rotation = Quaternion.Euler(tutorialHandEulerAngles);
                // tutorialHand.position = Vector3.Lerp(tutorialHand.position, new Vector3(0.4f * _screenSizeWorld.x, tutorialHand.position.y, -5), lerpDelta);
            }

            lerpProgress += lerpDelta;

            if (lerpProgress >= 1)
            {
                if (phase == 0)
                {
                    enableSwipingEvent?.Invoke(true);
                }

                phase++;
                lerpProgress = 0;
                // lerpDelta = 0.03f;
            }

            yield return waitForSeconds;
        }

        tutorialHandSplinePositioner.enabled = false;

        StartCoroutine(TutorialStepThree());
    }

    // private IEnumerator TutorialStepThree()
    // {
    //     float TIME_OUT = 5;

    //     WaitForSeconds waitForSeconds = new WaitForSeconds(Time.deltaTime);

    //     float lerpDelta = 0.25f;
    //     float lerpProgress = 0;

    //     int phase = 0;
    //     float timePassed = 0;

    //     localizedTutorialText.TranslationName = translationNames[2];

    //     // for (int i = 0; i < scaleIcons.Length; i++)
    //     // {
    //     //     scaleIcons[i].gameObject.SetActive(true);
    //     //     scaleIcons[i].color = ColorUtil.WithAlpha(scaleIcons[i].color, 0);
    //     // }

    //     // scaleIconHand.gameObject.SetActive(true);
    //     // scaleIconHand.color = ColorUtil.WithAlpha(scaleIconHand.color, 0);

    //     // Tween.Alpha(rotateIcon, 0, duration: 0.3f);

    //     // for (int i = 0; i < scaleIcons.Length; i++)
    //     // {
    //     //     Tween.Alpha(scaleIcons[i], 1, duration: 0.3f);
    //     // }

    //     // Tween.Alpha(scaleIconHand, 1, duration: 0.3f);

    //     Vector3 targetPosition;

    //     Vector2 screenSizeWorld = CommonUtil.GetScreenSizeWorld(Camera.main);

    //     enableSwipingEvent?.Invoke(false);

    //     while (_step == 2 && timePassed < TIME_OUT)
    //     {
    //         if (phase % 2 == 0)
    //         {
    //             targetPosition = new Vector3(-0.32f * screenSizeWorld.x, 0.1f * screenSizeWorld.y, tutorialHand.transform.position.z);
    //         }
    //         else
    //         {
    //             targetPosition = new Vector3(-0.32f * screenSizeWorld.x, 0.35f * screenSizeWorld.y, tutorialHand.transform.position.z);
    //         }

    //         // if (phase == 0)
    //         // {
    //         //     tutorialHand.position = Vector3.Lerp(tutorialHand.position, pinchTutorialHandPosition, lerpDelta);

    //         //     lerpProgress += lerpDelta;

    //         //     if (lerpProgress >= 1)
    //         //     {
    //         //         enablePinchingEvent?.Invoke(true);

    //         //         _isConstraintHand = true;
    //         //         phase++;
    //         //     }

    //         tutorialHand.position = Vector3.Lerp(tutorialHand.position, targetPosition, lerpDelta);

    //         lerpProgress += lerpDelta;

    //         if (lerpProgress >= 1)
    //         {
    //             phase++;
    //             lerpProgress = 0;

    //             if (phase % 2 == 0)
    //             {
    //                 lerpDelta = 0.08f;
    //             }
    //             else
    //             {
    //                 lerpDelta = 0.04f;
    //             }
    //         }

    //         timePassed += Time.deltaTime;

    //         yield return waitForSeconds;
    //     }

    //     EndTutorial();
    // }

    private IEnumerator TutorialStepThree()
    {
        float TIME_OUT = 5;

        WaitForSeconds waitForSeconds = new WaitForSeconds(Time.deltaTime);

        float timePassed = 0;



        pinchHand.gameObject.SetActive(true);

        pinchHand.color = ColorUtil.WithAlpha(pinchHand.color, 0);

        Tween.Alpha(pinchHand, 1, duration: 0.2f);

        for (int i = 0; i < scaleIcons.Length; i++)
        {
            scaleIcons[i].gameObject.SetActive(true);
            scaleIcons[i].color = ColorUtil.WithAlpha(scaleIcons[i].color, 0);
        }

        Tween.Alpha(rotateIcon, 0, duration: 0.3f);
        Tween.Alpha(tutorialHandSprite, 0, duration: 0.3f);

        for (int i = 0; i < scaleIcons.Length; i++)
        {
            Tween.Alpha(scaleIcons[i], 1, duration: 0.3f);
        }

        Tween.Alpha(scaleIconHand, 1, duration: 0.3f)
        .OnComplete(() =>
        {
            enablePinchingEvent?.Invoke(true);
        });

        while (_step == 2 && timePassed < TIME_OUT)
        {
            yield return waitForSeconds;
        }

        EndTutorial();
    }

    private void EndTutorial()
    {
        Tween.Custom(1, 0, duration: 0.3f, onValueChange: newVal =>
        {
            canvasGroup.alpha = newVal;
            scaleIconHand.color = ColorUtil.WithAlpha(scaleIconHand.color, newVal);
        });
        Tween.Alpha(tutorialHandSprite, 0, duration: 0.3f).OnComplete(() =>
        {
            gameObject.SetActive(false);
        });

        enableSwipingEvent?.Invoke(true);
    }

    #region CALLBACK
    private void OnScrewSelected()
    {
        if (_step == 0)
        {
            _step++;
        }
    }

    private void OnScreenSwiped(Vector2 swipeDirection)
    {
        if (_step == 1)
        {
            _step++;
        }
    }

    private void OnScreenZoomed(float orthographicSize)
    {
        if (_step == 2)
        {
            _step++;
        }
    }
    #endregion
}
