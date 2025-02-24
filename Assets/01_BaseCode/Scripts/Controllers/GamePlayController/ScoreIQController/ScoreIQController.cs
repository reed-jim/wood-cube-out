using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class DataScoreLevelJson
{
    public List<DataScoreLevelJsonModel> data;

    public DataScoreLevelJson()
    {
        data = new List<DataScoreLevelJsonModel>();
    }
}

[Serializable]
public class DataScoreLevelJsonModel
{
    public int level;
    public List<DataScore> scores;
}

[Serializable]
public class DataScore
{
    public float timeAdd; //Thời điểm tăng điểm
    public int score; //Score trong thời điểm đó
}

public class ScoreIQController : MonoBehaviour
{
    public List<ComboData> comboData;

    public int currentIQScore;

    private int score;
    [HideInInspector] public List<DataScore> dataScore;
    [HideInInspector] public List<int> scoreRemeber;
    private int currentIndexCombo;
    private bool isCombo;
    private float timerScore;
    private float timmerCombo;

    public ScoreIQView view;

    [SerializeField] private AudioClip comboClip;
    [SerializeField] private AudioSource comboAudioSource;

    public void Init()
    {
        score = 0;
        currentIndexCombo = -1;
        isCombo = false;

        dataScore = new List<DataScore>();
        scoreRemeber = new List<int>();
        scoreRemeber.Add(0);

        currentIQScore = 0;

        view.Init(this);
    }

    public int InitStateScorePassWave()
    {
        var lenghtScores = scoreRemeber.Count;
        if (lenghtScores > 1)
        {
            var score = scoreRemeber[lenghtScores - 1];

            scoreRemeber.Clear();
            scoreRemeber.Add(score);

            return score;
        }

        return 0;
    }

    public int InitStateScoreReplayWave()
    {
        var lenghtScores = scoreRemeber.Count;
        if (lenghtScores > 1)
        {
            var firstScore = scoreRemeber[0];
            var scoreInWave = 0;
            for (var i = 1; i < scoreRemeber.Count; i++) scoreInWave += scoreRemeber[i];
            scoreRemeber.Clear();

            scoreRemeber.Add(firstScore);
            return scoreInWave;
        }

        return 0;
    }

    public void AddScore()
    {
        isCombo = true;
        currentIndexCombo++;
        if (currentIndexCombo >= comboData.Count)
            currentIndexCombo = comboData.Count - 1;
        timmerCombo = comboData[currentIndexCombo].timeSpace;
        score = comboData[currentIndexCombo].score;


        var m_dtScore = new DataScore();
        m_dtScore.timeAdd = timerScore;
        m_dtScore.score = score;
        dataScore.Add(m_dtScore);
        scoreRemeber.Add(score);

        currentIQScore += score;

        view.AddCombo(comboData[currentIndexCombo]);

        /*
        if (GameController.Instance.modeGame == ModeGame.PvP1)
        {
           // PUNController.Singleton.SyncScore(score);
        }*/

        GameController.Instance.musicManager.PlayOneShotWithPitch(comboClip, comboAudioSource, comboData[currentIndexCombo].pitchSound);
    }

    public ComboData GetCurrentComboData()
    {
        return comboData[currentIndexCombo];
    }    

    private void Update()
    {
        if (GamePlayController.Instance.state != StateGame.Playing)
            return;

        timerScore += Time.deltaTime;

        if (isCombo)
        {
            timmerCombo -= Time.deltaTime;
            view.CoolDownCombo((float)timmerCombo / comboData[currentIndexCombo].timeSpace);
            if (timmerCombo <= 0)
            {
                ResetCombo();
                isCombo = false;
            }
        }
    }

    public void ResetCombo()
    {
        isCombo = false;
        currentIndexCombo = -1;
        score = 0;
        view.ResetCombo();
    }
}

public enum TypeCombo
{
    Cool = 0,
    Great = 1,
    Excellent = 2,
    Perfect = 3,
    Fantastic = 4,
    Lengendary = 5
}

[Serializable]
public class ComboData
{
    public TypeCombo typeCombo;
    public int score;
    public float timeSpace;
    public Sprite iconCombo;
    public float pitchSound;
}