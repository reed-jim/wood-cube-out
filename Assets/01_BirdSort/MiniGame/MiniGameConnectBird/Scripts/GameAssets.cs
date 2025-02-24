using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Spine.Unity;
using DG.Tweening;
using System;
public class GameAssets : MonoBehaviour
{
    public Text textTurn;
    public Text textCoolDown;
    public Text tvTimeMin;
    public Text tvTimeSecond;
    public GameObject cellPrefab;
    public Transform catMoveEffectPrefab;
    public Transform heartEffectPrefab;
    public Transform cutOutCirclePrefab;
    public Transform lineHolderPrefab;
    public Transform pathMoveControllerPrefab;
    public Transform heartParticlePrefab;
    public Transform heartParticleUIPrefab;
    public Transform cellBackground;
    public Sprite grid_0;
    public Sprite grid_1;
    public List<GameObject> catPrefabs;
    public Transform canvas;
    public List<MiniGameBird> lsMiniGameBirds;
    public GameObject post1;
    public GameObject post2;
    public int randomVar;
    public bool ran;
    public PointConnectBirdMiniGame point;
    public Transform panelGame;
    public Sprite yellowThumnails;
    public Sprite sliverThumnails;
    public AudioClip birdFlyOutClip;
    public AudioClip claimSound;
    public AudioSource audioSource;
    public GameObject xObject;
    public void Init()
    {
        SpawnBirdFirst();

    }
    public void InitState()
    {
        EventDispatcher.EventDispatcher.Instance.RegisterListener(EventID.MINI_GAME_CONNECT_BIRD_EGG, HandleShowEgg);
    }

    public MiniGameBird SpawnBird(int index, Vector3 post, Transform parent, int idmap , Action callback)
    {
     
     
        randomVar = UnityEngine.Random.RandomRange(0, 2);
        if (randomVar == 1)
        {
            ran = true;
        }
        else
        {
            ran = false;
        }
        var Bird = SimplePool2.Spawn(lsMiniGameBirds[index], ran ? post1.transform.position : post2.transform.position, Quaternion.identity);
        Bird.transform.SetParent(parent);
        //4 0.75f ,  6 0.6f
        if (idmap == 4 || idmap == 2)
        {
            Bird.transform.localScale = new Vector3(0.75f * (ran ? 1 : -1), 0.75f, 0.75f);
        }
        else
        {
            Bird.transform.localScale = new Vector3(0.6f * (ran ? 1 : -1), 0.6f, 0.6f);
        }
        Bird.FlyCome("fly");
        Bird.GetComponent<RectTransform>().DOAnchorPos(post, UnityEngine.Random.RandomRange(1.5f, 1.75f)).OnComplete(delegate { Bird.FlyCome("grounding"); callback?.Invoke(); });
        Bird.idLeftRight = ran;
     
        return Bird;
    }

    public void SpawnBirdFirst()
    {
        SimplePool2.Preload(point.gameObject,1, panelGame);
        SimplePool2.Preload(point.gameObject, 2, panelGame);
        SimplePool2.Preload(point.gameObject, 3, panelGame);
    
    }    

    public void PlaySoundClaim()
    {
        audioSource.PlayOneShot(claimSound);
    }
    public void SpawnXObject(Vector3 post, Transform parent)
    {

        var XObject = SimplePool2.Spawn(xObject, new Vector3(post.x, post.y +0.3f , post.z), Quaternion.identity);
        XObject.transform.SetParent(parent);
        XObject.GetComponent<XObject>().ScaleInOut();

    }

    public void HandleShowEgg(object param)
    {
        var miniGameData = HomeController.Instance.miniGameController.connectBirdMG;
        textTurn.text = "" + (miniGameData.ConnectBirdTurnFree + UseProfile.ConnectBirdTurnBuy);
    }
    private void OnDisable()
    {
        EventDispatcher.EventDispatcher.Instance.RemoveListener(EventID.MINI_GAME_CONNECT_BIRD_EGG, HandleShowEgg);
    }
}
