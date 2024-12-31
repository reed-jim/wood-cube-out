using UnityEngine;

public class BoosterHammer : MonoBehaviour
{
    [SerializeField] private ParticleSystem hitFx;

    public void PlayHitFx()
    {
        hitFx.gameObject.SetActive(true);
        
        hitFx.Play();
    }
}
