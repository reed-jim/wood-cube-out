using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BirdSort2;
using CandyCoded.HapticFeedback;
using PrimeTween;
using Saferio.Util.SaferioTween;
using UnityEngine;
using static GameEnum;

public class ScrewBox : MonoBehaviour
{
    [SerializeField] private ScrewBoxSlot[] screwBoxSlots;
    [SerializeField] private Transform boxLid;
    [SerializeField] private SpriteRenderer screwBoxSpriteRenderer;

    [SerializeField] private ScrewBoxServiceLocator screwBoxServiceLocator;

    [Header("CUSTOMIZE")]
    [SerializeField] private bool isLocked;
    [SerializeField] private float delayMoveInDuration;
    [SerializeField] private float closeBoxLidDuration;
    [SerializeField] private float scaleDownDuration;
    [SerializeField] private float delayMoveOutDuration;
    [SerializeField] private float moveOutDuration;

    [Header("SCRIPTABLE OBJECT")]
    [SerializeField] protected GameSetting gameSetting;

    #region EVENT
    public static event Action shakeCameraEvent;
    #endregion

    #region PRIVATE FIELD
    private List<Tween> _tweens;
    [SerializeField]public Vector3 _initialScale;
    private bool _isInTransition;
    #endregion

    public GameFaction Faction
    {
        get => screwBoxServiceLocator.screwBoxFaction.Faction;
        set => screwBoxServiceLocator.screwBoxFaction.Faction = value;
    }

    public ScrewBoxSlot[] ScrewBoxSlots
    {
        get => screwBoxSlots;
    }

    public bool IsLocked
    {
        get => isLocked;
        set => isLocked = value;
    }

    public bool IsInTransition
    {
        get => _isInTransition;
        set => _isInTransition = value;
    }

    public ScrewBoxServiceLocator ScrewBoxServiceLocator
    {
        get => screwBoxServiceLocator;
    }

    private void Awake()
    {
        _tweens = new List<Tween>();

        boxLid.gameObject.SetActive(false);
        
    }

    private void OnDestroy()
    {
        CommonUtil.StopAllTweens(_tweens);
    }
    public async void OnScrewBoxCompleted(int instanceId)
    {
        if (instanceId == gameObject.GetInstanceID())
        {
            await Task.Delay((int)(delayMoveInDuration * 1000));
            GameController.Instance.musicManager.PlayOneShot(BirdSort2.NameMusic.ScrewBoxDone);

            float initialPositionY = transform.position.y;

            boxLid.gameObject.SetActive(true);

            boxLid.localPosition = boxLid.localPosition.ChangeY(9);

            Tween.LocalPositionY(boxLid, 0.4f, duration: closeBoxLidDuration)
            .OnComplete(() =>
            {
                shakeCameraEvent?.Invoke();
                // Tween.ShakeCamera(Camera.main, strengthFactor: 0.3f, duration: closeBoxLidDuration);
                
                if (GameController.Instance.useProfile.OnVibration)
                {
                    HapticFeedback.HeavyFeedback();
                }
            })
            .Chain(Tween.ScaleY(transform, 0.6f * _initialScale.y, duration: scaleDownDuration)
            .Chain(Tween.LocalPositionY(transform, transform.position.y + 8, startDelay: delayMoveOutDuration, duration: moveOutDuration).OnComplete(() =>
            {
                GamePlayController_BirdSort.Instance.level.screwBoxManager.OnScrewBoxCompleted(this);
                GamePlayController_BirdSort.Instance.level.screwManager.SpawnNewScrewBox();

                transform.position = transform.position.ChangeY(initialPositionY);
                transform.localScale = _initialScale;

                boxLid.gameObject.SetActive(false);

                ObjectPoolingEverything.ReturnToPool(GameConstants.SCREW_BOX, gameObject, transform.parent);
            })));
        }
    }

    public void Lock()
    {
        screwBoxSpriteRenderer.enabled = false;

        screwBoxServiceLocator.screwBoxUI.Lock();

        isLocked = true;
    }

    public void Unlock()
    {
        screwBoxSpriteRenderer.enabled = true;

        // CLEAR SCREWS
        for (int i = 0; i < screwBoxSlots.Length; i++)
        {
            if (screwBoxSlots[i].IsFilled)
            {
                screwBoxSlots[i].IsFilled = false;

                Destroy(screwBoxSlots[i].Screw.gameObject);

                screwBoxSlots[i].Screw = null;
            }
        }

        isLocked = false;

        GamePlayController_BirdSort.Instance.level.screwManager.AssignFactionForNewAdsScrewBox(this);
        GamePlayController_BirdSort.Instance.level.screwManager.OnScrewBoxUnlocked(this);
        GamePlayController_BirdSort.Instance.level.screwBoxManager.OnScrewBoxUnlocked(this);
    }

    public void Unlock(int instanceId)
    {
        if (instanceId == gameObject.GetInstanceID())
        {
            Unlock();
        }
    }
}
