

    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    using DG.Tweening;
    using System;
    public class IQScorePassController : MonoBehaviour
    {
        protected IDisposable _downloadAvatarDispose;
        protected IDisposable _downloadFlagDispose;


        public Transform passBar;
        public IQScorePlayerBadge player1;
        public IQScorePlayerBadge player2;
        public IQScorePlayerBadge player3;
        public IQScorePlayerBadge player4;

        public IQScorePlayerBadge goldBadge;

        public Text iqScoreText;

        public AudioClip soundUp1;
        public AudioClip soundUp2;
        public AudioClip soundUp3;

        private Vector3 originalPos1;
        private Vector3 originalPos2;
        private Vector3 originalPos3;
        private Vector3 originalPos4;

        private WinBox controller;

        private int _valueCache;
        private Tweener _textTween;

        public void Init(WinBox _controller)
        {
            this.controller = _controller;
            if (Application.internetReachability == NetworkReachability.NotReachable || !LoginController.IsConnected)
            {
                passBar.gameObject.SetActive(false);
                controller.MoveUpCompleteContent();
                return;
            }
            InitPlayersAround();

            originalPos1 = player1.GetComponent<RectTransform>().anchoredPosition;
            originalPos2 = player2.GetComponent<RectTransform>().anchoredPosition;
            originalPos3 = player3.GetComponent<RectTransform>().anchoredPosition;
            originalPos4 = player4.GetComponent<RectTransform>().anchoredPosition;
            player4.SetCurrPlayer();
            //goldBadge.SetCurrPlayer();
            passBar.gameObject.SetActive(false);

            _valueCache = BirdSort2.GamePlayController_BirdSort.Instance.scoreController.currentScore;
        }
        public void StartAnim()
        {
            if (Application.internetReachability == NetworkReachability.NotReachable || !LoginController.IsConnected)
            {
                controller.MoveUpCompleteContent();
                return;
            }
            Step1();
        }
        public void Step1()
        {
            if (BirdSort2.GamePlayController_BirdSort.Instance.iqScoreController.aroundPlayers.Count < 3)
            {
                controller.MoveUpCompleteContent();
                passBar.gameObject.SetActive(false);
                return;
            }
            passBar.gameObject.SetActive(true);
            player4.SetCurrPlayer();
            goldBadge.SetCurrPlayer();
            player4.DOAnimScore(UseProfile.IQScore, () =>
            {
                Step2();
            });
            controller.MoveDownCompleteContent();
        }

        public void Step2()
        {
            player4.transform.GetComponent<RectTransform>().DOAnchorPos(originalPos3, 0.25f).OnComplete(() =>
            {
            //GamePlayController.Instance.a
            GameController.Instance.musicManager.PlayOneShot(soundUp1);
                Step3();
            });
            player3.transform.GetComponent<RectTransform>().DOAnchorPos(originalPos4, 0.25f);
        }
        public void Step3()
        {
            player4.transform.GetComponent<RectTransform>().DOAnchorPos(originalPos2, 0.25f).OnComplete(() =>
            {
                GameController.Instance.musicManager.PlayOneShot(soundUp2);
                Step4();
            });
            player2.transform.GetComponent<RectTransform>().DOAnchorPos(originalPos3, 0.25f);
        }
        public void Step4()
        {
            player4.transform.GetComponent<RectTransform>().DOAnchorPos(originalPos1, 0.25f).OnComplete(() =>
            {
                GameController.Instance.musicManager.PlayOneShot(soundUp3);
                Step5();
            });
            player1.transform.GetComponent<RectTransform>().DOAnchorPos(originalPos2, 0.25f);
        }
        public void Step5()
        {
            goldBadge.gameObject.SetActive(true);
            goldBadge.transform.DOScale(1.2f, 0.2f).OnComplete(() =>
            {
                goldBadge.transform.DOScale(1f, 0.12f).SetDelay(0.1f);
            });
        }

        public void InitPlayersAround()
        {
          
            List<IQPlayerBadgeData> aroundPlayers = BirdSort2.GamePlayController_BirdSort.Instance.iqScoreController.aroundPlayers;
            if (aroundPlayers.Count == 0)
            {
            BirdSort2.GamePlayController_BirdSort.Instance.iqScoreController.GetPlayersAround();
                aroundPlayers = BirdSort2.GamePlayController_BirdSort.Instance.iqScoreController.aroundPlayers;
            }
            if (aroundPlayers.Count < 3)
            {
                passBar.gameObject.SetActive(false);
                controller.MoveUpCompleteContent();
            }
            if (aroundPlayers.Count >= 1)
            {
                player1.SetUI(aroundPlayers[0]);
            }
            else
            {
                player1.SetFakeUI();
            }
            if (aroundPlayers.Count >= 2)
            {
                player2.SetUI(aroundPlayers[1]);
            }
            else
            {
                player2.SetFakeUI();
            }
            if (aroundPlayers.Count >= 3)
            {
                player3.SetUI(aroundPlayers[2]);
            }
            else
            {
                player3.SetFakeUI();
            }
        }

        public virtual void UpdateIQValue(int IQValue)
        {
            _textTween = _textTween ??
                         DOTween.To(() => _valueCache, value =>
                         {
                             _valueCache = value;
                             iqScoreText.text = "IQ: " + value.ToString();
                         }, IQValue, 1f)
                                .SetUpdate(true)
                                .SetAutoKill(false).OnComplete(() =>
                                {
                                    iqScoreText.text = "IQ: " + IQValue.ToString();
                                });
            _textTween.ChangeStartValue(_valueCache);
            _textTween.ChangeEndValue(IQValue);
            _textTween.Play();
        }
    }
