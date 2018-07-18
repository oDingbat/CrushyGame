using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour {

	public List<AudioSource> jukeboxes;
	public GameObject prefab_jukebox;

	int jukeboxCount = 16;
	int jukeboxIndex = 0;

	private void Start () {
		InitializeJukeboxes();
	}

	private void InitializeJukeboxes () {
		for (int j = 0; j < jukeboxCount; j++) {
			AudioSource newJukebox = Instantiate(prefab_jukebox, transform.position, Quaternion.identity, transform).GetComponent<AudioSource>();
			jukeboxes.Add(newJukebox);
		}
	}

	public void PlayClipAtPoint(AudioClip clip, Vector2 point) {
		PlayClipAtPoint(clip, point, 1f, 1f, 0f);
	}

	public void PlayClipAtPoint(AudioClip clip, Vector2 point, float volume) {
		PlayClipAtPoint(clip, point, volume, 1f, 0f);
	}

	public void PlayClipAtPoint(AudioClip clip, Vector2 point, float volume, float pitch) {
		PlayClipAtPoint(clip, point, volume, pitch, 0f);
	}

	public void PlayClipAtPoint(AudioClip clip, Vector2 point, float volume, float pitch, float pitchFluctuation) {
		// Player audio clip at point
		AudioSource jukeboxCurrent = jukeboxes[jukeboxIndex];
		jukeboxCurrent.transform.position = point;
		jukeboxCurrent.clip = clip;
		jukeboxCurrent.volume = volume;
		jukeboxCurrent.pitch = pitch + Random.Range(-pitchFluctuation / 2, pitchFluctuation);
		jukeboxCurrent.Play();

		// Increment jukeboxIndex
		jukeboxIndex = (jukeboxIndex == jukeboxCount - 1 ? 0 : jukeboxIndex + 1);
	}

}
