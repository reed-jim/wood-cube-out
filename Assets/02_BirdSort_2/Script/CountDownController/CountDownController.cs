
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    using System;
    using DG.Tweening;
    public class CountDownController : MonoBehaviour
    {
        public TimeSpan timeSpan;
        public Text tvTime;
        public bool wasCoolDown;
        public Coroutine coroutineCoolDown;
        private BirdSort2.Level_BirdSort_2 level;


        public void Init()
        {
            level = BirdSort2.GamePlayController_BirdSort.Instance.level;
         //   HandleCoolDown();
        }

        public void HandleCoolDown()
        {
           
            HandelShowTime();
            wasCoolDown = true;
            if (coroutineCoolDown != null)
            {
                StopCoroutine(coroutineCoolDown);
                coroutineCoolDown = null;
            }
            coroutineCoolDown = StartCoroutine(CoolDown());
        }
        private IEnumerator CoolDown()
        {
            yield return new WaitForSeconds(1);
            timeSpan -= new TimeSpan(0, 0, 1);

            HandelShowTime();
            if (timeSpan <= new TimeSpan(0, 0, 30))
            {
                tvTime.DOKill();
                HandleColorText();
            }
            if (timeSpan <= new TimeSpan(0, 0, 0))
            {
                wasCoolDown = false;
            }
            if (wasCoolDown == true)
            {
                coroutineCoolDown = StartCoroutine(CoolDown());
            }
            else
            {
                StartCoroutine(corutinLose());
            }
        }
        private void HandelShowTime()
        {
            var splitText = timeSpan.Seconds.ToString();
            var temp1 = new char();
            var temp2 = new char();
            if (splitText.Length > 1)
            {
                temp1 = splitText[splitText.Length - 1];
                temp2 = splitText[0];
            }
            else
            {
                temp1 = splitText[splitText.Length - 1];
                temp2 = '0';
            }

            if (timeSpan.Minutes < 10)
            {
                tvTime.text = "0" + timeSpan.Minutes + ":" + temp2 + temp1;
            }
            else
            {
                tvTime.text = timeSpan.Minutes + ":" + temp2 + temp1;

            }

        }
        private void HandleColorText()
        {
            tvTime.DOColor(Color.red, 0.5f).OnComplete(delegate
            {
                tvTime.DOColor(Color.white, 0.5f).OnComplete(delegate { HandleColorText(); });
            });
        }
        private IEnumerator corutinLose()
        {
            yield return new WaitForSeconds(1);
         //   LoseBox.Setup().Show();
         
            LoseBox.SetupLegacy().Show();
            // Debug.LogError("Lose");



        }


        public void PauseCoolDown()
        {
            if (coroutineCoolDown != null)
            {
                StopCoroutine(coroutineCoolDown);
                coroutineCoolDown = null;
            }
        }
        public void ContinueCollDown()
        {
            coroutineCoolDown = StartCoroutine(CoolDown());

            HandelShowTime();
        }

        public void HandleAddTime(TimeSpan param)
        {
            timeSpan += param;
        }

     
        
    }
