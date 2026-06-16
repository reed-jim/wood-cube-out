namespace BirdSort2
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    using DG.Tweening;
    using Spine.Unity;
    using Spine;

    public class SleepBirdExtral : ExtralPlayBase_BirdSort
    {
        private int idBirdHasClock;

        private BirdBase birdHasClock;
        [SerializeField] private SkeletonGraphic clockSke;

        [SerializeField] protected AudioClip clockAppearClip;
        [SerializeField] protected AudioClip clockArlamClip;
        [SerializeField] protected AudioSource clockAudioSource;

        [SerializeField] protected AudioClip sleepAudio;
        [SerializeField] protected AudioClip wakeupAudio;
        [SerializeField] protected AudioSource birdSleepSource;

        public List<BirdBase> birdsSleep;
        public List<GameObject> ZsleepLst;
        public GameObject zSleepPrefab;

        public override void Init(ExtralPlayData data, int id)
        {
            base.Init(data, id);

            idBirdHasClock = data.birdParent.id;
            birdHasClock = data.birdParent;

            //Hiện chìa khoá ở chân chim idBirdHasKey
            clockSke.transform.parent = birdHasClock.transform;
            clockSke.transform.localPosition = new Vector3(1, -45f, 0);

            ZsleepLst = new List<GameObject>();
            birdsSleep = new List<BirdBase>();
            for (int i = 0; i < data.extralValues.Length; i += 2)
            {
                int idBird = (int)data.extralValues[i];
                int idStand = (int)data.extralValues[i + 1];

                var stand_has = GamePlayController_BirdSort.Instance.level.standsInLevel_Real[idStand];
                var birdSleep = stand_has.birdsOnArray[idBird];
                birdSleep.extrals.Add(this);
                birdSleep.isLockMove = true;

                var zSleep = Instantiate(zSleepPrefab, birdSleep.gameObject.transform);
                zSleep.transform.localPosition = Vector3.zero;
                zSleep.gameObject.SetActive(false);

                ZsleepLst.Add(zSleep);
                birdsSleep.Add(birdSleep);
            }


            body.gameObject.SetActive(false);
            clockSke.gameObject.SetActive(false);
        }

        public override void ReAppearExtral(int idBird, int idStand)
        {

        }

        protected override void CheckExtralHandle(BirdBase bird, StandingBase stand, bool isFlyOut)
        {
            if (isFlyOut)
            {
                if (bird.id == idBirdHasClock)
                {
                    state = StateExtral.Success;
                    if (m_ExtralSuccess != null)
                        StopCoroutine(m_ExtralSuccess);
                    m_ExtralSuccess = ExtralSuccessHandle(bird);
                    StartCoroutine(m_ExtralSuccess);

                    GamePlayController_BirdSort.Instance.tutorialController.DoneTutExtral();
                }
            }
        }

        private IEnumerator ExtralSuccessHandle(BirdBase bird)
        {
            yield return new WaitUntil(() => birdHasClock.isFlyOut);
            PlayAnimEnd();
            //Post Event check lại StandSuccess cả map
        }

        protected override void ReturnExtralHandle()
        {

        }

        protected override void PlayAnimAppear()
        {
            base.PlayAnimAppear();
            StartCoroutine(Helper.StartAction(() =>
            {
                body.gameObject.SetActive(true);
                clockSke.AnimationState.ClearTracks();
                clockSke.AnimationState.SetAnimation(0, "2-idle", true);

                clockSke.gameObject.SetActive(true);
                clockSke.transform.parent = GamePlayController_BirdSort.Instance.level.birdParent;
                clockSke.transform.SetAsLastSibling();
                clockSke.transform.localPosition = new Vector3(0, 400, 0);
                clockSke.transform.DOKill();
                clockSke.transform.localScale = Vector3.zero;

                clockSke.transform.DOScale(3f * Vector3.one, 0.3f).OnComplete(() =>
                    {
                        float timeMove = 0.45f;
                    //float timeMove = Vector2.Distance(birdHasKey.transform.position, key.transform.position) / 3.5f;
                    clockSke.transform.DOMove(birdHasClock.transform.position - new Vector3(0, 0.3f, 0), timeMove).OnComplete(() =>
                        {
                            clockSke.transform.parent = birdHasClock.transform;
                        //clockSke.transform.localPosition = new Vector3(1, -45f, 0);
                   
                                GamePlayController_BirdSort.Instance.tutorialController.ActiveTutExtral(this);
                                GamePlayController_BirdSort.Instance.tutorialController.ActiveHintLockBird();
                                GamePlayController_BirdSort.Instance.extralPlayController.InitDestroyExtralButton(data.type, this);
                            


                            isReady = true;
                            GamePlayController_BirdSort.Instance.returnController.IsLockReturn = false;
                        }).SetDelay(0.3f);
                        clockSke.transform.DOScale(1.3f, timeMove * 0.95f).SetDelay(0.3f);
                        GameController.Instance.musicManager.PlayOneShot(clockAppearClip, clockAudioSource);
                    }).SetEase(Ease.OutBack);

                for (int i = 0; i < birdsSleep.Count; i++)
                {
                    int index = i;
                    birdsSleep[index].PlayAnim(BirdBase.PRE_SLEEP, false, () =>
                    {
                        birdsSleep[index].PlayAnim(BirdBase.SLEEP, true, delegate { birdsSleep[index].isInStand = true; });

                    });
                    ZsleepLst[index].gameObject.SetActive(true);
                    if (index == birdsSleep.Count - 1)
                        GameController.Instance.musicManager.PlayOneShot(sleepAudio, birdSleepSource, volume: 0.4f);

                }

            }, () => this.data.birdParent.isInStand));
        }

        protected override void PlayAnimEnd()
        {
            //=> anim chìa khoá bay mở cửa
            clockSke.gameObject.SetActive(true);
            clockSke.transform.parent = GamePlayController_BirdSort.Instance.level.birdParent;
            clockSke.transform.SetAsLastSibling();
            clockSke.transform.DOKill();
            clockSke.transform.DOLocalMove(new Vector3(0, 400, 0), 0.3f).OnComplete(() =>
            {
                //key.transform.DOScale(0.8f, 0.3f);
                GameController.Instance.musicManager.PlayOneShot(clockArlamClip, clockAudioSource);

                clockSke.AnimationState.ClearTracks();
                var track = clockSke.AnimationState.SetAnimation(0, "3-alarm", true);


                for (int i = 0; i < birdsSleep.Count; i++)
                {
                    int index = i;
                    birdsSleep[i].PlayAnim(BirdBase.WAKE_UP, false, () =>
                    {
                        birdsSleep[index].isLockMove = false;
                        birdsSleep[index].PlayAnim(BirdBase.IDLE, true);
                    });

                    ZsleepLst[index].gameObject.SetActive(false);

                    if (index == birdsSleep.Count - 1)
                        GameController.Instance.musicManager.PlayOneShot(wakeupAudio, birdSleepSource);
                }

            //Chuyển anim bird sang Idle

            StartCoroutine(Helper.StartAction(() =>
                {
                    clockSke.transform.DOScale(0, 0.3f).OnComplete(() =>
                    {
                        this.clockSke.gameObject.SetActive(false);
                        body.gameObject.SetActive(false);
                    });

                }, 2));
            });
            clockSke.transform.localScale = Vector3.one;
            clockSke.transform.DOScale(3f * Vector3.one, 0.3f);
            base.PlayAnimEnd();
        }

        public override void DestroyExtrals()
        {
            base.DestroyExtrals();
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
        }
        public override void ResetExtral()
        {
            for (int i = 0; i < birdsSleep.Count; i++)
            {
                int index = i;
                birdsSleep[index].isLockMove = false;
                birdsSleep[index].PlayAnim(BirdBase.IDLE, true);
            }
            foreach (GameObject param in ZsleepLst)
            {
                Destroy(param.gameObject);
            }
            ZsleepLst.Clear();
            Destroy(clockSke.gameObject);
            Destroy(this.gameObject);
        }
    }

}