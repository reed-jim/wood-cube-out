using System.Collections;
using Lean.Localization;
using PrimeTween;
using TMPro;
using UnityEngine;

public class LevelTutorial : MonoBehaviour
{
    [SerializeField] private Transform tutorialHand;
    [SerializeField] private SpriteRenderer tutorialHandSprite;
    [SerializeField] private Transform screw;

    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TMP_Text tutorialText;
    [SerializeField] private LeanLocalizedTextMeshProUGUI localizedTutorialText;

    [SerializeField] private string[] translationNames;

    #region PRIVATE FIELD
    private int _step;
    private Vector2 _screenSizeWorld;
    #endregion

    void Awake()
    {
        ScrewSelectionInput.selectScrewEvent += OnScrewSelected;
        SwipeGesture.swipeGestureEvent += OnScreenSwiped;

        StartCoroutine(TutorialStepOne());

        _screenSizeWorld = CommonUtil.GetScreenSizeWorld(Camera.main);
    }

    void Update()
    {
        tutorialHand.rotation = Quaternion.Euler(Vector3.zero);
    }

    void OnDestroy()
    {
        ScrewSelectionInput.selectScrewEvent -= OnScrewSelected;
        SwipeGesture.swipeGestureEvent -= OnScreenSwiped;
    }

    private IEnumerator TutorialStepOne()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(Time.deltaTime);

        tutorialHand.position = screw.position - 5f * screw.right;

        float lerpDelta = 0.05f;
        float lerpProgress = 0;
        Vector3 targetPosition;

        int phase = 0;

        while (_step == 0)
        {
            if (phase % 2 == 0)
            {
                targetPosition = screw.position - screw.right;
            }
            else
            {
                targetPosition = screw.position - 5 * screw.right;
            }

            targetPosition.y += 0.09f * _screenSizeWorld.y;
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

        float lerpDelta = 1f / 4;
        float lerpProgress = 0;

        int phase = 0;

        localizedTutorialText.TranslationName = translationNames[1];

        while (_step == 1)
        {
            if (phase % 2 == 0)
            {
                tutorialHand.position = Vector3.Lerp(tutorialHand.position, new Vector3(-0.3f * _screenSizeWorld.x, tutorialHand.position.y, -5), lerpDelta);
            }
            else
            {
                tutorialHand.position = Vector3.Lerp(tutorialHand.position, new Vector3(0.4f * _screenSizeWorld.x, tutorialHand.position.y, -5), lerpDelta);
            }

            lerpProgress += lerpDelta;

            if (lerpProgress >= 1)
            {
                phase++;
                lerpProgress = 0;
                lerpDelta = 0.03f;
            }

            yield return waitForSeconds;
        }

        EndTutorial();
    }

    private void EndTutorial()
    {
        Tween.Custom(1, 0, duration: 0.3f, onValueChange: newVal =>
        {
            canvasGroup.alpha = newVal;
        });
        Tween.Alpha(tutorialHandSprite, 0, duration: 0.3f).OnComplete(() =>
        {
            gameObject.SetActive(false);
        });
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
    #endregion
}
