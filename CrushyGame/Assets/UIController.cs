using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIController : MonoBehaviour {

	PlayerController player;

	public GameObject panel_PauseMenu;

	bool isPaused;

	private void Start () {
		player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
	}

	void Update() {
		bool pressingPause = Input.GetKeyDown(player.controlScheme.pause) || Input.GetKeyDown(player.controlScheme.pauseAlt) || Input.GetButtonDown("G_Pause");
		if (pressingPause == true) {
			Pause();
		}
	}

	private void Pause () {
		isPaused = !isPaused;
		
		Time.timeScale = (isPaused == true ? 0 : 1);
		panel_PauseMenu.SetActive(isPaused);
	}

}
