using UnityEngine;

public class PlayOnAwake : MonoBehaviour {
	
    public AudioClip audioClip;
	
	private void OnEnable () {
        BirdSortGameController.Instance.musicManager.PlayOneShot(audioClip);
    }
	
}
