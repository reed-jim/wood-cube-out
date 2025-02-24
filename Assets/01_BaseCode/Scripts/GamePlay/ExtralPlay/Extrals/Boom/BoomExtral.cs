using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Spine.Unity;

public class BoomExtral : ExtralPlayBase
{
    protected float timeCoolDown;
    [SerializeField] private Text coolDownTxt;
    [SerializeField] private SkeletonAnimation boomSke;
    [SerializeField] private GameObject coolDownObj;

    [SerializeField] protected AudioClip beepClip;
    [SerializeField] protected AudioClip explosionClip;
    [SerializeField] protected AudioSource audioBoomSource;

    [SerializeField] protected GameObject fxFuse;
    [SerializeField] protected GameObject fxExplosion;

    [SerializeField] protected ExtralLayer boomLayer;
    private bool isEndExtral;
    private bool isBoomExplosied;

    public override void Init(ExtralPlayData data, int id)
    {
        base.Init(data, id);

        timeCoolDown = this.data.extralValues[0];
        coolDownTxt.text = timeCoolDown.ToString();
        coolDownObj.transform.DOKill();
        coolDownObj.transform.localScale = Vector3.zero;

        body.gameObject.SetActive(false);

        StartCoroutine(Helper.StartAction(() =>
        {
            coolDownObj.transform.DOScale(1.2f, 0.25f).SetLoops(-1, LoopType.Yoyo); ;
            coolDownTxt.DOColor(new Color(1, 0, 0, 1), 0.25f).SetLoops(-1, LoopType.Yoyo);
            body.transform.DOKill();
            body.transform.DOScale(1.1f, 0.35f).SetLoops(-1, LoopType.Yoyo);

        }, () => timeCoolDown <= 5));

        fxFuse.gameObject.SetActive(false);
        fxExplosion.gameObject.SetActive(false);
        data.birdParent.isHasExtral = true;
        GamePlayController.Instance.level.AddBirdHasExtral(data.birdParent.id, data.birdParent);
        boomLayer.Init(LayerBase);
        data.birdParent.AddLayerExtral(boomLayer);

        isEndExtral = false;
        isBoomExplosied = false;
    }

    protected override void CheckExtralHandle(BirdBase bird, StandingBase stand, bool isFlyMatch3, bool isUseManget = false)
    {
        if (isEndExtral)
            return;
        if (!isFlyMatch3)
        {
            if (!isUseManget)
                ActiveCoolDown();
        }
        else
        {
            if (bird != null && data.birdParent.id == bird.id && !data.birdParent.isBirdInGadern)
            {
                //Phá boom thành công, hiển thị effect phá boom
                state = StateExtral.Success;

                if (m_ExtralSuccess != null)
                    StopCoroutine(m_ExtralSuccess);
                m_ExtralSuccess = ExtralSuccessHandle();
                StartCoroutine(m_ExtralSuccess);
               
                isEndExtral = true;
            }
            else
            {
                if (!isUseManget)
                    ActiveCoolDown();
            }
        }
    }

    private void ActiveCoolDown()
    {
        timeCoolDown--;
        GameController.Instance.musicManager.PlayOneShot(beepClip, audioBoomSource);

        if (timeCoolDown <= 0)
        {
            timeCoolDown = 0;
            EndExtral(false);
        }

        coolDownTxt.text = timeCoolDown.ToString();
    }

    private IEnumerator ExtralSuccessHandle()
    {
        // this.transform.parent = GamePlayController.Instance.level.birdParent;
        yield return new WaitUntil(() => data.birdParent.isMatch3Done);
        PlayAnimEnd();
    }

    protected override void ReturnExtralHandle()
    {
        timeCoolDown++;
        if (timeCoolDown > this.data.extralValues[0])
            timeCoolDown = this.data.extralValues[0];
        coolDownTxt.text = timeCoolDown.ToString();
        //state = StateExtral.Life;
    }

