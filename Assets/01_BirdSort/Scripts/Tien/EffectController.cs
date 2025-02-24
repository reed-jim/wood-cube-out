using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public class EffectController : MonoBehaviour
{
    public GameObject heartPrefab;
    public Transform scoreHappyPos;
    public AudioClip collectSound;
    public AudioClip singleCollectSound;

    private Vector3 orginalScaleScoreHappy;
    public void Init()
    {
        orginalScaleScoreHappy = scoreHappyPos.localScale;
    }
    public void PlayCollectHeartEffect(Transform startEffectPos, int quantity)
    {
        bool soundPlayed = false;
        for (int i = 0; i < quantity; i++)
        {
            Vector3 randomSpawnPoint = startEffectPos.position + Random.insideUnitSphere;
            GameObject obj = SimplePool2.Spawn(heartPrefab, startEffectPos.position + new Vector3(0f, 0.4f, 0f), Quaternion.identity);
            obj.GetComponent<SpriteRenderer>().sortingLayerName = "FX";

            obj.transform.localScale = Vector3.zero;
            float rr = Random.Range(0.1f, 0.3f);
            obj.transform.DOScale(Vector3.one * 0.25f, 1f).SetDelay(rr);
            Vector3 firstPos = obj.transform.position + new Vector3(Random.Range(-0.4f, 0.4f), Random.Range(-0.2f, 0.3f), 0f);
            obj.transform.DOMove(firstPos,0.6f).SetEase(Ease.OutCubic).SetDelay(rr).OnComplete(() =>
            {
                float r = Random.Range(0f, 0.3f);
                obj.transform.DOScale(Vector3.one * 0.2f, 0.5f).SetDelay(r);
                obj.transform.DOMove(scoreHappyPos.position, 0.5f).SetDelay(r).OnComplete(() =>
                {
                    if(!soundPlayed)
                    {
                        GameController.Instance.musicManager.PlayOneShot(collectSound);
                        soundPlayed = true;
                    }
                    obj.GetComponent<SpriteRenderer>().sortingLayerName = "FXUI";
                    //scoreHappyPos.DOKill();
                    scoreHappyPos.DOScale(orginalScaleScoreHappy * 1.2f, 0.1f).OnComplete(() => 
                    {
                        scoreHappyPos.DOScale(orginalScaleScoreHappy, 0.07f);
                    });
                    SimplePool2.Despawn(obj);
                });
            });
        }
    }
    public void PlayCollectSingleHeartEffect(Transform startEffectPos)
    {
        PlayCollectHeartEffect(startEffectPos, 1);
        //Vector3 orginalScale = scoreHappyPos.localScale;

        //Vector3 randomSpawnPoint = startEffectPos.position + Random.insideUnitSphere;
        //GameObject obj = SimplePool2.Spawn(heartPrefab, startEffectPos.position + new Vector3(0f, 0.4f, 0f), Quaternion.identity);
        //obj.transform.localScale = Vector3.zero;
        //float rr = Random.Range(0.1f, 0.3f);
        //obj.transform.DOScale(Vector3.one * 0.25f, 1f).SetDelay(rr);
        //Vector3 firstPos = obj.transform.position + new Vector3(Random.Range(-0.4f, 0.4f), Random.Range(-0.2f, 0.3f), 0f);
        //obj.transform.DOMove(firstPos, 0.6f).SetEase(Ease.OutCubic).SetDelay(rr).OnComplete(() =>
        //{
        //    float r = Random.Range(0f, 0.3f);
        //    obj.transform.DOScale(Vector3.one * 0.2f, 0.5f).SetDelay(r);
        //    obj.transform.DOMove(scoreHappyPos.position, 0.5f).SetDelay(r).OnComplete(() =>
        //    {
        //        GameController.Instance.musicManager.PlayOneShot(singleCollectSound);
        //    });
        //    obj.GetComponent<SpriteRenderer>().sortingLayerName = "FXUI";
        //    scoreHappyPos.DOKill();
        //    scoreHappyPos.DOScale(orginalScale * 1.2f, 0.1f).OnComplete(() =>
        //    {
        //        scoreHappyPos.DOScale(orginalScale, 0.1f);
        //    });
        //});
    }
}
