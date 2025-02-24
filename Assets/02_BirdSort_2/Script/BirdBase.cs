using DG.DemiLib;

namespace BirdSort2
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using DG.Tweening;
    using UnityEngine.Events;
    using Spine.Unity;
    using Spine;
    using System;
    using Newtonsoft.Json;

    public class BirdBase : MonoBehaviour
    {
        public const string FLY = "fly";
        public const string GROUNDING = "grounding";
        public const string IDLE = "idle";
        public const string IDLE_2 = "idle2";
        public const string PRE_SLEEP = "pre_sleep";
        public const string SLEEP = "sleep";
        public const string WAKE_UP = "wakeup";
        public int id;
        protected bool isFlying;
        [SerializeField] protected SkeletonGraphic body;
        public float speedMove = 5;
        private TrackEntry entryAnim;

        private Action<object> actionDisableOutline;
        public bool onGround;


        [SerializeField] protected Material normalMaterial;
        [SerializeField] protected Material outlineMaterial;
        [SerializeField] protected Material lockOutlineMaterial;

        [SerializeField] protected GameObject outLineObj;
        [SerializeField] protected GameObject outLineCanMoveObj;

        [SerializeField] protected AudioClip birdCallClip;
        [SerializeField] protected AudioClip birdFlyClip;
        [SerializeField] protected AudioClip birdFlyOutClip;
        [SerializeField] protected AudioSource audioFlySource;
        [SerializeField] protected AudioSource audioSourceIdle;
        [SerializeField] protected AudioSource audioCallSource;
        private float timmerCallSound;
        private float timmerSoundIdle;
        private float timmerDelayIdle;
        public bool isCallSoundIdle;

        public List<ExtralPlayBase_BirdSort> extrals;

        public List<ItemChallenge> lsItemChallenges;


        [HideInInspector] public bool isFlyOut;

        protected int m_isLockMove;
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
                }

                if (m_isLockMove <= 0)
                {
                    body.material = normalMaterial;
                    // if (outLineCanMoveObj.transform.localScale.x != 0)
                    // {
                    //     outLineCanMoveObj.transform.localScale = Vector3.zero;
                    //     outLineObj.transform.localScale = Vector3.one;
                    // }
                }
            }
        }

        private Action<object> actionFlyRandom;
        public bool isMoveRandom;


        public bool isInStand;

        public virtual void Init(Vector3 posStart, Vector3 posSpawn, UnityAction actionCome)
        {
            this.transform.DOKill();
            this.transform.position = posStart;
            FlyCome(posSpawn, actionCome);

            actionDisableOutline = (sender) => DisableOutLine();
            EventDispatcher.EventDispatcher.Instance.RegisterListener(EventID.DISABLE_OUTLINE_BIRD, actionDisableOutline);

            timmerCallSound = 1f;
            timmerDelayIdle = UnityEngine.Random.Range(4, 6);

            extrals = new List<ExtralPlayBase_BirdSort>();
            lsItemChallenges = new List<ItemChallenge>();

            actionFlyRandom = (sender) => FlyOutRandom((BirdBase)sender);
            EventDispatcher.EventDispatcher.Instance.RegisterListener(EventID.FLY_OUT_RANDOM, actionFlyRandom);

            SetSkin();
            DisableOutLine();
            //Debug.LogError("sizeDelta " + body.rectTransform.localScale);
            //Debug.LogError("Helper.Rateofscreen  " + Helper.Rateofscreen);
            if (Helper.Rateofscreen >= 2)
            {
                var temp = Helper.Rateofscreen / 1.8f;
                body.rectTransform.localScale *= temp;
            }    
          
        }

        public virtual void FlyOut(UnityAction actionCome, bool isSoundFlyOut = false)
        {
            if (isMoveRandom)
                return;
            //GamePlayController.Instance.returnController.IsLockReturn = true;
            body.AnimationState.ClearTracks();
            body.AnimationState.SetAnimation(0, FLY, true);
            this.transform.DOKill();
            Vector3 posOut = Helper.GetPointDistanceFromObject(10, Helper.GetDirectionFromAngle_2(Vector2.down, 150, this.transform.position), this.transform.position);
            if (body.transform.position.x < 0)
                posOut = Helper.GetPointDistanceFromObject(10, Helper.GetDirectionFromAngle_2(Vector2.down, 230, this.transform.position), this.transform.position);
            posOut = new Vector3(posOut.x, posOut.y, this.transform.position.z);
            float timeMove = Vector2.Distance(this.transform.position, posOut) / speedMove;

            this.transform.DOMove(posOut, timeMove).OnComplete(() => { actionCome?.Invoke(); });

            if (isSoundFlyOut)
                GameController.Instance.musicManager.PlayOneShot(NameMusic.FlyOut, birdFlyOutClip, audioFlySource);

                this.transform.parent = GamePlayController_BirdSort.Instance.level.birdParent;
            isFlyOut = true;
            isInStand = false;
         
        }

        public virtual void FlyCome(Vector3 pos, UnityAction actionCome)
        {
            isFlying = true;
            try
            {
                body.AnimationState.ClearTracks();
            }
            catch (Exception e)
            {
                Debug.LogError(id);
                Debug.LogError(e);
            }
            body.AnimationState.SetAnimation(0, FLY, true);
            GamePlayController_BirdSort.Instance.level.isBirdFlying = true;
            this.transform.DOKill();
            float timeMove = Vector2.Distance(this.transform.position, pos) / speedMove;
            onGround = false;

            this.transform.DOMove(pos, timeMove).OnComplete(() =>
            {
                isFlying = false;
               
                float valueScaleX = Mathf.Abs(body.transform.localScale.x);
                if (body.transform.position.x >= 0)
                {
                    body.transform.localScale = new Vector3(-valueScaleX, body.transform.localScale.y, body.transform.localScale.z);
                }
                else
                {
                    body.transform.localScale = new Vector3(valueScaleX, body.transform.localScale.y, body.transform.localScale.z);
                }
                onGround = true;
                actionCome?.Invoke();
            });
            GroundingHandle2(pos);

            float valueScaleX = Mathf.Abs(body.transform.localScale.x);
            if (pos.x > this.transform.position.x)
            {
                body.transform.localScale = new Vector3(valueScaleX, body.transform.localScale.y, body.transform.localScale.z);
            }
            else
            {
                body.transform.localScale = new Vector3(-valueScaleX, body.transform.localScale.y, body.transform.localScale.z);
            }

            GameController.Instance.musicManager.PlayOneShot(NameMusic.Fly, birdFlyClip, audioFlySource);
            isInStand = false;
        
        }

        public virtual void FlyCome(Transform _posTrans, UnityAction actionCome, StandingBase standing, float upSpeed = 1)
        {
            if (isMoveRandom)
                return;
            isFlyOut = false;

            //  Vector3 posMiddle = _posMiddle;
            Vector3 posTrans = _posTrans.position;

            isFlying = true;
            body.AnimationState.ClearTracks();
            body.AnimationState.SetAnimation(0, FLY, true);
            GamePlayController_BirdSort.Instance.level.isBirdFlying = true;
            this.transform.DOKill();
            onGround = false;
            this.transform.SetParent(_posTrans);

            float timeMove = Vector2.Distance(this.transform.position, posTrans) / speedMove;

            this.transform.DOLocalMove(new Vector3(0, 5, 0), timeMove * upSpeed).OnComplete(() =>
              {
                  isFlying = false;
              
                  actionCome?.Invoke();
              
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
            this.transform.DOLocalMove(new Vector3(0, 0, 0), 0.5f).OnComplete(() =>
                  {
                      onGround = true;
                      standing.Shake();

                

                      StopFlySound();
                  });
              });

            float valueScaleX = Mathf.Abs(body.transform.localScale.x);
            if (posTrans.x > this.transform.position.x)
            {
                body.transform.localScale = new Vector3(valueScaleX, body.transform.localScale.y, body.transform.localScale.z);
            }
            else
            {
                body.transform.localScale = new Vector3(-valueScaleX, body.transform.localScale.y, body.transform.localScale.z);
            }

            GameController.Instance.musicManager.PlayOneShot(NameMusic.Fly, birdFlyClip, audioFlySource);
            isInStand = false;
          
        }

        public virtual void GroundingHandle(Vector3 posCome)
        {
            //StartCoroutine(Helper.StartAction(() =>
            //{
            body.AnimationState.ClearTracks();
            var track = body.AnimationState.SetAnimation(0, GROUNDING, false);
            track.Complete += OnAnimEndDone;
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
                body.AnimationState.ClearTracks();
                entryAnim = body.AnimationState.SetAnimation(0, IDLE, true);
                isInStand = true;
            }
        }
        public virtual void HightLight()
        {
            if (!isLockMove)
            {
                body.material = outlineMaterial;
                // outLineObj.transform.localScale = Vector3.one;
                // outLineCanMoveObj.transform.localScale = Vector3.zero;
            }
            else
            {
                body.material = lockOutlineMaterial;
                // outLineObj.transform.localScale = Vector3.zero;
                // outLineCanMoveObj.transform.localScale = Vector3.one;
            }

            if (timmerCallSound >= 1)
            {
                GameController.Instance.musicManager.PlayOneShot(NameMusic.Fly, birdCallClip, audioCallSource);
                timmerCallSound = 0;
            }

        }

        public virtual void DisableOutLine()
        {
            body.material = normalMaterial;
            // outLineObj.transform.localScale = Vector3.zero;
            // outLineCanMoveObj.transform.localScale = Vector3.zero;
        }

        public virtual void OnDestroy()
        {
            EventDispatcher.EventDispatcher.Instance.RemoveListener(EventID.DISABLE_OUTLINE_BIRD, actionDisableOutline);
            EventDispatcher.EventDispatcher.Instance.RemoveListener(EventID.FLY_OUT_RANDOM, actionFlyRandom);
        }

        public virtual void Update()
        {
            if (timmerCallSound < 1)
            {
                timmerCallSound += Time.deltaTime;
            }

            PlaySoundIdle();
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
                GameController.Instance.musicManager.PlayOneShot(NameMusic.Idle, birdCallClip, audioSourceIdle);
                timmerDelayIdle = UnityEngine.Random.Range(5, 7);
            }
        }

        public virtual void DisableBody()
        {
            body.gameObject.SetActive(false);
        }

        public virtual void ActiveBody()
        {
            body.gameObject.SetActive(true);
        }

        public virtual void FlyOutRandom(BirdBase bird)
        {
            if (bird != null)
            {
                if (bird == this)
                {
                    isMoveRandom = true;
                    this.transform.DOKill();
                    return;
                }

            }

            //GamePlayController.Instance.returnController.IsLockReturn = true;
            body.AnimationState.ClearTracks();
            body.AnimationState.SetAnimation(0, FLY, true);
            this.transform.DOKill();
            Vector3 posOut = Helper.GetPointDistanceFromObject(10,
                Helper.GetDirectionFromAngle_2(Vector2.down, UnityEngine.Random.Range(0, 10) % 2 == 0 ? 150 : 230, this.transform.position),
                this.transform.position);
            //// if (body.transform.position.x < 0)
            // posOut = Helper.GetPointDistanceFromObject(10, Helper.GetDirectionFromAngle_2(Vector2.down, 230, this.transform.position), this.transform.position);
            posOut = new Vector3(posOut.x, posOut.y, this.transform.position.z);
            float timeMove = Vector2.Distance(this.transform.position, posOut) / (speedMove * 1.5f);
            this.transform.DOKill();
            this.transform.DOMove(posOut, timeMove).OnComplete(() => { });

            GameController.Instance.musicManager.PlayOneShot(NameMusic.FlyOut, birdFlyOutClip, audioFlySource);

            this.transform.parent = GamePlayController_BirdSort.Instance.level.birdParent;
            isFlyOut = true;
            isMoveRandom = true;

            float valueScaleX = Mathf.Abs(body.transform.localScale.x);
            if (posOut.x > this.transform.position.x)
            {
                body.transform.localScale = new Vector3(valueScaleX, body.transform.localScale.y, body.transform.localScale.z);
            }
            else
            {
                body.transform.localScale = new Vector3(-valueScaleX, body.transform.localScale.y, body.transform.localScale.z);
            }

            isInStand = false;
        }

        public virtual void PlayAnim(string nameAnim, bool isLoop, UnityAction actionDone = null)
        {
            body.AnimationState.ClearTracks();
            var track = body.AnimationState.SetAnimation(0, nameAnim, isLoop);
            track.Complete += (s) => { actionDone?.Invoke(); };
        }

        public void SetSkin()
        {
            //try
            //{
            //    string skinName = "skin-1";
            //    var skins = GamePlayController.Instance.skinController.listSkin;
            //    for (int i = 0; i < skins.Count; i++)
            //    {
            //        int index = i;
            //        if (skins[index].idBird == id + 1)
            //        {
            //            skinName = "skin-" + (skins[index].idSkin + 1);
            //            //switch (skins[i].idSkin)
            //            //{
            //            //    case 0:
            //            //        skinName = "skin-1";
            //            //        break;
            //            //    case 1:
            //            //        skinName = "skin-2";
            //            //        break;
            //            //    case 2:
            //            //        skinName = "skin-3";
            //            //        break;
            //            //    case 3:
            //            //        skinName = "skin-4";
            //            //        break;
            //            //    case 4:
            //            //        skinName = "skin-5";
            //            //        break;
            //            //    case 5:
            //            //        skinName = "skin-6";
            //            //        break;
            //            //    case 6:
            //            //        skinName = "skin-7";
            //            //        break;
            //            //    case 7:
            //            //        skinName = "skin-8";
            //            //        break;
            //            //}
            //        }
            //    }
            //    Skin newSkin = new Skin("new_skin"); // 1. Create a new empty skin
            //    newSkin.AddSkin(body.SkeletonData.FindSkin(skinName)); // 2. Add items
            //    body.Skeleton.SetSkin(newSkin); // 1. Set the active skin
            //    body.Skeleton.SetSlotsToSetupPose();
            //}
            //catch
            //{

            //}
        }
        public void SetSkinWithName(string skinName)
        {
            Skin newSkin = new Skin("new_skin"); // 1. Create a new empty skin
            newSkin.AddSkin(body.SkeletonData.FindSkin(skinName)); // 2. Add items
            body.Skeleton.SetSkin(newSkin); // 1. Set the active skin
            body.Skeleton.SetSlotsToSetupPose();
        }

  

        public void HandleOnActionSuggetBooster()
        {
           // body.Skeleton.SetToSetupPose();
            //body.AnimationState.ClearTracks();
            //body.AnimationState.SetAnimation(0, IDLE_2, true);
            HightLight();

        }

        public void HandleOffActionSuggetBooster()
        {
         //   body.Skeleton.SetToSetupPose();
            //body.AnimationState.ClearTracks();
            //body.AnimationState.SetAnimation(0, IDLE, true);
            DisableOutLine();

        }

        public void HandleClaimItemChallenge()
        {
          if(lsItemChallenges.Count > 0)
            {
                foreach(var item in lsItemChallenges)
                {
                    item.Claimp();
                }
            }

        }    


    }
}