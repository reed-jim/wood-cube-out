using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AnimBlinkMany : MonoBehaviour
{
    [SerializeField] List<Image> images = new List<Image>();
    float delay = 0.75f;

    float countdown = 5.5f;
    void Start()
    {

    }

    void Blink()
    {
        foreach (var item in images)
        {
            item.DOFade(1f, 0.5f).SetDelay(delay * images.IndexOf(item));
            item.DOFade(0f, 0.5f).SetDelay(delay * images.IndexOf(item) + 0.5f);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (countdown > 0f)
        {
            countdown -= Time.deltaTime;
        }
        else
        {
            countdown = 5.5f;
        }
        if (countdown > 5f)
        {
            Blink();
        }
    }
}
