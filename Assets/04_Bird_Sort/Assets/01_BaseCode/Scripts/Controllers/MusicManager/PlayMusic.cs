using UnityEngine;

public class PlayMusic : MonoBehaviour {

	public AudioClip musicClip;

	private void Start()
	{
		BirdSortGameController.Instance.musicManager.PlayMusic(musicClip);
	}

}