    public override void EndExtral(bool isDone)
    {
        base.EndExtral(isDone);
        if (!isDone)
        {
            //Gọi hiệu ứng nổ, show popup lose
            Debug.Log("============= Boom Lose");
            fxFuse.gameObject.SetActive(false);
            fxExplosion.gameObject.SetActive(true);

            state = StateExtral.Fail;
            GamePlayController.Instance.state = StateGame.Lose;
            //this.data.birdParent.DisableBody();
            body.gameObject.SetActive(false);
            this.data.birdParent.DisableBody();
            GameController.Instance.musicManager.PlayOneShot(explosionClip, audioBoomSource);
            //EventDispatcher.EventDispatcher.Instance.PostEvent(EventID.FLY_OUT_RANDOM, this.data.birdParent);
            if (GameController.Instance.modeGame == ModeGame.Campaign)
            {
                StartCoroutine(Helper.StartAction(() =>
                {
                    GamePlayController.Instance.level.Lose(type: 0, isLoseExtral: true);
                }, 2f));
            }
            isEndExtral = true;
            isBoomExplosied = true;
        }
        else
        {

        }
        //data.birdParent.isHasExtral = false;
        //GamePlayController.Instance.level.RemoveBirdHasExtral(data.birdParent.id, data.birdParent);
    }

    public override void ReAppearExtral(int idBird, int idStand)
    {

    }

    protected override void PlayAnimAppear()
    {
        base.PlayAnimAppear();


        StartCoroutine(Helper.StartAction(() =>
        {
            body.gameObject.SetActive(true);
            this.transform.parent = GamePlayController.Instance.level.birdParent;
            this.transform.SetAsLastSibling();
            this.transform.localPosition = new Vector3(0, 0, 0);
            this.transform.DOKill();
            this.transform.localScale = 3 * Vector3.one;

            GameController.Instance.musicManager.PlayOneShot(beepClip, audioBoomSource);
            boomSke.AnimationState.ClearTracks();

            var track = boomSke.AnimationState.SetAnimation(0, "start", false);
            track.Complete += (s) =>
            {
                boomSke.AnimationState.SetAnimation(0, "idle", true);
                body.transform.DOKill();
                body.transform.DOScale(1.1f, 0.7f).SetLoops(-1, LoopType.Yoyo);
                fxFuse.gameObject.SetActive(true);
                coolDownObj.transform.DOScale(1, 0.05f);

                // float timeMove = Vector2.Distance(this.data.birdParent.transform.position, body.transform.position) / 3.5f;
                float timeMove = 0.25f;
                this.transform.DOMove(this.data.birdParent.transform.position, timeMove).OnComplete(() =>
                {
                    if (GameController.Instance.modeGame == ModeGame.Campaign)
                    {

                        //GamePlayController.Instance.tutorialController.ActiveTutExtral(this);
                        GamePlayController.Instance.extralPlayController.InitDestroyExtralButton(data.type, this);
                    }


                    this.transform.parent = this.data.birdParent.transform;
                    this.transform.localPosition = Vector3.zero;
                    isReady = true;
                    GamePlayController.Instance.extralPlayController.IsExtralsReady();

                    data.birdParent.ReloadCurrentLayer();
                }).SetDelay(0.05f);
                this.transform.DOScale(1, timeMove * 0.95f).SetDelay(0.05f);
            };

        }, () => this.data.birdParent.onGround));
    }

    protected override void PlayAnimEnd()
    {
        Debug.Log("=============== Boom Done ");
        coolDownObj.gameObject.SetActive(false);
        fxFuse.gameObject.SetActive(false);
        body.transform.DOKill();
        body.transform.localScale = Vector3.one;
        boomSke.AnimationState.ClearTracks();
        var track = boomSke.AnimationState.SetAnimation(0, "fall", false);
        track.TimeScale = 1f;
        track.Complete += (s) =>
        {
            body.gameObject.SetActive(false);

        };
        base.PlayAnimEnd();
        data.birdParent.isHasExtral = false;
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();
        //if (Input.GetMouseButtonDown(0))
        //    GamePlayController.Instance.tutorialController.DeActiveTutExtral(true);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        this.transform.DOKill();
    }
    public override void ResetExtral()
    {
        Destroy(this.gameObject);
    }

    public override void ReviveExtral()
    {
        if (this.data.birdParent.state != StateBird.Match3 && !this.data.birdParent.isDie)
            this.data.birdParent.ActiveBody();
        if (isBoomExplosied)
        {
            isEndExtral = true;
            data.birdParent.isHasExtral = false;
        }
        else
        {
            timeCoolDown = this.data.extralValues[0];
            coolDownTxt.text = timeCoolDown.ToString();
            isEndExtral = false;
        }

    }
}
