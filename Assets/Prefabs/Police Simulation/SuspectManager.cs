using System.Linq;
using UnityEngine;

public class SuspectManager : Singleton<SuspectManager>
{
    public Suspect[] suspects;
    public Suspect CurrentSuspect => suspects[currentSuspectIndex];

    [SerializeField] private LevelConfigContainer levelConfigContainer;

    public int currentSuspectIndex;

    public Suspect FindSuspectNotTakenVerdict()
    {
        Suspect suspect = null;

        for (int i = 0; i < suspects.Length; i++)
        {
            if (suspects[i].decision == Decision.None)
            {
                suspect = suspects[i];

                break;
            }
        }

        return suspect;
    }

    public int NumSuspectNotTakenVerdict()
    {
        int count = 0;

        for (int i = 0; i < suspects.Length; i++)
        {
            if (suspects[i].decision == Decision.None)
            {
                count++;
            }
        }

        return count;
    }

    public int GetTotalScore()
    {
        SuspectResult[] suspectResults = levelConfigContainer.levelConfigs[0].suspectResults;

        int totalScore = 0;

        for (int i = 0; i < suspects.Length; i++)
        {
            for (int j = 0; j < suspectResults[i].results.Count; j++)
            {
                if (suspectResults[i].results[j].key == suspects[i].decision)
                {
                    totalScore += suspectResults[i].results[j].value;

                    break;
                }
            }
        }

        return totalScore;
    }

    public int GetMaxScore()
    {
        SuspectResult[] suspectResults = levelConfigContainer.levelConfigs[0].suspectResults;

        int maxScore = 0;

        for (int i = 0; i < suspectResults.Length; i++)
        {
            int maxScoreForSuspect = suspectResults[i].results.Max(item => item.value);

            maxScore += maxScoreForSuspect;
        }

        return maxScore;
    }

    public int GetScore(Suspect suspect)
    {
        SuspectResult[] suspectResults = levelConfigContainer.levelConfigs[0].suspectResults;

        for (int i = 0; i < suspects.Length; i++)
        {
            if (suspect.suspectName == suspects[i].suspectName)
            {
                for (int j = 0; j < suspectResults[i].results.Count; j++)
                {
                    if (suspectResults[i].results[j].key == suspect.decision)
                    {
                        return suspectResults[i].results[j].value;
                    }
                }
            }
        }

        return 0;
    }
}
