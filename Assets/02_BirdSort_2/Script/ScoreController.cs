namespace BirdSort2
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    using System.Collections;
    public class ScoreController : MonoBehaviour
    {
        public List<ComboData> comboData;

        public int score;
        public List<DataScore> dataScore;
        public List<int> scoreRemeber;
        private int currentIndexCombo;
        private bool isCombo;
        [SerializeField]private float timerScore;
        [SerializeField]private float timmerCombo;
        public Text tvScore;
        public int currentScore;

        private void Update()
        {
            timerScore += Time.deltaTime;

            if (isCombo)
            {
                timmerCombo -= Time.deltaTime;
                if (timmerCombo <= 0)
                {
                    ResetCombo();
                    isCombo = false;
                }
            }
        }

        public void Init()
        {
            score = 0;
            currentIndexCombo = -1;
            isCombo = false;

            dataScore = new List<DataScore>();
            scoreRemeber = new List<int>();
            scoreRemeber.Add(0);
            currentScore = 0;
        }

      


        public void AddScore()
        {
            isCombo = true;
            currentIndexCombo++;
            if (currentIndexCombo >= comboData.Count)
                currentIndexCombo = comboData.Count - 1;
            timmerCombo = comboData[currentIndexCombo].timeSpace;
            score = comboData[currentIndexCombo].score;
            ShowCombo(comboData[currentIndexCombo].typeCombo);



            var m_dtScore = new DataScore();
            m_dtScore.timeAdd = timerScore;
            m_dtScore.score = score;
            dataScore.Add(m_dtScore);
            scoreRemeber.Add(score);
            currentScore += score;

            tvScore.text = "IQ: " + currentScore;


        }

        public void ReturnScore(bool isDownScore = false)
        {

            if (isDownScore)
            {
                var lenghtScores = scoreRemeber.Count;
                if (lenghtScores >= 2)
                {
                    Debug.Log("DATA: " + scoreRemeber[lenghtScores - 1] + " " + scoreRemeber[lenghtScores - 2]);
                    score = scoreRemeber[lenghtScores - 1];
                    scoreRemeber.RemoveAt(lenghtScores - 1);

                    var m_dtScore = new DataScore();
                    m_dtScore.timeAdd = timerScore;
                    m_dtScore.score = score;
                    dataScore.Add(m_dtScore);

                }
            }

            ResetCombo();
        }

        public void ResetCombo()
        {
            isCombo = false;
            currentIndexCombo = -1;
            score = 0;
        }

        public void ShowCombo(TypeCombo typeCombo)
        {
            //  Debug.LogError("typeCombo " + typeCombo);
            // GamePlayController.Instance.effectManager.SpwanEffect(typeCombo);
        }
    }




    [Serializable]
    public class DataScore
    {
        public float timeAdd; //Thời điểm tăng điểm
        public int score; //Score trong thời điểm đó
    }
}