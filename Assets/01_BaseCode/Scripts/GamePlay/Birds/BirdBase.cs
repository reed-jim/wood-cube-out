using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;
using Spine.Unity;
using Spine;
using System;
using Newtonsoft.Json;
//using MoreMountains.NiceVibrations;

public enum StateBird
{
    Idle = 0,
    Flying = 1,
    FlyOut = 2,
    InSlot = 3,
    MoveRandom = 4,
    Match3 = 5

}
public class BirdBase : MonoBehaviour
{
    public const string FLY = "fly";
    public const string GLIDE = "fly2";
    public const string GROUNDING = "grounding";
    public const string IDLE = "idle";
    public const string IDLE_2 = "idle2";
    public const string PRE_SLEEP = "pre-sleep";
    public const string SLEEP = "sleep";
    public const string WAKE_UP = "wakeup";
    public const string SELLECT = "select";
    public const string WALK = "walk";
    public const string DIE = "die";

    [Header("Data")]
    public int id;
    public Sprite icon;
    [SerializeField] protected float speedMove = 5;
    [SerializeField] protected float speedMoveToSlot = 5;
    public float speedMoveInGadern = 1.5f;
    public float speedMovePathInGadern = 0.8f;
    public float speedMerge = 2.5f;
    public Transform hitLaserPoint;
    public Collider2D collider2D;
    [SerializeField] protected float scaleBirdSlot = 0.7f;
    [SerializeField] protected ClickBirdClass clickBirdClass;
    public bool isBirdHasCover;//Bird bị che và che
    public bool isBirdInCover;//Bird bị che và che
    public bool isRoping;//Bird đang nối vs 1 bird khác

    [Header("FSM")]
    public FSMBird fsm;

    [Header("Anim")]
    protected TrackEntry entryAnim;
    public SkeletonAnimation body;
    public MeshRenderer meshRenderer;
    [SerializeField] protected GameObject scaleBody;
    //[SerializeField] protected MaterialPropertyBlock normalMaterial;
    //[SerializeField] protected MaterialPropertyBlock outLineMaterial;
    protected Material normalMaterial;
    [SerializeField] protected Material outLineMaterial;
    [SerializeField] protected Material outLineRedMaterial;


    [Header("Out Line")]
    [SerializeField] protected GameObject outLineObj;
    [SerializeField] protected GameObject outLineCanMoveObj;
    protected Action<object> actionDisableOutline;

    [Header("Sound")]
    [SerializeField] protected AudioClip birdCallClip;
    [SerializeField] protected AudioClip birdFlyClip;
    [SerializeField] protected AudioClip birdFlyOutClip;
    [SerializeField] protected AudioClip sellectClip;
    [SerializeField] protected AudioClip pickUpClip;

    [SerializeField] protected AudioSource audioFlySource;
    [SerializeField] protected AudioSource audioSourceIdle;
    [SerializeField] protected AudioSource audioCallSource;
    private float timmerCallSound;
    private float timmerSoundIdle;
    private float timmerDelayIdle;
    [SerializeField] protected bool isCallSoundIdle;

    /// <summary>
    /// State
    /// </summary>
    public StateBird state;
    protected Action<object> actionFlyRandom;
    [HideInInspector] public int m_isLockMove;
    public bool isLockMove
    {
        get
        {
            return m_isLockMove > 0;
        }
        set
        {
            if (value)
                m_isLockMove++;
            else
            {
                m_isLockMove--;
                if (m_isLockMove < 0)
                    m_isLockMove = 0;

                EventDispatcher.EventDispatcher.Instance.PostEvent(EventID.UNLOCK_MOVE_BIRD, this);
            }

            //if (m_isLockMove <= 0)
            //{
            //    body.CustomMaterialOverride[normalMaterial] = outLineMaterial;
            //}
        }
    }

    [HideInInspector] public int m_isHasExtral;
    public bool isHasExtral
    {
        get
        {
            return m_isHasExtral > 0;
        }
        set
        {
            if (value)
                m_isHasExtral++;
            else
            {
                m_isHasExtral--;
                if (m_isHasExtral < 0)
                    m_isHasExtral = 0;
            }
        }
    }

