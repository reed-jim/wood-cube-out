using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CandyCoded.HapticFeedback;
using Firebase.RemoteConfig;
using NamPhuThuy;
using PrimeTween;
using Saferio.Util.SaferioTween;
using UnityEngine;
using static GameEnum;

public class ScrewBox : MonoBehaviour
{
    [SerializeField] private ScrewBoxSlot[] screwBoxSlots;
    [SerializeField] private Transform boxLid;
    [SerializeField] protected SpriteRenderer screwBoxSpriteRenderer;

    [SerializeField] protected ScrewBoxServiceLocator screwBoxServiceLocator;

    [Header("CUSTOMIZE")]
    [SerializeField] protected bool isLocked;
    [SerializeField] private float delayMoveInDuration;
    [SerializeField] private float closeBoxLidDuration;
    [SerializeField] private float scaleDownDuration;
    [SerializeField] private float delayMoveOutDuration;
    [SerializeField] private float moveOutDuration;

    [Header("SCRIPTABLE OBJECT")]
    [SerializeField] protected GameSetting gameSetting;

    #region EVENT
    public static event Action<ScrewBox> screwBoxCompletedEvent;
    public static event Action spawnNewScrewBoxEvent;
    public static event Action<ScrewBox> setFactionForScrewBoxEvent;
    public static event Action<ScrewBox> screwBoxUnlockedEvent;
    public static event Action shakeCameraEvent;
    public static event Action<Color> playFullscreenEffectEvent;
    #endregion

    #region PRIVATE FIELD
    private List<Sequence> _sequences;
    private Vector3 _initialScale;
    private Vector3 _initialSize;
    private bool _isInTransition;
    private bool _isPlayBoxCompleteFx;
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

    protected virtual void Awake()
    {
        ScrewBoxCameraManager.setCameraEvent += OnScrewBoxCameraSet;
        ScrewBoxSlot.screwBoxCompleteEvent += OnScrewBoxCompleted;
        ScrewBoxUI.unlockScrewBox += Unlock;

        _sequences = new List<Sequence>();

        if (boxLid != null)
        {
            boxLid.gameObject.SetActive(false);
        }
    }

    protected virtual void OnDestroy()
    {
        ScrewBoxCameraManager.setCameraEvent -= OnScrewBoxCameraSet;
        ScrewBoxSlot.screwBoxCompleteEvent -= OnScrewBoxCompleted;
        ScrewBoxUI.unlockScrewBox -= Unlock;

        CommonUtil.StopAllSequences(_sequences);
    }

    private void OnScrewBoxCameraSet(Camera screwBoxCamera)
    {
        Vector2 worldSize = CommonUtil.GetScreenSizeWorld(screwBoxCamera);

        float scaleWithWidth = (0.18f * worldSize.x) * transform.localScale.x / screwBoxSpriteRenderer.bounds.size.x;
        float scaleWithHeight = (0.11f * worldSize.y) * transform.localScale.y / screwBoxSpriteRenderer.bounds.size.y;

        scaleWithWidth *= 1.05f;
        scaleWithHeight *= 1.05f;

        float finalScale = Mathf.Min(scaleWithWidth, scaleWithHeight);

        transform.localScale = finalScale * transform.localScale / transform.lossyScale.x;

        _initialScale = transform.localScale;
        _initialSize = screwBoxSpriteRenderer.bounds.size;

        _isPlayBoxCompleteFx = RemoteConfigController.GetBoolConfig(FirebaseConfig.IS_PLAY_BOX_COMPLETE_FX, true);
    }

