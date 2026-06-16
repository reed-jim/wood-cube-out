namespace BirdSort2
{
	using UnityEngine;

	public class PlayOnAwake_BirdSort : MonoBehaviour
	{

		public AudioClip audioClip;

		private void OnEnable()
		{
			GameController.Instance.musicManager.PlayOneShot(audioClip);
		}

	}
}