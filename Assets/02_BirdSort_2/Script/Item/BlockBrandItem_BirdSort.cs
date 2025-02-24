using System.Collections;
using System.Collections.Generic;
using BirdSort2;
using UnityEngine;
using DG.Tweening;


public class BlockBrandItem_BirdSort : ItemBase
{
   
    private Tweener _textTween;
    [SerializeField] protected AudioClip addTimeClip;
    [SerializeField] protected AudioSource addTimeSource;
    public GoldenBrandVFX goldenBrandVFX;
    public Transform canvas;




    public BirdSort2.StandingBase randomTranformBrand
    {
        get
        {
            var ran = Random.Range(0, BirdSort2.GamePlayController_BirdSort.Instance.level.standsInLevel_Real.Count);

            return BirdSort2.GamePlayController_BirdSort.Instance.level.standsInLevel_Real[ran];
        }
    }    



    public override void Init()
    {
    

    }

    protected override bool ActiveItemHandle()
    {
        //Diễn FX
        var currentBrand = randomTranformBrand;
    
        var post = currentBrand.icon.transform.position;
        currentBrand.parent.transform.SetAsLastSibling();
        currentBrand.transform.SetAsLastSibling();
       
        
        
        var itemVFX = Instantiate(goldenBrandVFX,canvas.transform);
        itemVFX.transform.position = new Vector3(0,2.5f,0);
        itemVFX.Init();
       
        Vector3 tempVfx = new Vector3();
        if (currentBrand.side < 0)
        {  
            // itemVFX.RectTransform.localScale = new Vector3(1, 1, 1);
            // itemVFX.RectTransform.localEulerAngles = new Vector3(0, 0, 48);
            
             tempVfx = new Vector3(1, 1, 1);
          Debug.LogError("left " +  currentBrand.name);
        }
        else
        {
            itemVFX.RectTransform.localScale = new Vector3(-1, 1, 1);
            itemVFX.RectTransform.localEulerAngles = new Vector3(0, 0, -48);
            tempVfx = new Vector3(-1, 1, 1);
            Debug.LogError("right" +  currentBrand.name);
        }
        
        
        
        

        itemVFX.RectTransform.pivot = currentBrand.icon.GetComponent<RectTransform>().pivot;
        itemVFX.transform.localScale = new Vector3(0, 0, 0);
        itemVFX.transform.DOScale(tempVfx*1.3f, 0.4f).OnComplete(delegate
        {
            itemVFX.transform.DOScale(tempVfx, 0.2f).SetEase(Ease.InOutBack).OnComplete(delegate
            {
                itemVFX.smoke.DOFade(0, 0.7f);
                itemVFX.transform.DOLocalRotate(new Vector3(0,0,0), 0.5f).SetDelay(0.1f);
                itemVFX.transform.parent = currentBrand.icon.transform;
                itemVFX.transform.DOLocalMove(Vector3.zero, 0.5f).SetDelay(0.1f).OnComplete(delegate
                {
                    // var tempCur = currentBrand.icon.transform.localScale;
                    // currentBrand.icon.transform.DOScale(new Vector3(1.3f,1.3f,1.3f), 0.3f).SetEase(Ease.OutBack).OnComplete(delegate
                    // {
                    //     currentBrand.icon.transform.DOScale(new Vector3(1, 1, 1), 0.15f);
                    //
                    // });
                
                    currentBrand.HandleBlockBrand();
                    itemVFX.thumbnail.color = new Color32(0, 0, 0, 0);
                    
                    GameController.Instance.musicManager.PlayOneShot(addTimeClip, addTimeSource);
                    itemVFX.transform.DOKill();
                    itemVFX.transform.DOScale(0, 0.3f).SetEase(Ease.InBack);
                    itemVFX.transform.DOKill();
                    itemVFX.thumbnail.DOFade(0, 0.3f).OnComplete(() =>
                    {
                       
                      
                       
                    });
               
           
                });

           
            
            });

           
            
        });
      
      

        return true;
    }

    int valueCache;
    private void PlayTweenTimeText(int targetValue)
    {
        _textTween = _textTween ??
                       DOTween.To(() => valueCache, value => {
                           valueCache = value;
                           GamePlayController.Instance.gameScene.timePlayLevelTxt.text = TimeManager.ShowTime2((long)value);
                       }, targetValue, 1f)
                              .SetUpdate(true)
                              .SetAutoKill(false).OnComplete(() => {
                                  GamePlayController.Instance.gameScene.timePlayLevelTxt.text = TimeManager.ShowTime2((long)targetValue);
                              });
        _textTween.ChangeStartValue(valueCache);
        _textTween.ChangeEndValue(targetValue);
        _textTween.Play();
    }

    public override void EndItem()
    {

    }

    public override bool IsCanUseItem()
    {
        return true;
    }
}
