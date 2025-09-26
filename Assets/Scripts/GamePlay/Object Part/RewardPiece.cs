using System;
using UnityEngine;

public class RewardPiece : MonoBehaviour
{
    public static event Action<int> showSupriseCoinRewardEvent;

    public void EarnReward()
    {
        ParticleSystem rewardFx = ObjectPoolingEverything.GetFromPool<ParticleSystem>(GameConstants.SUPRISE_REWARD_FX);

        rewardFx.gameObject.SetActive(true);
        rewardFx.transform.position = transform.position;
        rewardFx.Play();

        showSupriseCoinRewardEvent?.Invoke(999);
    }
}