    [HideInInspector] public bool onGround;
    [HideInInspector] public bool isBirdInGadern;
    [HideInInspector] public bool isDie;
    [HideInInspector] public bool isMatch3Done;
    public CheckPointTran checkPointIn;
    
    protected IEnumerator flyInitCroutine;
    protected Action<object> onCatchBird;
    protected Action<object> onExitBird;

    protected IEnumerator dieCroutine;


    [HideInInspector] public List<BirdBase> birdsDependence;


    public virtual void Init(Vector3 posStart, Vector3 posMiddle, Vector3 posSpawn, UnityAction actionCome, bool isFlyOut, float delayTimeFlyOut = 0)
    {
        fsm.Init(this);
        ResetState();
        birdsDependence = new List<BirdBase>();
        if (clickBirdClass == null)
            clickBirdClass = body.GetComponent<ClickBirdClass>();

        if (clickBirdClass != null)
            clickBirdClass.Init(this);

        if (collider2D == null)
            collider2D = body.GetComponent<BoxCollider2D>();
        //body.gameObject.SetActive(false);
        bool isLeft = posSpawn.x <= 0 ? true : false;

        this.transform.DOKill();

        var level = GamePlayController.Instance.level;
        var posLimitLeft = level.posLimitScreenTL.position;
        var posLimitRight = level.posLimitScreenTR.position;
        var posLimitMiddle = (level.posLimitScreenTL.position + level.posLimitScreenBL.position) / 2;

        if (isLeft)
        {
            this.transform.position = new Vector3(
                posLimitLeft.x - 1,
                posStart.y,
                0);
        }
        else
        {
            this.transform.position = new Vector3(
               posLimitRight.x + 1,
               posStart.y,
               0);
        }

        UnityAction actionBirdReady = () =>
        {
            actionCome?.Invoke();
            fsm.ChangeState(FSMBird.StateFSM.Idle);
            ChangeLayer(checkPointIn.indexLayer, LayerGame.BIRD);
            GamePlayController.Instance.level.CheckAllBirdReady();

        };

        if (isFlyOut)
        {
            flyInitCroutine = Helper.StartAction(() =>
            {
                FlyCome(checkPointIn.posMiddle.position, posSpawn, () =>
                {
                    actionBirdReady();
                }, checkPointIn.branch);
            }, delayTimeFlyOut);
            StartCoroutine(flyInitCroutine);

            //fsm.ChangeState(FSMBird.StateFSM.FlyCurrentCheckPoint);
        }
        else
        {
            this.transform.position = new Vector3(posSpawn.x, posSpawn.y + 0.5f, 0);
            FlyCome(checkPointIn.posMiddle.position, posSpawn, () =>
            {
                actionBirdReady();

            }, checkPointIn.branch);
        }


        actionDisableOutline = (sender) => DisableOutLine();
        EventDispatcher.EventDispatcher.Instance.RegisterListener(EventID.DISABLE_OUTLINE_BIRD, actionDisableOutline);

        timmerCallSound = 1f;
        timmerDelayIdle = UnityEngine.Random.Range(4, 6);

        actionFlyRandom = (sender) => FlyOutRandom((BirdBase)sender, true);
        EventDispatcher.EventDispatcher.Instance.RegisterListener(EventID.FLY_OUT_RANDOM, actionFlyRandom);

        onCatchBird = (sender) => OnEventCatchBird((BirdBase)sender);
        EventDispatcher.EventDispatcher.Instance.RegisterListener(EventID.CATCH_BIRD, onCatchBird);

        onExitBird = (sender) => OnEventFreeBird((BirdBase)sender);
        EventDispatcher.EventDispatcher.Instance.RegisterListener(EventID.EXIT_BIRD, onExitBird);

        isBirdInGadern = true;

        //skeletonGhost = body.GetComponent<SkeletonGhostRenderer>();
        //skeletonGhost.enabled = false;

        if (body.SkeletonDataAsset != null)
            normalMaterial = body.SkeletonDataAsset.atlasAssets[0].PrimaryMaterial;

        //DisableOutLine();
    }

