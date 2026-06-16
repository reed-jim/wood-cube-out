namespace BirdSort2
{
	using UnityEngine;

	public class PlayMusic__BirdSort : MonoBehaviour
	{

		public AudioClip musicClip;

		private void Start()
		{
			GameController.Instance.musicManager.PlayMusic(musicClip);
		}

	}
}