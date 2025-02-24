namespace BirdSort2
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    using DG.Tweening;
    using Spine.Unity;

    public class BoomExtral_BirdSort : ExtralPlayBase_BirdSort
    {
        protected float timeCoolDown;
        [SerializeField] private Text coolDownTxt;
        [SerializeField] private SkeletonGraphic boomSke;
        [SerializeField] private GameObject coolDownObj;

        [SerializeField] protected AudioClip beepClip;
        [SerializeField] protected AudioClip explosionClip;
        [SerializeField] protected AudioSource audioBoomSource;

        [SerializeField] protected GameObject fxFuse;
        [SerializeField] protected GameObject fxExplosion;

        public override void Init(ExtralPlayData data, int id)
        {
            base.Init(data, id);

            timeCoolDown = this.data.extralValues[0];
            coolDownTxt.text = timeCoolDown.ToString();
            coolDownObj.transform.DOKill();
            coolDownObj.transform.localScale = Vector3.zero;

            body.gameObject.SetActive(false);

            StartCoroutine(Helper.StartAction(() =>
            {
                coolDownObj.transform.DOScale(1.2f, 0.25f).SetLoops(-1, LoopType.Yoyo); ;
                coolDownTxt.DOColor(new Color(1, 0, 0, 1), 0.25f).SetLoops(-1, LoopType.Yoyo);
                body.transform.DOKill();
                body.transform.DOScale(1.1f, 0.35f).SetLoops(-1, LoopType.Yoyo);

            }, () => timeCoolDown <= 5));

            fxFuse.gameObject.SetActive(false);
            fxExplosion.gameObject.SetActive(false);

        }

        protected override void CheckExtralHandle(BirdBase bird, StandingBase stand, bool isFlyOut)
        {
            if (!isFlyOut)
            {
                ActiveCoolDown();
            }
            else
            {
                if (bird != null && data.birdParent.id == bird.id)
                {
                    //Phá boom thành công, hiển thị effect phá boom
                    state = StateExtral.Success;

                    if (m_ExtralSuccess != null)
                        StopCoroutine(m_ExtralSuccess);
                    m_ExtralSuccess = ExtralSuccessHandle();
                    StartCoroutine(m_ExtralSuccess);
                }
                else
                {
                    ActiveCoolDown();
                }
            }
        }

        private void ActiveCoolDown()
        {
            timeCoolDown--;
            GameController.Instance.musicManager.PlayOneShot(beepClip, audioBoomSource);

            if (timeCoolDown <= 0)
            {
                timeCoolDown = 0;
                EndExtral(false);
            }

            coolDownTxt.text = timeCoolDown.ToString();
        }

        private IEnumerator ExtralSuccessHandle()
        {
            // this.transform.parent = GamePlayController.Instance.level.birdParent;
            yield return new WaitUntil(() => data.birdParent.isFlyOut);
            PlayAnimEnd();
        }

        protected override void ReturnExtralHandle()
        {
            timeCoolDown++;
            if (timeCoolDown > this.data.extralValues[0])
                timeCoolDown = this.data.extralValues[0];
            coolDownTxt.text = timeCoolDown.ToString();
            //state = StateExtral.Life;
        }

        public override void EndExtral(bool isDone)
        {
            base.EndExtral(isDone);
            if (!isDone)
            {
                //Gọi hiệu ứng nổ, show popup lose
                Debug.Log("============= Boom Lose");
                fxFuse.gameObject.SetActive(false);
                fxExplosion.gameObject.SetActive(true);

                state = StateExtral.Fail;
                GamePlayController_BirdSort.Instance.state = StateGame.Lose;
                this.data.birdParent.DisableBody();
                body.gameObject.SetActive(false);
                GameController.Instance.musicManager.PlayOneShot(explosionClip, audioBoomSource);
                EventDispatcher.EventDispatcher.Instance.PostEvent(EventID.FLY_OUT_RANDOM, this.data.birdParent);
         
                    StartCoroutine(Helper.StartAction(() =>
                    {
                        LoseBox.SetupLegacy().Show();
                       
                    }, 2f));
            
            }
            else
            {

            }

        }



        public override void ReAppearExtral(int idBird, int idStand)
        {
            //StopAllCoroutines();
            //coolDownTxt.text = timeCoolDown.ToString();
            //state = StateExtral.Life;
            //PlayAnimAppear();
            //if (m_ExtralSuccess != null)
            //    StopCoroutine(m_ExtralSuccess);
            //PlayAnimAppear();
        }

        protected override void PlayAnimAppear()
        {
            base.PlayAnimAppear();

           
            StartCoroutine(Helper.StartAction(() =>
            {
                body.gameObject.SetActive(true);
                this.transform.parent = GamePlayController_BirdSort.Instance.level.birdParent;
                this.transform.SetAsLastSibling();
                this.transform.localPosition = new Vector3(0, 0, 0);
                this.transform.DOKill();
                this.transform.localScale = 3 * Vector3.one;

                GameController.Instance.musicManager.PlayOneShot(beepClip, audioBoomSource);
                boomSke.AnimationState.ClearTracks();

                var track = boomSke.AnimationState.SetAnimation(0, "start", false);
                track.Complete += (s) =>
                {
                    boomSke.AnimationState.SetAnimation(0, "idle", true);
                    body.transform.DOKill();
                    body.transform.DOScale(1.1f, 0.7f).SetLoops(-1, LoopType.Yoyo);
                    fxFuse.gameObject.SetActive(true);
                    coolDownObj.transform.DOScale(1, 0.05f);

                // float timeMove = Vector2.Distance(this.data.birdParent.transform.position, body.transform.position) / 3.5f;
                float timeMove = 0.25f;
                    this.transform.DOMove(this.data.birdParent.transform.position, timeMove).OnComplete(() =>
                    {


                            GamePlayController_BirdSort.Instance.tutorialController.ActiveTutExtral(this);
                            GamePlayController_BirdSort.Instance.extralPlayController.InitDestroyExtralButton(data.type, this);
                      

                        this.transform.parent = this.data.birdParent.transform;
                        this.transform.localPosition = Vector3.zero;
                        isReady = true;

                        GamePlayController_BirdSort.Instance.returnController.IsLockReturn = false;
                    }).SetDelay(0.05f);
                    this.transform.DOScale(1, timeMove * 0.95f).SetDelay(0.05f);
                };

            }, () => this.data.birdParent.onGround));
        }

        protected override void PlayAnimEnd()
        {
            Debug.Log("=============== Boom Done ");
            coolDownObj.gameObject.SetActive(false);
            fxFuse.gameObject.SetActive(false);
            body.transform.DOKill();
            body.transform.localScale = Vector3.one;
            boomSke.AnimationState.ClearTracks();
            var track = boomSke.AnimationState.SetAnimation(0, "fall", false);
            track.TimeScale = 1f;
            track.Complete += (s) =>
            {
                body.gameObject.SetActive(false);

            };
            base.PlayAnimEnd();

        }

        protected override void OnUpdate()
        {
            base.OnUpdate();
            if (Input.GetMouseButtonDown(0))
                GamePlayController_BirdSort.Instance.tutorialController.DeActiveTutExtral(true);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            this.transform.DOKill();
        }
        public override void ResetExtral()
        {
            Destroy(this.gameObject);
        }
    }
}