    public void ResetState()
    {
        isBirdInGadern = false;
        isDie = false;
        isMatch3Done = false;
        onGround = false;
        m_isLockMove = 0;
        timmerCallSound = 0;
        timmerSoundIdle = 0;
        timmerDelayIdle = 0;
        state = StateBird.Idle;
        fsm.ResetState();
    }
    public virtual void HightLight()
    {
        if (!isLockMove)
        {
            if (body.SkeletonDataAsset != null)
                body.CustomMaterialOverride[normalMaterial] = outLineMaterial; // to enable the replacement.

            //meshRenderer.SetPropertyBlock(outLineMaterial);

            //outLineObj.transform.localScale = Vector3.one;
            //outLineCanMoveObj.transform.localScale = Vector3.zero;
        }
        else
        {
            if (body.SkeletonDataAsset != null)
                body.CustomMaterialOverride[normalMaterial] = outLineRedMaterial; // to enable the replacement.

            // DisableOutLine();


            //meshRenderer.SetPropertyBlock(normalMaterial);

            //outLineObj.transform.localScale = Vector3.zero;
            //outLineCanMoveObj.transform.localScale = Vector3.one;
        }

        if (timmerCallSound >= 1)
        {
         //   GameController.Instance.musicManager.PlayOneShot(NameMusic.Fly, birdCallClip, audioCallSource);
            timmerCallSound = 0;
        }

    }

    public virtual void DisableOutLine()
    {
        //outLineObj.transform.localScale = Vector3.zero;
        //outLineCanMoveObj.transform.localScale = Vector3.zero;
        if (body.SkeletonDataAsset != null)
            body.CustomMaterialOverride[normalMaterial] = normalMaterial;
    }

    public virtual void StopFlySound()
    {
        audioFlySource.DOFade(0, 0.5f);
    }

    public virtual void PlaySoundIdle()
    {
        if (!isCallSoundIdle)
            return;
        timmerSoundIdle += Time.deltaTime;
        if (timmerSoundIdle >= timmerDelayIdle)
        {
            timmerSoundIdle = 0;
         //   GameController.Instance.musicManager.PlayOneShot(NameMusic.Idle, birdCallClip, audioSourceIdle);
            timmerDelayIdle = UnityEngine.Random.Range(5, 7);
        }
    }

    public virtual void DisableBody()
    {
        body.gameObject.SetActive(false);
        //Test
        /// transform.GetChild(0).transform.GetChild(2).gameObject.SetActive(false);
    }

    public virtual void ActiveBody()
    {
        body.gameObject.SetActive(true);
    }

    /// <summary>
    /// Bị sét đánh
    /// </summary>
    public virtual void LightingHit()
    {
        ///FX
        fsm.ClearFSM();
        isDie = true;
        collider2D.enabled = false;
        this.transform.DOKill();
        body.transform.DOKill();
        PlayAnim(DIE, false);
        dieCroutine = Helper.StartAction(DisableBody, 0.4f);
        StartCoroutine(dieCroutine);
        ChangeLayer(1000, LayerGame.BIRD_SLOT);
        //Gọi FX Merge
        var fx = GameController.Instance.fXController.GetFX(FXController.TypeFX.ExplosionLightning);
        fx.transform.position = this.transform.position;
    }

    public virtual void ActiveMatch3()
    {
        scaleBody.transform.DOKill();
        scaleBody.transform.localScale = new Vector3(scaleBirdSlot, scaleBirdSlot, 1);
        scaleBody.transform.DOScale(1.2f, 0.25f).OnComplete(() =>
        {
            scaleBody.transform.DOScale(0f, 0.15f).OnComplete(() =>
            {
                DisableBody();
            });
        }).SetEase(Ease.OutBack);

        //Gọi FX Merge
        var fx = GameController.Instance.fXController.GetFX(FXController.TypeFX.Combine);
        fx.transform.position = this.transform.position + new Vector3(0, 0.15f, 0);

        GamePlayController.Instance.level.CheckAllRequiremnt();

        if (GameController.Instance.useProfile.OnVibration)
        {
          //  MMVibrationManager.Haptic(HapticTypes.Success, false, true, this);
        }
    }

