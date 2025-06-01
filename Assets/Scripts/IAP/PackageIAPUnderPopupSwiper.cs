using System;
using PrimeTween;
using UnityEngine;
using UnityEngine.EventSystems;

public class PackageIAPUnderPopupSwiper : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private RectTransform target;

    [SerializeField] private RectTransform[] packs;

    [SerializeField] private RectTransform packOne;
    [SerializeField] private RectTransform packTwo;

    [SerializeField] private int maxPack;

    private Vector2 _startPos;
    private float _swipeThreshold = 50f; // Minimum distance for a swipe

    private float _distance;

    private bool _isSwiping;
    private int _currentIndex;

    public static event Action requestFetchIAPPrice;

    void Awake()
    {
        _distance = packTwo.localPosition.x - packOne.localPosition.x;
    }

    private void OnEnable()
    {
        try
        {
            target.gameObject.SetActive(true);

            ShowSalePacks();

            Tween.Delay(0.3f).OnComplete(() =>
            {
                requestFetchIAPPrice?.Invoke();
            });
        }
        catch (System.Exception)
        {
            target.gameObject.SetActive(false);
        }
    }

    private void ShowSalePacks()
    {
        Vector3 currentPosition = Vector3.zero;

        int suggestionPackIndex = (int)GamePersistentVariable.saleIAPPackType;

        currentPosition.y = packs[suggestionPackIndex].localPosition.y;

        packs[suggestionPackIndex].localPosition = currentPosition;

        for (int i = 0; i < packs.Length; i++)
        {
            if (i == suggestionPackIndex)
            {
                continue;
            }

            currentPosition.x += _distance;
            currentPosition.y = packs[i].localPosition.y;

            packs[i].gameObject.SetActive(true);

            packs[i].localPosition = currentPosition;
        }

        if (suggestionPackIndex == 0 || suggestionPackIndex == 1)
        {
            for (int i = 2; i < packs.Length; i++)
            {
                packs[i].gameObject.SetActive(false);
            }

            maxPack = 2;
        }

        if (suggestionPackIndex >= 2 && suggestionPackIndex <= 4)
        {
            for (int i = 5; i < packs.Length; i++)
            {
                packs[i].gameObject.SetActive(false);
            }

            maxPack = 5;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _startPos = eventData.position;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (_isSwiping)
        {
            return;
        }

        Vector2 endPos = eventData.position;
        Vector2 delta = endPos - _startPos;

        if (delta.magnitude >= _swipeThreshold)
        {
            if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
            {
                // RIGHT
                if (delta.x > 0)
                {
                    if (_currentIndex - 1 >= 0)
                    {
                        Tween.LocalPositionX(target, target.localPosition.x + _distance, duration: 0.3f)
                        .OnComplete(() =>
                        {
                            _isSwiping = false;
                        });

                        _currentIndex--;
                    }
                    else
                    {
                        Tween.LocalPositionX(target, target.localPosition.x + 100, duration: 0.3f, cycles: 2, cycleMode: CycleMode.Yoyo)
                        .OnComplete(() =>
                            {
                                _isSwiping = false;
                            });
                    }
                }
                else
                {
                    if (_currentIndex + 1 < maxPack)
                    {
                        Tween.LocalPositionX(target, target.localPosition.x - _distance, duration: 0.3f)
                        .OnComplete(() =>
                        {
                            _isSwiping = false;
                        });

                        _currentIndex++;
                    }
                    else
                    {
                        Tween.LocalPositionX(target, target.localPosition.x - 100, duration: 0.3f, cycles: 2, cycleMode: CycleMode.Yoyo)
                        .OnComplete(() =>
                            {
                                _isSwiping = false;
                            });
                    }
                }

                _isSwiping = true;
            }
        }
    }
}
