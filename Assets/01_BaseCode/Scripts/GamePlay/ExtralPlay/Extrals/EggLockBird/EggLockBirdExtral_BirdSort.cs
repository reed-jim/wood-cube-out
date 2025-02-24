namespace BirdSort2
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    using DG.Tweening;
    using Spine.Unity;
    using Spine;

    public class EggLockBirdExtral_BirdSort : ExtralPlayBase_BirdSort
    {
        private int idBirdHasKey;
        [SerializeField] private GameObject hammer;

        [SerializeField] private Image hammerImg;
        [SerializeField] private Sprite[] hammerIcon;

        public BirdBase birdHasKey;
        [SerializeField] private SkeletonGraphic skeEgg;
        [SerializeField] private Transform posLock;

        [SerializeField] protected AudioClip eggLockClip;
        [SerializeField] protected AudioClip eggOpenClip;
        [SerializeField] protected AudioSource audioCageSource;

        [SerializeField] protected AudioClip hammerAppearClip;
        [SerializeField] protected AudioClip getHammerClip;
        [SerializeField] protected AudioSource hammerSource;
        [SerializeField] private GameObject fxEggDestroy;

        private int remeberIDBirdParent;

        public override void Init(ExtralPlayData data, int id)
        {
            base.Init(data, id);
            data.birdParent.isLockMove = true;
            data.birdParent.DisableBody();
            remeberIDBirdParent = data.birdParent.id;
            data.birdParent.id = -100;
            data.birdParent.isInStand = true;
            idBirdHasKey = (int)data.extralValues[0];
            hammerImg.sprite = hammerIcon[(int)data.extralValues[1]];
            // skeGrade.Skeleton.Skin.

            Skin newSkin = new Skin("new-skin"); // 1. Create a new empty skin
            newSkin.AddSkin(skeEgg.SkeletonData.FindSkin(string.Format("skin-{0}", (int)data.extralValues[1] + 1))); // 2. Add items
            skeEgg.Skeleton.SetSkin(newSkin); // 1. Set the active skin

            if (data.extralValues.Length > 2)
            {
                int idStandKey = (int)data.extralValues[2];

                //Hiện chìa khoá ở chân chim idBirdHasKey
                var stand = GamePlayController_BirdSort.Instance.level.standsInLevel_Real[idStandKey];

                birdHasKey = stand.birdsOnArray[idBirdHasKey];
                hammer.transform.parent = birdHasKey.transform;
                hammer.transform.localPosition = new Vector3(0, -1.5f, 0);
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
                        hammer.transform.parent = birdHasKey.transform;
                        hammer.transform.localPosition = new Vector3(0, -1.5f, 0);
                        break;
                    }
                }
            }

            body.gameObject.SetActive(false);
            hammer.gameObject.SetActive(false);
            fxEggDestroy.gameObject.SetActive(false);
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
                skeEgg.AnimationState.ClearTracks();
                GameController.Instance.musicManager.PlayOneShot(eggLockClip, audioCageSource);
                var track = skeEgg.AnimationState.SetAnimation(0, "1-fall", false);
                track.Complete += (s) =>
                {
                    hammer.gameObject.SetActive(true);
                    hammer.transform.parent = GamePlayController_BirdSort.Instance.level.extralShowParent;
                    hammer.transform.SetAsLastSibling();
                // hammer.transform.localPosition = new Vector3(0, 400, 0);
                hammer.transform.DOKill();
                    hammer.transform.localScale = Vector3.zero;

                    hammer.transform.DOScale(3f * Vector3.one, 0.3f).OnComplete(() =>
                    {
                        float timeMove = 0.45f;
                    //float timeMove = Vector2.Distance(birdHasKey.transform.position, key.transform.position) / 3.5f;
                    hammer.transform.DOMove(birdHasKey.transform.position, timeMove).OnComplete(() =>
                        {
                            GameController.Instance.musicManager.PlayOneShot(hammerAppearClip, hammerSource);
                            hammer.transform.parent = birdHasKey.transform;
                            GamePlayController_BirdSort.Instance.tutorialController.ActiveTutExtral(this);
                                GamePlayController_BirdSort.Instance.tutorialController.ActiveHintLockBird();
                                GamePlayController_BirdSort.Instance.extralPlayController.InitDestroyExtralButton(data.type, this);
                                isReady = true;
                            GamePlayController_BirdSort.Instance.returnController.IsLockReturn = false;
                        }).SetDelay(0.3f);
                        hammer.transform.DOScale(1f, timeMove * 0.95f).SetDelay(0.3f);
                        GameController.Instance.musicManager.PlayOneShot(hammerAppearClip, hammerSource);
                    }).SetEase(Ease.OutBack);

                    skeEgg.AnimationState.SetAnimation(0, "2-idle", true);
                };

            }, () => this.data.birdParent.onGround));
        }

        protected override void PlayAnimEnd()
        {
            //=> anim chìa khoá bay mở cửa
            hammer.transform.parent = GamePlayController_BirdSort.Instance.level.birdParent;

            hammer.gameObject.SetActive(true);
            hammer.transform.DOKill();
            hammer.transform.localScale = Vector3.one;
            hammer.transform.DOScale(1.5f, 0.35f).OnComplete(() =>
            {
                //key.transform.DOScale(0.8f, 0.3f);
                GameController.Instance.musicManager.PlayOneShot(getHammerClip, hammerSource);
            });

            float timeMoveKey = Vector2.Distance(hammer.transform.position, posLock.transform.position) / 4.5f;
            // float timeMoveKey = 0.25f;
            hammer.transform.DORotate(new Vector3(0, 0, 180), 0.15f).SetLoops(-1, LoopType.Incremental).SetEase(Ease.Linear);
            hammer.transform.DOMove(posLock.position, timeMoveKey).OnComplete(() =>
            {
                skeEgg.AnimationState.ClearTracks();
                data.birdParent.ActiveBody();
                GameController.Instance.musicManager.PlayOneShot(eggOpenClip, audioCageSource);
                var track = skeEgg.AnimationState.SetAnimation(0, "3-break", false);
                track.Complete += (s) =>
                {
                    body.gameObject.SetActive(false);
                    data.birdParent.id = remeberIDBirdParent;
                    data.birdParent.isLockMove = false;
                };
                hammer.transform.DOKill();
                hammer.transform.localEulerAngles = new Vector3(0, 0, 0.703f);
                StartCoroutine(Helper.StartAction(() => { this.hammer.SetActive(false); }, 0.2f));
                fxEggDestroy.gameObject.SetActive(true);
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
            this.hammer.SetActive(false);
            this.hammer.transform.DOKill();
        }
        public override void ResetExtral()
        {
            Destroy(this.gameObject);
        }
    }
}