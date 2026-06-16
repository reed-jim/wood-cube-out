using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FXController : MonoBehaviour
{
    public enum TypeFX
    {
        Combine = 0,
        ExplosionLightning = 1,
        DoneRequiremnent = 2,
        ClaimStarChestHome = 3,
    }    

    [SerializeField] private GameObject fxCombinePrefab;
    [SerializeField] private GameObject fxExplosionLightningPrefab;
    [SerializeField] private GameObject fxDoneRequiremnentPrefab;
    [SerializeField] private GameObject fxClaimStarChestHomePrefab;

    private Dictionary<TypeFX, List<GameObject>> poolFXs;
    [SerializeField] private Transform transFX;

    public void Init()
    {
        poolFXs = new Dictionary<TypeFX, List<GameObject>>();

        InitFX(TypeFX.Combine, fxCombinePrefab);
        InitFX(TypeFX.ExplosionLightning, fxExplosionLightningPrefab);
        InitFX(TypeFX.DoneRequiremnent, fxDoneRequiremnentPrefab);
        InitFX(TypeFX.ClaimStarChestHome, fxClaimStarChestHomePrefab);
    }   
    
    private void InitFX(TypeFX type, GameObject prefab, int value = 10)
    {
        for (int i = 0; i < value; i++)
        {
            var fx = Instantiate(prefab, transFX);
            fx.gameObject.SetActive(false);

            if (poolFXs.ContainsKey(type))
            {
                poolFXs[type].Add(fx);
            }
            else
            {
                List<GameObject> lst = new List<GameObject>();
                lst.Add(fx);
                poolFXs.Add(type, lst);
            }
        }
    }    


    public GameObject GetFX(TypeFX type)
    {
        var poolFX = poolFXs[type];
        for (int i = 0; i < poolFX.Count; i++)
        {
            if (!poolFX[i].gameObject.activeSelf)
            {
                poolFX[i].gameObject.SetActive(true);
                return poolFX[i].gameObject;
            }    
        }

          

        var fx = Instantiate(GetPrefab(type), transFX);
        fx.gameObject.SetActive(true);
        poolFXs[type].Add(fx);

        return fx;
    }    

    private GameObject GetPrefab(TypeFX type)
    {
        GameObject prefab = fxCombinePrefab;
        switch (type)
        {
            case TypeFX.Combine:
                prefab = fxCombinePrefab;
                break;
            case TypeFX.ExplosionLightning:
                prefab = fxExplosionLightningPrefab;
                break;
            case TypeFX.DoneRequiremnent:
                prefab = fxDoneRequiremnentPrefab;
                break;
            case TypeFX.ClaimStarChestHome:
                prefab = fxClaimStarChestHomePrefab;
                break;
        }

        return prefab;
    }    
}