    #region Layer

    public virtual void ChangeLayer(int layer, string layerName)
    {
        meshRenderer.sortingLayerName = layerName;
        meshRenderer.sortingOrder = layer;

    }

    public virtual void ReloadCurrentLayer()
    {
        if (checkPointIn != null)
            ChangeLayer(checkPointIn.indexLayer, LayerGame.BIRD);
    }

    public virtual void SetLayerPosZ()
    {
        float layer = -(float)meshRenderer.sortingOrder / 1000;
        this.transform.position = new Vector3(this.transform.position.x, this.transform.position.y, layer < -100 ? -100 : layer);
    }
    #endregion

    #region Fly
    private UnityAction actionComeDontDestry;
    public void FlyToSlot(Vector3 _posMiddle, Vector3 _posTrans, int indexLayer, UnityAction actionCome)
    {
        _posMiddle.z = 0;
        _posTrans.z = 0;
        this.transform.position = new Vector3(this.transform.position.x, this.transform.position.y, 0);
        FlyCome(_posMiddle, _posTrans, () =>
        {
            state = StateBird.InSlot;
            actionCome?.Invoke();
            if (this.actionComeDontDestry != null)
            {
                this.actionComeDontDestry.Invoke();
                this.actionComeDontDestry = null;
            }
            float valueScaleX = Mathf.Abs(body.transform.localScale.x);
            body.transform.localScale = new Vector3(valueScaleX, body.transform.localScale.y, body.transform.localScale.z);
        }, speed: speedMoveToSlot);
        ChangeLayer(indexLayer, LayerGame.BIRD_SLOT);
    }

    public void FlyToSlot(Vector3 _posMiddle, Vector3 _posTrans, int indexLayer, UnityAction actionCome, UnityAction actionComeDontDestry = null, UnityAction actionScaleDone = null)
    {
        _posMiddle.z = 0;
        _posTrans.z = 0;
        this.transform.position = new Vector3(this.transform.position.x, this.transform.position.y, 0);
        if (checkPointIn != null)
        {
            checkPointIn.birdOn = null;
            checkPointIn = null;
        }


        FlyCome(_posMiddle, _posTrans, () =>
        {
            state = StateBird.InSlot;
            actionCome?.Invoke();

            this.actionComeDontDestry = actionComeDontDestry;
            if (this.actionComeDontDestry != null)
            {
                this.actionComeDontDestry.Invoke();
                this.actionComeDontDestry = null;
            }
            float valueScaleX = Mathf.Abs(body.transform.localScale.x);
            body.transform.localScale = new Vector3(valueScaleX, body.transform.localScale.y, body.transform.localScale.z);
        }, speed: speedMoveToSlot);

        float timeMove = Vector2.Distance(this.transform.position, _posTrans) / speedMoveToSlot;
        scaleBody.transform.DOKill();
        scaleBody.transform.DOScale(scaleBirdSlot, timeMove).OnComplete(() =>
        {
            actionScaleDone?.Invoke();

        });
        ChangeLayer(indexLayer, LayerGame.BIRD_SLOT);
    }

