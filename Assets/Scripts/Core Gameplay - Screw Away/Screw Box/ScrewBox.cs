using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CandyCoded.HapticFeedback;
using Firebase.RemoteConfig;
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
    public bool isFreezeddddd;

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

    private void Awake()
    {
        ScrewBoxCameraManager.setCameraEvent += OnScrewBoxCameraSet;
        ScrewBoxSlot.screwBoxCompleteEvent += OnScrewBoxCompleted;
        ScrewBoxUI.unlockScrewBox += Unlock;

        _sequences = new List<Sequence>();

        boxLid.gameObject.SetActive(false);

        gameSetting.LoadAsync();
    }

    private void OnDestroy()
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

        // scaleWithWidth *= 1.25f;
        // scaleWithHeight *= 1.25f;

        float finalScale = Mathf.Min(scaleWithWidth, scaleWithHeight);

        transform.localScale = finalScale * transform.localScale / transform.lossyScale.x;

        _initialScale = transform.localScale;

        _isPlayBoxCompleteFx = RemoteConfigController.GetBoolConfig(FirebaseConfig.IS_PLAY_BOX_COMPLETE_FX, true);
    }

    private async void OnScrewBoxCompleted(int instanceId)
    {
        if (instanceId == gameObject.GetInstanceID())
        {
            await Task.Delay((int)(delayMoveInDuration * 1000));

            SoundManager.Instance.PlaySoundScrewBoxDone();

            float initialPositionY = transform.position.y;

            boxLid.gameObject.SetActive(true);

            boxLid.localPosition = boxLid.localPosition.ChangeY(9);

            _sequences.Add(Tween.LocalPositionY(boxLid, 0.4f, duration: closeBoxLidDuration)
                .OnComplete(() =>
                {
                    shakeCameraEvent?.Invoke();
                    // Tween.ShakeCamera(Camera.main, strengthFactor: 0.3f, duration: closeBoxLidDuration);

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
                                rippleFx.Play();
                            }
                        }
                        catch (System.Exception e)
                        {

                        }

                        playFullscreenEffectEvent?.Invoke(FactionUtility.GetColorForFaction(Faction));
                    }
                })
                .Chain(Tween.ScaleY(transform, 0.6f * _initialScale.y, duration: scaleDownDuration)
                .Chain(Tween.LocalPositionY(transform, transform.position.y + 8, startDelay: delayMoveOutDuration, duration: moveOutDuration).OnComplete(() =>
                {
                    screwBoxCompletedEvent?.Invoke(this);
                    spawnNewScrewBoxEvent?.Invoke();

                    transform.position = transform.position.ChangeY(initialPositionY);
                    transform.localScale = _initialScale;

                    boxLid.gameObject.SetActive(false);

                    ObjectPoolingEverything.ReturnToPool(GameConstants.SCREW_BOX, gameObject, transform.parent);
                })))
            );
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
}
