namespace BirdSort2
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using DG.Tweening;

    public class TutorialController : MonoBehaviour
    {
        [SerializeField] private GameObject m_handTutGamePlay;

        public CanvasGroup tutBoomExtral;
        public CanvasGroup tutLockBirdExtral;
        public CanvasGroup tutEggExtral;
        public CanvasGroup tutSleepExtral;
        public CanvasGroup tutLockStandExtral;

     
        
        public List<GameObject> listTutIcons;
        
        public List<GameObject> listCanvasGroup;

        public bool isHasTutInLevel
        {
            get
            {
                foreach (var item in listCanvasGroup)
                {
                    if(item.activeSelf)
                    {
                        return true;
                    }
                }

                return false;
            }
        }
        public static int LEVEL_START_TUT_BOOM
        {
            get
            {
                if (RemoteConfigController.GetBoolConfig(FirebaseConfig.TEST_LEVEL_CAGE_BOOM, true))
                    return 11;
                else
                    return 11;
            }
        }

        public static int LEVEL_START_TUT_CAGE
        {
            get
            {
                if (RemoteConfigController.GetBoolConfig(FirebaseConfig.TEST_LEVEL_CAGE_BOOM, true))
                    return 21;
                else
                    return 21;
            }
        }

        public static int LEVEL_START_TUT_SLEEP
        {
            get
            {
                if (RemoteConfigController.GetBoolConfig(FirebaseConfig.TEST_LEVEL_CAGE_BOOM, true))
                    return 31;
                else
                    return 31;
            }
        }

        public static int LEVEL_START_TUT_EGG
        {
            get
            {
                if (RemoteConfigController.GetBoolConfig(FirebaseConfig.TEST_LEVEL_CAGE_BOOM, true))
                    return 41;
                else
                    return 41;
            }
        }

        public static int LEVEL_START_TUT_LOCKSTAND
        {
            get
            {
                if (RemoteConfigController.GetBoolConfig(FirebaseConfig.TEST_LEVEL_CAGE_BOOM, true))
                    return 51;
                else
                    return 51;
            }
        }

        private GameObject handTut
        {
            get { return m_handTutGamePlay; }
        }

        private int currentIndexTut;


        private bool isTutLevelExtral;
        private bool doneTutExtral;

        public void Init()
        {
            m_handTutGamePlay.gameObject.SetActive(false);
            StartTutGamePlay();

            // ActiveTutExtral();
        }


        public void ActiveTutExtral(ExtralPlayBase_BirdSort extral)
        {

            DeActiveTutExtral();
            //if (!Localization.language.Equals("English"))
            //{
            for (int i = 0; i < listTutIcons.Count; i++)
            {
                listTutIcons[i].SetActive(false);
            }
            //}
            if (UseProfile.CurrentLevelPlay == LEVEL_START_TUT_BOOM)
            {
                tutBoomExtral.gameObject.SetActive(true);
                tutBoomExtral.alpha = 0;
                tutBoomExtral.DOKill();
                tutBoomExtral.DOFade(1, 0.3f);
                return;
            }

            if (UseProfile.CurrentLevelPlay == LEVEL_START_TUT_CAGE)
            {
                tutLockBirdExtral.gameObject.SetActive(true);
                tutLockBirdExtral.alpha = 0;
                tutLockBirdExtral.DOKill();
                tutLockBirdExtral.DOFade(1, 0.3f);
                return;
            }

            if (UseProfile.CurrentLevelPlay == LEVEL_START_TUT_SLEEP)
            {
                tutSleepExtral.gameObject.SetActive(true);
                tutSleepExtral.alpha = 0;
                tutSleepExtral.DOKill();
                tutSleepExtral.DOFade(1, 0.3f);
                return;
            }

            if (UseProfile.CurrentLevelPlay == LEVEL_START_TUT_EGG)
            {
                tutEggExtral.gameObject.SetActive(true);
                tutEggExtral.alpha = 0;
                tutEggExtral.DOKill();
                tutEggExtral.DOFade(1, 0.3f);
                return;
            }

            if (UseProfile.CurrentLevelPlay == LEVEL_START_TUT_LOCKSTAND)
            {
                tutLockStandExtral.gameObject.SetActive(true);
                tutLockStandExtral.alpha = 0;
                tutLockStandExtral.DOKill();
                tutLockStandExtral.DOFade(1, 0.3f);
                return;
            }
        }

        public void DeActiveTutExtral(bool isFade = false)
        {


            if (isFade)
            {
                if (tutBoomExtral.gameObject.activeSelf)
                {
                    tutBoomExtral.DOKill();
                    tutBoomExtral.DOFade(0, 0.3f).OnUpdate(() => { tutBoomExtral.gameObject.SetActive(false); });
                }

                if (tutLockBirdExtral.gameObject.activeSelf)
                {
                    tutLockBirdExtral.DOKill();
                    tutLockBirdExtral.DOFade(0, 0.3f).OnUpdate(() => { tutLockBirdExtral.gameObject.SetActive(false); });
                }

                if (tutEggExtral.gameObject.activeSelf)
                {
                    tutEggExtral.DOKill();
                    tutEggExtral.DOFade(0, 0.3f).OnUpdate(() => { tutEggExtral.gameObject.SetActive(false); });
                }

                if (tutSleepExtral.gameObject.activeSelf)
                {
                    tutSleepExtral.DOKill();
                    tutSleepExtral.DOFade(0, 0.3f).OnUpdate(() => { tutSleepExtral.gameObject.SetActive(false); });
                }

                if (tutLockStandExtral.gameObject.activeSelf)
                {
                    tutLockStandExtral.DOKill();
                    tutLockStandExtral.DOFade(0, 0.3f).OnUpdate(() => { tutLockStandExtral.gameObject.SetActive(false); });
                }
            }
            else
            {
                tutBoomExtral.gameObject.SetActive(false);
                tutLockBirdExtral.gameObject.SetActive(false);
                tutEggExtral.gameObject.SetActive(false);
                tutSleepExtral.gameObject.SetActive(false);
                tutLockStandExtral.gameObject.SetActive(false);
            }
        }


        private void StartTutGamePlay()
        {


            int currentLevelPlay = UseProfile.CurrentLevelPlay;

            if (currentLevelPlay == 1)
            {
                var stand = GamePlayController_BirdSort.Instance.level.standsInLevel_Real[0];
                m_handTutGamePlay.gameObject.SetActive(true);
                m_handTutGamePlay.transform.position = stand.posTut.position;
                //GameController.Instance.AnalyticsController.LogTutLevelStart(currentLevelPlay);
            }
        }

        public void ActiveHintLockBird()
        {


            int currentLevelPlay = UseProfile.CurrentLevelPlay;

            if (currentLevelPlay == LEVEL_START_TUT_CAGE || currentLevelPlay == LEVEL_START_TUT_SLEEP ||
                currentLevelPlay == LEVEL_START_TUT_EGG || currentLevelPlay == LEVEL_START_TUT_LOCKSTAND)
            {
                int indexStand = 1;
                if (currentLevelPlay == LEVEL_START_TUT_LOCKSTAND)
                {
                    indexStand = 2;
                }
                else if (currentLevelPlay == LEVEL_START_TUT_EGG)
                {
                    indexStand = 0;
                }
                else if (currentLevelPlay == LEVEL_START_TUT_SLEEP)
                {
                    indexStand = 0;
                }

                var stand = GamePlayController_BirdSort.Instance.level.standsInLevel_Real[indexStand];
                m_handTutGamePlay.gameObject.SetActive(true);
                m_handTutGamePlay.transform.position = stand.posTut.position;
                isTutLevelExtral = true;
            }
        }


        public int currentTurnChoiceStand;

        public void ActiveTutGamePlay(StandingBase StandChoice)
        {


            int currentLevelPlay = UseProfile.CurrentLevelPlay;

            if (currentLevelPlay == 1)
            {
                var standTarget = GamePlayController_BirdSort.Instance.hintController.GetPosHint(StandChoice.ID);
                if (standTarget != null)
                {
                    m_handTutGamePlay.gameObject.SetActive(true);
                    m_handTutGamePlay.transform.DOKill();
                    m_handTutGamePlay.transform.DOMove(standTarget.posTut.position, 0.5f);
                }
                else
                {
                    // m_handTut.gameObject.SetActive(false);
                }
            }
        }

        public void ActiveCanHint(StandingBase currentStand)
        {


            int currentLevelPlay = UseProfile.CurrentLevelPlay;
            if (currentLevelPlay == 1)
            {
                if (currentStand.isTargetHint)
                {
                    ActiveTutGamePlay(currentStand);
                    currentStand.isTargetHint = false;
                    return;
                }

                var standTarget = GamePlayController_BirdSort.Instance.hintController.GetPosCanHint();

                if (standTarget != null)
                {
                    standTarget.isTargetHint = true;
                    m_handTutGamePlay.gameObject.SetActive(true);
                    m_handTutGamePlay.transform.DOKill();
                    m_handTutGamePlay.transform.DOMove(standTarget.posTut.position, 0.5f);
                }
                else
                {
                    // m_handTut.gameObject.SetActive(false);
                }
            }
        }

        public void ActiveCanStandEmpty(StandingBase currentStand)
        {


            int currentLevelPlay = UseProfile.CurrentLevelPlay;
            if (isTutLevelExtral && (currentLevelPlay == LEVEL_START_TUT_CAGE || currentLevelPlay == LEVEL_START_TUT_SLEEP))
            {
                if (!doneTutExtral)
                {
                    var standTarget = GamePlayController_BirdSort.Instance.hintController.GetStandCanSort(currentStand.ID);
                    if (standTarget != null)
                    {
                        m_handTutGamePlay.gameObject.SetActive(true);
                        m_handTutGamePlay.transform.DOKill();
                        m_handTutGamePlay.transform.DOMove(standTarget.posTut.position, 0.5f);
                    }
                }
            }
        }

        private int numClickStand;

        public void OnClickStand(StandingBase currentStand)
        {

            int currentLevelPlay = UseProfile.CurrentLevelPlay;
            if (isTutLevelExtral &&
                (currentLevelPlay == LEVEL_START_TUT_CAGE || currentLevelPlay == LEVEL_START_TUT_SLEEP ||
                 currentLevelPlay == LEVEL_START_TUT_EGG || currentLevelPlay == LEVEL_START_TUT_LOCKSTAND) &&
                !doneTutExtral)
            {
                if (currentLevelPlay == LEVEL_START_TUT_LOCKSTAND)
                {
                    var standTarget = GamePlayController_BirdSort.Instance.hintController.GetStandCanSort2(currentStand.ID);
                    if (standTarget != null)
                    {
                        m_handTutGamePlay.gameObject.SetActive(true);
                        m_handTutGamePlay.transform.DOKill();
                        m_handTutGamePlay.transform.DOMove(standTarget.posTut.position, 0.5f);
                    }
                }
                else if (currentLevelPlay == LEVEL_START_TUT_EGG)
                {
                    var standTarget =
                        GamePlayController_BirdSort.Instance.hintController.GetStandCanSort2(currentStand.ID,
                            numClickStand < 2 ? false : true);
                    if (standTarget != null)
                    {
                        m_handTutGamePlay.gameObject.SetActive(true);
                        m_handTutGamePlay.transform.DOKill();
                        m_handTutGamePlay.transform.DOMove(standTarget.posTut.position, 0.5f);
                    }
                }
                else
                {
                    var standTarget = GamePlayController_BirdSort.Instance.hintController.GetPosHint2(currentStand.ID);
                    if (standTarget != null)
                    {
                        m_handTutGamePlay.gameObject.SetActive(true);
                        m_handTutGamePlay.transform.DOKill();
                        m_handTutGamePlay.transform.DOMove(standTarget.posTut.position, 0.5f);
                    }
                }
            }

            numClickStand++;
        }

        public void DoneTutExtral()
        {


            int currentLevelPlay = UseProfile.CurrentLevelPlay;
            if (isTutLevelExtral && (currentLevelPlay == LEVEL_START_TUT_CAGE ||
                                     currentLevelPlay == LEVEL_START_TUT_SLEEP || currentLevelPlay == LEVEL_START_TUT_EGG ||
                                     currentLevelPlay == LEVEL_START_TUT_LOCKSTAND))
            {
                m_handTutGamePlay.gameObject.SetActive(false);
                doneTutExtral = true;
            }
        }

        public void OnWinLevel()
        {


            m_handTutGamePlay.gameObject.SetActive(false);
            int currentLevelPlay = UseProfile.CurrentLevelPlay;
            if (currentLevelPlay == 1 || currentLevelPlay == 2)
            {
                //  GameController.Instance.AnalyticsController.LogTutLevelEnd(currentLevelPlay);
            }
        }

        private bool isHintedReturn;

        public void ActiveHintReturnBtn()
        {


     
        }

        public void OnClickHintReturnBtn()
        {


      
          
        }


        private bool isHintedBuyStand;

        public void ActiveHintBuyStandBtn(bool isCondition = true)
        {

        
        }

        public void OnClickHintBuyStandBtn()
        {


            m_handTutGamePlay.gameObject.SetActive(false);
        }
    }
}