    public virtual void FlyCome(Vector3 _posMiddle, Vector3 _posTrans, UnityAction actionCome, StandingBase standing = null, float speed = -1, Ease typeMove = Ease.Unset)
    {
        if (state == StateBird.MoveRandom || state == StateBird.Match3)
            return;

        onGround = false;
        Vector3 posMiddle = _posMiddle;
        Vector3 posTrans = _posTrans;

        state = StateBird.Flying;
        if (body.SkeletonDataAsset != null)
        {
            body.AnimationState.ClearTracks();
            body.AnimationState.SetAnimation(0, FLY, true);
        }
        GamePlayController.Instance.level.isBirdFlying = true;
        this.transform.DOKill();
        this.transform.position = new Vector3(this.transform.position.x, this.transform.position.y, 0);
        this.transform.parent = GamePlayController.Instance.level.birdParent;

        float timeMove = Vector2.Distance(this.transform.position, posTrans) / (speed != -1 ? speed : speedMove);

        this.transform.DOMove(posMiddle, timeMove).OnComplete(() =>
        {
            state = StateBird.Idle;

            float valueScaleX = Mathf.Abs(body.transform.localScale.x);
            if (body.transform.position.x >= 0)
            {
                body.transform.localScale = new Vector3(-valueScaleX, body.transform.localScale.y, body.transform.localScale.z);
            }
            else
            {
                body.transform.localScale = new Vector3(valueScaleX, body.transform.localScale.y, body.transform.localScale.z);
            }
            GroundingHandle(posTrans);
            // float timeMove = Vector2.Distance(this.transform.position, posTrans) / speedMove;
            this.transform.DOMove(posTrans, 0.5f).OnComplete(() =>
            {
                onGround = true;
                if (standing != null)
                    standing.Shake();
                //Debug.Log("Rung cây");
                StopFlySound();
                SetLayerPosZ();
            });

            actionCome?.Invoke();
        }).SetEase(typeMove);

        float valueScaleX = Mathf.Abs(body.transform.localScale.x);
        if (posTrans.x > this.transform.position.x)
        {
            body.transform.localScale = new Vector3(valueScaleX, body.transform.localScale.y, body.transform.localScale.z);
        }
        else
        {
            body.transform.localScale = new Vector3(-valueScaleX, body.transform.localScale.y, body.transform.localScale.z);
        }

        //if (posTrans.y < this.transform.position.y)
        //{
        //    PlayAnim(FLY, false, () => { PlayAnim(GLIDE, true); });
        //}
        //else
        //{
        //    PlayAnim(FLY, false);
        //}

   //     GameController.Instance.musicManager.PlayOneShot(NameMusic.Fly, birdFlyClip, audioFlySource);
    }

    public virtual void FlyComePath(Vector3[] path, Vector3 _posTrans, UnityAction actionCome, StandingBase standing = null, float speed = -1, Ease typeMove = Ease.Unset)
    {
        if (state == StateBird.MoveRandom || state == StateBird.Match3)
            return;

        onGround = false;
        Vector3 posTrans = _posTrans;

        state = StateBird.Flying;
        if (body.SkeletonDataAsset != null)
        {
            body.AnimationState.ClearTracks();
            body.AnimationState.SetAnimation(0, FLY, true);
        }
        GamePlayController.Instance.level.isBirdFlying = true;
        this.transform.DOKill();
        this.transform.position = new Vector3(this.transform.position.x, this.transform.position.y, 0);
        this.transform.parent = GamePlayController.Instance.level.birdParent;

        float distance = 0;
        for (int i = 0; i < path.Length - 1; i++)
        {
            distance += Vector2.Distance(path[i], path[i + 1]);
        }
        float timeMove = distance / (speed != -1 ? speed : speedMove);
        Vector3 remberPos = this.transform.position;
        this.transform.DOPath(path, timeMove, PathType.CatmullRom, PathMode.Ignore).OnComplete(() =>
        {
            state = StateBird.Idle;
            GroundingHandle(posTrans);
            // float timeMove = Vector2.Distance(this.transform.position, posTrans) / speedMove;
            this.transform.DOMove(posTrans, 0.5f).OnComplete(() =>
            {
                if (standing != null)
                    standing.Shake();
                //Debug.Log("Rung cây");
                StopFlySound();
                onGround = true;
                SetLayerPosZ();
            });

            actionCome?.Invoke();
        }).OnUpdate(() =>
        {
            float valueScaleX = Mathf.Abs(body.transform.localScale.x);
            if (remberPos.x <= this.transform.position.x)
            {
                body.transform.localScale = new Vector3(valueScaleX, body.transform.localScale.y, body.transform.localScale.z);
            }
            else
            {
                body.transform.localScale = new Vector3(-valueScaleX, body.transform.localScale.y, body.transform.localScale.z);
            }
            remberPos = this.transform.position;
        }).OnWaypointChange((index) =>
        {
            if (index >= path.Length || index == 0)
                return;
            if (path[index].y <= path[index - 1].y && Vector2.Distance(path[index], path[index - 1]) >= 0.7f)
            {
                //Bay xuống
                PlayAnim(GLIDE, true);
            }
            else
            {
                PlayAnim(FLY, true);
              //  GameController.Instance.musicManager.PlayOneShot(NameMusic.Fly, birdFlyClip, audioFlySource);
            }
        }).SetEase(typeMove);

       // GameController.Instance.musicManager.PlayOneShot(NameMusic.Fly, birdFlyClip, audioFlySource);
    }

