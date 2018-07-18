using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextButton : DynamicText {

	[Space (10)][Header ("References")]
	public PlayerController player;

	[Space (10)][Header ("Neighboring Text Button")]
	public TextButton buttonNext;
	public TextButton buttonPrevious;

	[Space (10)][Header ("Selection Settings")]
	public Color colorSelected;
	public Color colorDeselected;
	bool isSelected = false;

	private void Start () {
		textContainer = transform.Find("[TextContainer]");
		player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
		SetText(text);
		SetTextColor();
	}

	public void ToggleSelect () {
		isSelected = !isSelected;
		SetTextColor();
	}

	private void SetTextColor () {
		foreach (Transform letter in textContainer) {
			letter.GetComponent<SpriteRenderer>().color = (isSelected == true ? colorSelected : colorDeselected);
		}
	}

	private void Update () {
		if (isSelected) {

			bool pressingNext = Input.GetKeyDown(player.controlScheme.right) || Input.GetKeyDown(player.controlScheme.rightAlt) || Input.GetKeyDown(player.controlScheme.down) || Input.GetKeyDown(player.controlScheme.downAlt) || Input.GetKeyDown(player.controlScheme.down) || Input.GetKeyDown(player.controlScheme.down) || (Input.GetAxis("G_Horizontal") > 0.01f ? true : false) || (Input.GetAxis("G_Vertical") < -0.01f ? true : false);
			if (pressingNext == true) {
				ToggleSelect();
				buttonNext.ToggleSelect();
			} else {
				bool pressingPrevious = Input.GetKeyDown(player.controlScheme.left) || Input.GetKeyDown(player.controlScheme.leftAlt) || Input.GetKeyDown(player.controlScheme.up) || Input.GetKeyDown(player.controlScheme.upAlt) || (Input.GetAxis("G_Horizontal") < -0.01f ? true : false) || (Input.GetAxis("G_Vertical") > 0.01f ? true : false);
				if (pressingNext == true) {
					ToggleSelect();
					buttonPrevious.ToggleSelect();
				}
			}

		}
	}

}
