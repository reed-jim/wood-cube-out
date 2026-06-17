using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class RotatingCoin : MonoBehaviour
{
    public Image coinImage;

    public Sprite[] poolCoins;

    void OnEnable()
    {
        StartCoroutine(Rotating());
    }

    private IEnumerator Rotating()
    {
        WaitForSeconds waitForSeconds = new WaitForSeconds(1f / 60);

        int index = Random.Range(0, poolCoins.Length - 1);

        while (gameObject.activeInHierarchy)
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
}