    public virtual void FlyOutRandom(BirdBase bird, bool isPlaySoundFly = true)
    {
        if (bird != null)
        {
            if (bird == this)
            {
                this.transform.DOKill();
                return;
            }
        }
        onGround = false;

        //GamePlayController.Instance.returnController.IsLockReturn = true;
        if (body.SkeletonDataAsset != null)
        {
            body.AnimationState.ClearTracks();
            body.AnimationState.SetAnimation(0, FLY, true);
        }
        this.transform.DOKill();
        this.transform.position = new Vector3(this.transform.position.x, this.transform.position.y, 0);
        Vector3 posOut = Helper.GetPointDistanceFromObject(10,
            Helper.GetDirectionFromAngle_2(Vector2.down, UnityEngine.Random.Range(0, 10) % 2 == 0 ? 150 : 230, this.transform.position),
            this.transform.position);
        //// if (body.transform.position.x < 0)
        // posOut = Helper.GetPointDistanceFromObject(10, Helper.GetDirectionFromAngle_2(Vector2.down, 230, this.transform.position), this.transform.position);
        posOut = new Vector3(posOut.x, posOut.y, this.transform.position.z);
        float timeMove = Vector2.Distance(this.transform.position, posOut) / (speedMove * 0.65f);
        this.transform.DOKill();
        this.transform.DOMove(posOut, timeMove).OnComplete(() => { });

        if (isPlaySoundFly)
        //    GameController.Instance.musicManager.PlayOneShot(NameMusic.Fly, birdFlyClip, audioFlySource);

        this.transform.parent = GamePlayController.Instance.level.birdParent;
        state = StateBird.MoveRandom;

        float valueScaleX = Mathf.Abs(body.transform.localScale.x);
        if (posOut.x > this.transform.position.x)
        {
            body.transform.localScale = new Vector3(valueScaleX, body.transform.localScale.y, body.transform.localScale.z);
        }
        else
        {
            body.transform.localScale = new Vector3(-valueScaleX, body.transform.localScale.y, body.transform.localScale.z);
        }


        ChangeLayer(10, LayerGame.BIRD_FLY);
        DisableOutLine();
    }

    public virtual void FlyMerge(Vector3 _posTrans, UnityAction actionCome, float speed = -1, bool isSound = true, bool isSetStateIdle = true)
    {
        Vector3 posTrans = _posTrans;
        if (isSetStateIdle)
            state = StateBird.Flying;
        if (body.SkeletonDataAsset != null)
        {
            body.AnimationState.ClearTracks();
            body.AnimationState.SetAnimation(0, IDLE, true);
        }
        GamePlayController.Instance.level.isBirdFlying = true;
        this.transform.DOKill();
        this.transform.position = new Vector3(this.transform.position.x, this.transform.position.y, 0);
        this.transform.parent = GamePlayController.Instance.level.birdParent;

        // skeletonGhost.enabled = true;
        float timeMove = Vector2.Distance(this.transform.position, posTrans) / (speed != -1 ? speed : speedMove);
        this.transform.DOMove(posTrans, timeMove).OnComplete(() =>
        {
            if (isSetStateIdle)
                state = StateBird.Idle;
            GroundingHandle(posTrans);
            actionCome?.Invoke();
        }).SetEase(Ease.InBack);
    }

