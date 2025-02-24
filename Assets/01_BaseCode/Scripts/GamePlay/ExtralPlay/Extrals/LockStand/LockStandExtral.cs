namespace BirdSort2
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    using DG.Tweening;
    using Spine.Unity;
    using Spine;

    public class LockStandExtral : ExtralPlayBase_BirdSort
    {
        public class KeyElement
        {
            public int id;
            public GameObject key;
            public BirdBase birdHasKey;
            public SkeletonGraphic lockObj;
        }


        [SerializeField] private List<GameObject> keysPrefab;

        [SerializeField] private SkeletonGraphic locksPrefab;

        [SerializeField] private List<KeyElement> keyElements;

        [SerializeField] protected AudioClip cageLockClip;
        [SerializeField] protected AudioClip cageOpenClip;
        [SerializeField] protected AudioSource audioCageSource;

        [SerializeField] protected AudioClip keyAppearClip;
        [SerializeField] protected AudioClip getKeyClip;
        [SerializeField] protected AudioSource keySource;

        private int totalLocked;
        private int numLocked;

        private BirdBase finalBirdHasKey;

        [SerializeField] protected PrisonLockStand prison_4;
        [SerializeField] protected PrisonLockStand prison_5;
        [SerializeField] protected PrisonLockStand prison_6;

        protected PrisonLockStand currentPrison;

        public override void Init(ExtralPlayData data, int id)
        {
            base.Init(data, id);
            this.transform.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

            //  data.standParent.isLockStand = true;

            keyElements = new List<KeyElement>();

            TypeStand typeStandInLevel = GamePlayController_BirdSort.Instance.level.typeStandInLevel;
            if (typeStandInLevel == TypeStand.Slot_4)
            {
                currentPrison = prison_4;
            }
            else if (typeStandInLevel == TypeStand.Slot_5)
            {
                currentPrison = prison_5;
            }
            else if (typeStandInLevel == TypeStand.Slot_6)
            {
                currentPrison = prison_6;
            }


            int idkey = 0;
            if (data.extralValues.Length > 1)
            {
                for (int i = 0; i < data.extralValues.Length; i += 2)
                {
                    int idBirdKey = (int)data.extralValues[i];
                    int idStandKey = (int)data.extralValues[i + 1];

                    var stand = GamePlayController_BirdSort.Instance.level.standsInLevel_Real[idStandKey];

                    var birdHasKey = stand.birdsOnArray[idBirdKey];
                    var key = Instantiate(keysPrefab[idkey], birdHasKey.transform);
                    key.transform.localPosition = new Vector3(0, -1.5f, 0);
                    key.gameObject.SetActive(false);

                    var lock_ = Instantiate(locksPrefab, currentPrison.parentLock.transform);

                    SkeletonGraphic lockObj = lock_.GetComponent<SkeletonGraphic>();
                    //lockObj.transform.position = lst_posLock[idkey].position;
                    Skin newSkin = new Skin("new-skin"); // 1. Create a new empty skin

                    newSkin.AddSkin(lockObj.SkeletonData.FindSkin(string.Format("skin-{0}", idkey + 1))); // 2. Add items
                    lockObj.Skeleton.SetSkin(newSkin); // 1. Set the active skin
                    lockObj.gameObject.SetActive(false);

                    KeyElement keyElement = new KeyElement();
                    keyElement.birdHasKey = birdHasKey;
                    keyElement.id = birdHasKey.id;
                    keyElement.key = key;
                    keyElement.lockObj = lockObj;

                    keyElements.Add(keyElement);
                    idkey++;
                    totalLocked++;
                }
            }

            var birdsInStand = data.standParent.birdsOnArray;
            for (int i = 0; i < birdsInStand.Length; i++)
            {
                birdsInStand[i].isLockMove = true;
            }


            body.gameObject.SetActive(false);
            prison_4.body.gameObject.SetActive(false);
            prison_5.body.gameObject.SetActive(false);
            prison_6.body.gameObject.SetActive(false);
        }

        public override void ReAppearExtral(int idBird, int idStand)
        {

        }

        protected override void CheckExtralHandle(BirdBase bird, StandingBase stand, bool isFlyOut)
        {
            if (isFlyOut)
            {
                bool isEnd = false;
                for (int i = 0; i < keyElements.Count; i++)
                {
                    int index = i;
                    if (bird.id == keyElements[index].birdHasKey.id)
                    {
                        numLocked++;
                        if (numLocked >= totalLocked)
                        {
                            finalBirdHasKey = bird;
                            state = StateExtral.Success;
                            isEnd = true;

                            GamePlayController_BirdSort.Instance.tutorialController.DoneTutExtral();
                        }
                        else
                        {
                            isEnd = false;
                        }


                        //if (m_ExtralSuccess != null)
                        //    StopCoroutine(m_ExtralSuccess);
                        //m_ExtralSuccess = ExtralHandle(keyElements[index], bird, isEnd);
                        StartCoroutine(ExtralHandle(keyElements[index], bird, isEnd));
                        break;
                    }

                }

            }
        }

        private IEnumerator ExtralHandle(KeyElement keyElement, BirdBase bird, bool isEnd)
        {
            yield return new WaitUntil(() => bird.isFlyOut);
            FlyKey(keyElement, isEnd);
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


                TypeStand typeStandInLevel = GamePlayController_BirdSort.Instance.level.typeStandInLevel;
                if (typeStandInLevel == TypeStand.Slot_4)
                {
                    prison_4.body.gameObject.SetActive(true);
                }
                else if (typeStandInLevel == TypeStand.Slot_5)
                {
                    prison_5.body.gameObject.SetActive(true);
                }
                else if (typeStandInLevel == TypeStand.Slot_6)
                {
                    prison_6.body.gameObject.SetActive(true);
                }

                currentPrison.behideObj.transform.parent = this.transform.parent;
                currentPrison.behideObj.transform.SetAsFirstSibling();
                currentPrison.canvasGruop.alpha = 0;
                currentPrison.canvasGruop.DOFade(1, 0.2f).OnComplete(() =>
                {
                    currentPrison.doorObj.DOAnchorPosY(11.21f, 0.1f).OnComplete(() =>
                        {
                            GameController.Instance.musicManager.PlayOneShot(cageLockClip, audioCageSource);

                            for (int i = 0; i < keyElements.Count; i++)
                            {
                                int index = i;

                                var lockObj = keyElements[index].lockObj;
                                lockObj.gameObject.SetActive(true);
                                lockObj.AnimationState.SetAnimation(0, "start", false);
                                lockObj.transform.parent = currentPrison.parentLock;
                                lockObj.transform.localPosition = Vector3.zero;

                                var key = keyElements[index].key;

                                key.gameObject.SetActive(true);
                                key.transform.parent = GamePlayController_BirdSort.Instance.level.extralShowParent;
                                key.transform.SetAsLastSibling();
                                key.transform.localPosition = new Vector3(0, 400, 0);
                                key.transform.DOKill();
                                key.transform.localScale = Vector3.zero;

                                key.transform.DOScale(3f * Vector3.one, 0.3f).OnComplete(() =>
                                {
                                    float timeMove = 0.45f;
                                //float timeMove = Vector2.Distance(birdHasKey.transform.position, key.transform.position) / 3.5f;
                                key.transform.DOMove(keyElements[index].birdHasKey.transform.position, timeMove).OnComplete(() =>
                                    {
                                        GameController.Instance.musicManager.PlayOneShot(keyAppearClip, keySource);
                                        key.transform.parent = keyElements[index].birdHasKey.transform;


                                        if (index == keyElements.Count - 1)
                                        {
                                         
                                                GamePlayController_BirdSort.Instance.tutorialController.ActiveTutExtral(this);
                                                GamePlayController_BirdSort.Instance.tutorialController.ActiveHintLockBird();
                                                GamePlayController_BirdSort.Instance.extralPlayController.InitDestroyExtralButton(data.type, this);
                                            

                                            isReady = true;
                                            GamePlayController_BirdSort.Instance.returnController.IsLockReturn = false;
                                        }

                                    }).SetDelay(0.3f);
                                    key.transform.DOScale(1f, timeMove * 0.95f).SetDelay(0.3f);

                                    if (index == 0)
                                        GameController.Instance.musicManager.PlayOneShot(keyAppearClip, keySource);
                                }).SetEase(Ease.OutBack);
                            }

                        });
                });

            }, () => this.data.standParent.birdsOnArray[0].onGround));
        }

        protected override void PlayAnimEnd()
        {
            for (int i = 0; i < keyElements.Count; i++)
            {
                int index = i;
                bool isEnd = index == keyElements.Count - 1 ? true : false;
                FlyKey(keyElements[index], isEnd);
            }
            base.PlayAnimEnd();
        }

        private void FlyKey(KeyElement keyElement, bool isEnd)
        {
            var key = keyElement.key;
            var lockObj = keyElement.lockObj;

            key.transform.parent = GamePlayController_BirdSort.Instance.level.birdParent;

            key.gameObject.SetActive(true);
            key.transform.DOKill();
            key.transform.localScale = Vector3.one;
            key.transform.DOScale(1.5f, 0.35f).OnComplete(() =>
            {
                // key.transform.DOScale(0.8f, 0.3f);
                GameController.Instance.musicManager.PlayOneShot(getKeyClip, keySource);
            });

            float timeMoveKey = Vector2.Distance(key.transform.position, lockObj.transform.position) / 4.5f;
            // float timeMoveKey = 0.25f;
            key.transform.DOMove(lockObj.transform.position, timeMoveKey).OnComplete(() =>
            {
                key.SetActive(false);
                var trackLock = lockObj.AnimationState.SetAnimation(0, "unlock", false);
                trackLock.Complete += (s) =>
                {

                    if (isEnd)
                    {
                        GameController.Instance.musicManager.PlayOneShot(cageOpenClip, audioCageSource);
                        currentPrison.doorObj.DOAnchorPosY(137f, 0.1f).OnComplete(() =>
                        {
                            body.gameObject.SetActive(false);
                            currentPrison.canvasGruop.DOFade(0, 0.15f);
                            currentPrison.behideObj.DOFade(0, 0.15f);

                            var birdsInStand = data.standParent.birdsOnArray;
                            for (int i = 0; i < birdsInStand.Length; i++)
                            {
                                birdsInStand[i].isLockMove = false;
                            }
                        });

                    }
                };

            });
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
            foreach (KeyElement param in keyElements)
            {
                Destroy(param.key.gameObject);
            }


            Destroy(currentPrison.behideObj.gameObject);


            Destroy(this.gameObject);
        }
    }
}