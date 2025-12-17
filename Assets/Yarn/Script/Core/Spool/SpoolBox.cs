using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CandyCoded.HapticFeedback;
using Firebase.RemoteConfig;
using NamPhuThuy.Common;
using NamPhuThuy.Data;
using PrimeTween;
using Saferio.Util.SaferioTween;
using UnityEngine;
using static GameEnum;

public class SpoolBox : MonoBehaviour
{
    [SerializeField] private SpoolBoxSlot[] spoolBoxSlots;
    [SerializeField] private Transform boxLid;
    [SerializeField] private SpriteRenderer spoolBoxSpriteRenderer;
    [SerializeField] private SpriteRenderer spoolBoxShadowSpriteRenderer;

    [SerializeField] private SpoolBoxServiceLocator spoolBoxServiceLocator;

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

    public static event Action<SpoolBox> spoolBoxCompletedEvent;
    public static event Action spawnNewSpoolBoxEvent;
    public static event Action<SpoolBox> setFactionForSpoolBoxEvent;
    public static event Action<SpoolBox> spoolBoxUnlockedEvent;
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
        get => spoolBoxServiceLocator.spoolBoxProperty.Faction;
        set => spoolBoxServiceLocator.spoolBoxProperty.Faction = value;
    }

    public SpoolBoxSlot[] SpoolBoxSlots
    {
        get => spoolBoxSlots;
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

    public SpoolBoxServiceLocator SpoolBoxServiceLocator
    {
        get => spoolBoxServiceLocator;
    }

    #region MonoBehaviour Callbacks

    private void Awake()
    {
        SpoolBoxSlot.spoolBoxCompleteEvent += OnSpoolBoxCompleted;
        SpoolBoxUI.unlockSpoolBox += Unlock;

        _sequences = new List<Sequence>();

        boxLid.gameObject.SetActive(false);

        gameSetting.LoadAsync();
    }

    private void OnDestroy()
    {
        SpoolBoxSlot.spoolBoxCompleteEvent -= OnSpoolBoxCompleted;
        SpoolBoxUI.unlockSpoolBox -= Unlock;

        CommonUtil.StopAllSequences(_sequences);
    }

    #endregion

    public void SetSpoolSize(Vector2 worldSize)
    {
        transform.SetLossyScale(Vector3.one);

        float scaleWithWidth = (0.19f * worldSize.x) / spoolBoxSpriteRenderer.bounds.size.x;
        float scaleWithHeight = (0.12f * worldSize.y) / spoolBoxSpriteRenderer.bounds.size.y;

        float finalScale = Mathf.Min(scaleWithWidth, scaleWithHeight);

        transform.SetLossyScale(finalScale * Vector3.one);

        _initialScale = transform.localScale;
        _initialSize = spoolBoxSpriteRenderer.bounds.size;

        _isPlayBoxCompleteFx = RemoteConfigController.GetBoolConfig(FirebaseConfig.IS_PLAY_BOX_COMPLETE_FX, true);
    }

    private async void OnSpoolBoxCompleted(int instanceId)
    {
        // DebugLogger.Log(context:this);
        if (instanceId != gameObject.GetInstanceID()) return;

        await Task.Delay((int)(delayMoveInDuration * 300));

        SoundManager.Instance.PlaySoundScrewBoxDone();

        float initialPositionY = transform.localPosition.y;
        float initialBottomMostPositionY = initialPositionY - 0.5f * _initialSize.y;

        boxLid.gameObject.SetActive(true);

        boxLid.localPosition = boxLid.localPosition.ChangeY(9);

        // float hidePositionY = transform.position.y + 8;

        // if (GamePersistentVariable.currentTargetOrthographicSize < 10)
        // {
        //     hidePositionY = transform.position.y + 8 + 60 / GamePersistentVariable.currentTargetOrthographicSize;
        // }

        float hidePositionY = transform.position.y + 8 + 60 / GamePersistentVariable.currentTargetOrthographicSize;

        _sequences.Add(Tween.LocalPositionY(boxLid, 0.12f, duration: closeBoxLidDuration)
            .OnComplete(() =>
            {
                shakeCameraEvent?.Invoke();

                if (gameSetting.IsVibrate)
                {
                    HapticFeedback.HeavyFeedback();
                }

                // FX
                if (_isPlayBoxCompleteFx)
                {
                    try
                    {
                        ParticleSystem rippleFx =
                            ObjectPoolingEverything.GetFromPool<ParticleSystem>(GameConstants.BOX_COMPLETED_FX);

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
                            rippleFx.transform.position = transform.position.ChangeZ(10); // -9

                            // if (GamePersistentVariable.currentTargetOrthographicSize < 10)
                            // {
                            //     rippleFx.transform.localScale = 0.25f * Vector3.one;
                            // }
                            // else
                            // {
                            //     rippleFx.transform.localScale = 0.5f * Vector3.one;
                            // }

                            float initialScale = 0.5f;
                            float BEST_ORTHOGRAPHIC_SIZE = 20;

                            rippleFx.transform.localScale =
                                initialScale *
                                (GamePersistentVariable.currentTargetOrthographicSize / BEST_ORTHOGRAPHIC_SIZE) *
                                Vector3.one;

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
                            transform.localPosition.ChangeY(initialBottomMostPositionY +
                                                            newVal * 0.5f * _initialSize.y);
                    }
                ).OnComplete(() =>
                {
                    Tween.Scale(transform, _initialScale, startDelay: delayMoveOutDuration,
                        duration: 0.5f * moveOutDuration, ease: Ease.InOutSine);
                    Tween.LocalPositionY(transform, hidePositionY, startDelay: delayMoveOutDuration,
                            duration: moveOutDuration, ease: Ease.InOutSine)
                        .OnComplete(() =>
                        {
                            spoolBoxCompletedEvent?.Invoke(this);
                            spawnNewSpoolBoxEvent?.Invoke();

                            transform.position = transform.position.ChangeY(initialPositionY);
                            transform.localScale = _initialScale;

                            boxLid.gameObject.SetActive(false);

                            ObjectPoolingEverything.ReturnToPool(GameConstants.SCREW_BOX, gameObject, transform.parent);
                        });
                })
            )
        );

        foreach (var spoolBoxSlot in spoolBoxSlots)
        {
            spoolBoxSlot.SmoothSetDissolveHeight(1, 0);
        }
    }

    public void Lock()
    {
        spoolBoxSpriteRenderer.enabled = false;
        spoolBoxShadowSpriteRenderer.enabled = false;

        spoolBoxServiceLocator.spoolBoxUI.Lock();

        isLocked = true;
    }

    public void Unlock()
    {
        DebugLogger.Log(context: this);
        spoolBoxSpriteRenderer.enabled = true;
        spoolBoxShadowSpriteRenderer.enabled = true;

        // CLEAR SCREWS
        for (int i = 0; i < spoolBoxSlots.Length; i++)
        {
            spoolBoxSlots[i].Reset();
        }

        isLocked = false;

        GamePersistentVariable.numSpoolBoxUnlocked++;

        setFactionForSpoolBoxEvent?.Invoke(this);
        // spoolBoxUnlockedEvent?.Invoke(this);
    }

    private void Unlock(int instanceId)
    {
        DebugLogger.Log(message: $"instanceID {instanceId}", context: this);
        if (instanceId == gameObject.GetInstanceID())
        {
            Unlock();
        }
    }

    public void ResetVisually()
    {
        DebugLogger.Log(context: this);
        spoolBoxSpriteRenderer.enabled = true;
        spoolBoxShadowSpriteRenderer.enabled = true;

        for (int i = 0; i < spoolBoxSlots.Length; i++)
        {
            spoolBoxSlots[i].ResetVisually();
        }

        spoolBoxServiceLocator.spoolBoxUI.HideUnlockBoxButton();
    }

    public void InvokeSpoolBoxUnlockedEvent()
    {
        DebugLogger.Log(message: $"About to invoke spoolBoxUnlockEvent", context: this);
        spoolBoxUnlockedEvent?.Invoke(this);
    }
}