    public virtual void GroundingHandle(Vector3 posCome)
    {
        //StartCoroutine(Helper.StartAction(() =>
        //{
        if (body.SkeletonDataAsset != null)
        {
            body.AnimationState.ClearTracks();
            var track = body.AnimationState.SetAnimation(0, GROUNDING, false);
            track.Complete += OnAnimEndDone;
        }
        else
        {
            PlayAnimIdle();
        }
        //}, () => Vector2.Distance(posCome, this.transform.position) <= 0.4f));
    }

    public virtual void GroundingHandle2(Vector3 posCome)
    {
        StartCoroutine(Helper.StartAction(() =>
        {
            body.AnimationState.ClearTracks();
            var track = body.AnimationState.SetAnimation(0, GROUNDING, false);
            track.Complete += OnAnimEndDone;


        }, () => Vector2.Distance(posCome, this.transform.position) <= 0.7f));
    }

    public virtual void OnAnimEndDone(TrackEntry trackEntry)
    {
        if (trackEntry.Animation.Name.Equals(GROUNDING))
        {
            PlayAnimIdle();
        }
    }

    public virtual void PlayAnimIdle()
    {
        if (isDie)
            return;

        int randomIdle = UnityEngine.Random.Range(0, 10);
        if (randomIdle % 2 != 0)
        {
            PlayAnim(IDLE, false, PlayAnimIdle);
        }
        else
        {
            PlayAnim(IDLE_2, false, PlayAnimIdle);
        }
    }

    public virtual void PlayAnim(string nameAnim, bool isLoop, UnityAction actionDone = null)
    {
        if (body.SkeletonDataAsset != null)
        {
            body.AnimationState.ClearTracks();
            var track = body.AnimationState.SetAnimation(0, nameAnim, isLoop);
            track.Complete += (s) => { actionDone?.Invoke(); };
        }
    }
    #endregion

    public void ComeToGadern()
    {
        state = StateBird.Flying;

        var checkPoint = GamePlayController.Instance.level.map.GetCheckPointEmpty2();
        ChangeLayer(checkPoint.indexLayer, LayerGame.BIRD_FLY);
        FlyCome(checkPoint.posMiddle.position, checkPoint.posTrans.position, () =>
        {
            isBirdInGadern = true;
            fsm.lockFSM = false;
            ChangeLayer(checkPoint.indexLayer, LayerGame.BIRD);
            fsm.ChangeState(FSMBird.StateFSM.Idle);
        });
        checkPoint.birdOn = this;
        checkPointIn = checkPoint;
        float timeMove = Vector2.Distance(this.transform.position, checkPoint.posTrans.position) / speedMove;
        scaleBody.transform.DOKill();
        scaleBody.transform.DOScale(1, timeMove * 1.1f);

        GamePlayController.Instance.level.ReturnRequiremnt(this.id);
    }

    //public void OnMouseOver()
    //{
    //   // OnHoverToBird();
    //}

    public void OnHoverToBird()
    {
        if (GamePlayController.Instance.state != StateGame.Playing)
            return;

        if (!GamePlayController.Instance.level.isClickToScreen)
            return;

        if (!isBirdInGadern)
            return;
        if (isDie)
            return;

        HightLight();

        if (GamePlayController.Instance.level.birdChoosing != this && !isLockMove)
        {
            PlayAnim(SELLECT, true);
            fsm.ChangeState(FSMBird.StateFSM.Catching);
            EventDispatcher.EventDispatcher.Instance.PostEvent(EventID.CATCH_BIRD, this);
            GameController.Instance.musicManager.PlayOneShot(sellectClip);
        }

        GamePlayController.Instance.level.birdChoosing = this;
    }

    //public void OnMouseExit()
    //{
    //   // OnNoneSellect();
    //}