    private async void OnScrewBoxCompleted(int instanceId)
    {
        DebugLogger.LogSimple(message:$"instanceId: {instanceId}");
        if (instanceId != gameObject.GetInstanceID()) return;
        
        await Task.Delay((int)(delayMoveInDuration * 300));

        SoundManager.Instance.PlaySoundScrewBoxDone();

        float initialPositionY = transform.localPosition.y;
        float initialBottomMostPositionY = initialPositionY - 0.5f * _initialSize.y;

        boxLid.gameObject.SetActive(true);

        boxLid.localPosition = boxLid.localPosition.ChangeY(9);

        float hidePositionY = transform.position.y + 8 + 60 / GamePersistentVariable.currentTargetOrthographicSize;

        _sequences.Add(Tween.LocalPositionY(boxLid, 0.4f, duration: closeBoxLidDuration)
            .OnComplete(() =>
            {
                shakeCameraEvent?.Invoke();

                if (GamePersistentVariable.IsVibrate)
                {
                    HapticFeedback.HeavyFeedback();
                }

                // FX
                if (_isPlayBoxCompleteFx)
                {
                    try
                    {
                        ParticleSystem rippleFx = ObjectPoolingEverything.GetFromPool<ParticleSystem>(GameConstants.BOX_COMPLETED_FX);
                        
                        if (rippleFx != null)
                        {
                            Color factionColor = 1.3f * FactionUtility.GetColorForFaction(Faction);

                            ParticleSystem.MainModule rippleFxMain = rippleFx.main;

                            rippleFxMain.startColor = factionColor;

                            for (int i = 0; i < rippleFx.transform.childCount; i++)
                            {
                                ParticleSystem childFx = rippleFx.transform.GetChild(i).GetComponent<ParticleSystem>();

                                ParticleSystem.MainModule childFxMain = childFx.main;

                                childFxMain.startColor = factionColor;
                            }

                            rippleFx.gameObject.SetActive(true);
                            rippleFx.transform.position = transform.position.ChangeZ(-9);

                            float initialScale = 0.5f;
                            float BEST_ORTHOGRAPHIC_SIZE = 20;

                            rippleFx.transform.localScale = initialScale * (GamePersistentVariable.currentTargetOrthographicSize / BEST_ORTHOGRAPHIC_SIZE) * Vector3.one;

                            rippleFx.Play();
                        }
                    }
                    catch (System.Exception e)
                    {

                    }

                    playFullscreenEffectEvent?.Invoke(FactionUtility.GetColorForFaction(Faction));
                }
            })
            .Chain
            (
                Tween.Custom(1, 0.6f, duration: scaleDownDuration, onValueChange: newVal =>
                {
                    transform.localScale = transform.localScale.ChangeY(newVal * _initialScale.y);
                    transform.localPosition =
                        transform.localPosition.ChangeY(initialBottomMostPositionY + newVal * 0.5f * _initialSize.y);
                }
                ).OnComplete(() =>
                {
                    Tween.Scale(transform, _initialScale, startDelay: delayMoveOutDuration, duration: 0.5f * moveOutDuration, ease: Ease.InOutSine);
                    Tween.LocalPositionY(transform, hidePositionY, startDelay: delayMoveOutDuration, duration: moveOutDuration, ease: Ease.InOutSine)
                    .OnComplete(() =>
                    {
                        screwBoxCompletedEvent?.Invoke(this);
                        spawnNewScrewBoxEvent?.Invoke();

                        transform.position = transform.position.ChangeY(initialPositionY);
                        transform.localScale = _initialScale;

                        boxLid.gameObject.SetActive(false);

                        ObjectPoolingEverything.ReturnToPool(GameConstants.SCREW_BOX, gameObject, transform.parent);
                    });
                })
            )
        );
    }

    public virtual void Lock()
    {
        DebugLogger.LogSimple();

        screwBoxSpriteRenderer.enabled = false;
        screwBoxServiceLocator.screwBoxUI.Lock();
        isLocked = true;
    }

    public virtual void Unlock()
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

        GamePersistentVariable.numScrewBoxUnlocked++;

        setFactionForScrewBoxEvent?.Invoke(this);
        // screwBoxUnlockedEvent?.Invoke(this);
    }

    private void Unlock(int instanceId)
    {
        if (instanceId == gameObject.GetInstanceID())
        {
            Unlock();
        }
    }

    public void ResetVisually()
    {
        screwBoxSpriteRenderer.enabled = true;

        screwBoxServiceLocator.screwBoxUI.HideUnlockBoxButton();
    }

    public void InvokeScrewBoxUnlockedEvent()
    {
        screwBoxUnlockedEvent?.Invoke(this);
    }

    #region CHECKING
    public virtual bool IsValidToUnlock()
    {
        return isLocked;
    }
    #endregion
}
