using System.Collections;
using PrimeTween;
using UnityEngine;
using UnityEngine.UI;

public class RotatingCoin : MonoBehaviour
{
    public Image coinImage;
    public ParticleSystem fx;

    public Sprite[] poolCoins;

    void OnEnable()
    {
        StartCoroutine(Rotating());
    }

    private IEnumerator Rotating()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(1f / 60);

        int index = Random.Range(0, poolCoins.Length - 1);

        coinImage.enabled = true;

        while (coinImage.enabled)
        {
            coinImage.sprite = poolCoins[index];

            index++;

            if (index >= poolCoins.Length)
            {
                index = 0;
            }

            yield return waitForSeconds;
        }
    }

    public void PlayFX()
    {
        coinImage.enabled = false;

        fx.gameObject.SetActive(true);
        fx.Play();

        Tween.Delay(0.5f).OnComplete(() =>
        {
            fx.Stop();
            gameObject.SetActive(false);
        });
    }
}