    public bool IsCanSellect()
    {
      
        if (!isBirdInGadern)
            return false;
        if (isDie)
            return false;
        if (isLockMove)
            return false;
        if (!GamePlayController.Instance.level.slotController.IsHasSlot())
            return false;

        return true;
    }    

    public void OnChoiceBird()
    {
        if (GamePlayController.Instance.state != StateGame.Playing)
            return;
        
        if (!isBirdInGadern)
            return;
        if (isDie)
            return;

        OnChoiceBirdHandle();
    }

    public virtual void OnChoiceBirdHandle(bool isUseManget = false)
    {
        if (isLockMove)
            return;
        if(birdsDependence.Count > 0)
        {
            int numSlotEmpty = GamePlayController.Instance.level.slotController.GetNumSlotEmpty();
            if (numSlotEmpty <= birdsDependence.Count)
                return;
        }    

        DisableOutLine();
        var slotCome = GamePlayController.Instance.level.slotController.CheckSlot(this);
        if (slotCome != null)
        {
            isBirdInGadern = false;
            if (checkPointIn != null)
                checkPointIn.birdOn = null;
            GamePlayController.Instance.returnController.IsLockReturn = true;
            GamePlayController.Instance.returnController.AddStack(new DataReturn() { slot = slotCome, bird = this });
            fsm.lockFSM = true;

            bool isHasMatch3 = GamePlayController.Instance.level.CheckMatch3Bird(isCheck: true);
            EventDispatcher.EventDispatcher.Instance.PostEvent(EventID.EXIT_BIRD, this);

            GamePlayController.Instance.tutCoreGameController.NextTut();
            GamePlayController.Instance.tutExtralController.NextTut();
            if (GameController.Instance.useProfile.OnVibration)
            {
              //  MMVibrationManager.Haptic(HapticTypes.Selection, false, true, this);
            }

            GamePlayController.Instance.itemController.suggetUseItemInGame.ResetSugget();
            //  GameController.Instance.musicManager.PlayOneShot(pickUpClip);
        }
        else
        {
            fsm.lockFSM = false;
            OnNoneSellect();
        }
    }

    public void OnNoneSellect()
    {
        if (GamePlayController.Instance.state != StateGame.Playing)
            return;

        if (!isBirdInGadern)
            return;
        if (isDie)
            return;

        DisableOutLine();
        if (!isLockMove)
            fsm.ChangeState(FSMBird.StateFSM.FlyCurrentCheckPoint);


        GamePlayController.Instance.level.birdChoosing = null;
        EventDispatcher.EventDispatcher.Instance.PostEvent(EventID.EXIT_BIRD, this);
    }

    protected void OnEventCatchBird(BirdBase bird)
    {
        if (bird != this)
        {
            // this.collider2D.enabled = false;
        }

    }

    protected void OnEventFreeBird(BirdBase bird)
    {
        // this.collider2D.enabled = true;
    }

    public virtual void Update()
    {
        if (timmerCallSound < 1)
        {
            timmerCallSound += Time.deltaTime;
        }

        PlaySoundIdle();
    }

    protected virtual void OnDestroyHandle()
    {
        EventDispatcher.EventDispatcher.Instance.RemoveListener(EventID.DISABLE_OUTLINE_BIRD, actionDisableOutline);
        EventDispatcher.EventDispatcher.Instance.RemoveListener(EventID.FLY_OUT_RANDOM, actionFlyRandom);
        EventDispatcher.EventDispatcher.Instance.RemoveListener(EventID.CATCH_BIRD, onCatchBird);
        EventDispatcher.EventDispatcher.Instance.RemoveListener(EventID.EXIT_BIRD, onExitBird);
        if (flyInitCroutine != null)
            StopCoroutine(flyInitCroutine);
        if (dieCroutine != null)
            StopCoroutine(dieCroutine);
    }

    public virtual void OnDestroy()
    {
        OnDestroyHandle();
    }

    public bool IsOutScreen()
    {
        return !GamePlayController.Instance.level.map.IsCheckPointInLimit(this.transform.position);
    }
}
