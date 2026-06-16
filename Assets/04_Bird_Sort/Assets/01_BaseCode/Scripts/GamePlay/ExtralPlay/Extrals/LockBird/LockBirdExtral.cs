namespace BirdSort2
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    using DG.Tweening;
    using Spine.Unity;

    public class LockBirdExtral : ExtralPlayBase_BirdSort
    {
        private int idBirdHasKey;
        [SerializeField] private GameObject key;
        public BirdBase birdHasKey;
        [SerializeField] private SkeletonGraphic skeGrade;
        [SerializeField] private Transform posLock;

        [SerializeField] protected AudioClip cageLockClip;
        [SerializeField] protected AudioClip cageOpenClip;
        [SerializeField] protected AudioSource audioCageSource;

        [SerializeField] protected AudioClip keyAppearClip;
        [SerializeField] protected AudioClip getKeyClip;
        [SerializeField] protected AudioSource keySource;
        public override void Init(ExtralPlayData data, int id)
        {
            base.Init(data, id);
            data.birdParent.isLockMove = true;
            idBirdHasKey = (int)data.extralValues[0];

            if (data.extralValues.Length > 1)
            {
                int idBirdKey = (int)data.extralValues[1];
                int idStandKey = (int)data.extralValues[2];

                //Hiện chìa khoá ở chân chim idBirdHasKey
                var stand = GamePlayController_BirdSort.Instance.level.standsInLevel_Real[idStandKey];

                birdHasKey = stand.birdsOnArray[idBirdKey];
                key.transform.parent = birdHasKey.transform;
                key.transform.localPosition = new Vector3(0, -1.5f, 0);
            }
            else
            {
                //Hiện chìa khoá ở chân chim idBirdHasKey
                var birdsInGame = GameObject.FindObjectsOfType<BirdBase>();
                if (birdsInGame != null)
                {
                    for (int i = 0; i < birdsInGame.Length; i++)
                    {
                        if (birdsInGame[i].id != idBirdHasKey)
                            continue;

                        birdHasKey = birdsInGame[i];
                        key.transform.parent = birdHasKey.transform;
                        key.transform.localPosition = new Vector3(0, -1.5f, 0);
                        break;
                    }
                }
            }

            body.gameObject.SetActive(false);
            key.gameObject.SetActive(false);
        }

        public override void ReAppearExtral(int idBird, int idStand)
        {

        }

        protected override void CheckExtralHandle(BirdBase bird, StandingBase stand, bool isFlyOut)
        {
            if (isFlyOut)
            {
                if (bird.id == idBirdHasKey)
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
            yield return new WaitUntil(() => birdHasKey.isFlyOut);
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
                skeGrade.AnimationState.ClearTracks();
                GameController.Instance.musicManager.PlayOneShot(cageLockClip, audioCageSource);
                var track = skeGrade.AnimationState.SetAnimation(0, "lock", false);
                track.Complete += (s) =>
                {
                    key.gameObject.SetActive(true);
                    key.transform.parent = GamePlayController_BirdSort.Instance.level.birdParent;
                    key.transform.SetAsLastSibling();
                    key.transform.localPosition = new Vector3(0, 400, 0);
                    key.transform.DOKill();
                    key.transform.localScale = Vector3.zero;

                    key.transform.DOScale(3f * Vector3.one, 0.3f).OnComplete(() =>
                    {
                        float timeMove = 0.45f;
                    //float timeMove = Vector2.Distance(birdHasKey.transform.position, key.transform.position) / 3.5f;
                    key.transform.DOMove(birdHasKey.transform.position, timeMove).OnComplete(() =>
                        {
                            GameController.Instance.musicManager.PlayOneShot(keyAppearClip, keySource);
                            key.transform.parent = birdHasKey.transform;
                        
                                GamePlayController_BirdSort.Instance.tutorialController.ActiveTutExtral(this);
                                GamePlayController_BirdSort.Instance.tutorialController.ActiveHintLockBird();
                                GamePlayController_BirdSort.Instance.extralPlayController.InitDestroyExtralButton(data.type, this);
                            


                            isReady = true;
                            GamePlayController_BirdSort.Instance.returnController.IsLockReturn = false;
                        }).SetDelay(0.3f);
                        key.transform.DOScale(1f, timeMove * 0.95f).SetDelay(0.3f);
                        GameController.Instance.musicManager.PlayOneShot(keyAppearClip, keySource);
                    }).SetEase(Ease.OutBack);
                };

            }, () => this.data.birdParent.onGround));
        }

        protected override void PlayAnimEnd()
        {
            //=> anim chìa khoá bay mở cửa
            Debug.Log("=============== Boom Done ");
            key.transform.parent = GamePlayController_BirdSort.Instance.level.birdParent;

            key.gameObject.SetActive(true);
            key.transform.DOKill();
            key.transform.localScale = Vector3.one;
            key.transform.DOScale(1.5f, 0.35f).OnComplete(() =>
            {
                //key.transform.DOScale(0.8f, 0.3f);
                GameController.Instance.musicManager.PlayOneShot(getKeyClip, keySource);
            });

            float timeMoveKey = Vector2.Distance(key.transform.position, posLock.transform.position) / 4.5f;
            // float timeMoveKey = 0.25f;
            key.transform.DOMove(posLock.position, timeMoveKey).OnComplete(() =>
            {
                skeGrade.AnimationState.ClearTracks();
                GameController.Instance.musicManager.PlayOneShot(cageOpenClip, audioCageSource);
                var track = skeGrade.AnimationState.SetAnimation(0, "lock3", false);
                track.Complete += (s) =>
                {
                    body.gameObject.SetActive(false);
                    data.birdParent.isLockMove = false;
                };
                this.key.SetActive(false);

            });
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
            //this.key.SetActive(false);
            //this.key.transform.DOKill();
        }
        public override void ResetExtral()
        {
            Destroy(key.gameObject);
            Destroy(this.gameObject);
        }
    }
}