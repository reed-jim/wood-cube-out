namespace BirdSort2
{
	using UnityEngine;

	public class PlayMusic__BirdSort : MonoBehaviour
	{

		public AudioClip musicClip;

		private void Start()
		{
			BirdSortGameController.Instance.musicManager.PlayMusic(musicClip);
		}

	}
}
