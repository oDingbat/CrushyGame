using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ControlScheme {

	public KeyCode jump, jumpAlt;
	public KeyCode left, leftAlt;
	public KeyCode right, rightAlt;
	public KeyCode up, upAlt;
	public KeyCode down, downAlt;
	public KeyCode zoom, zoomAlt;
	public KeyCode suicide, suicideAlt;
	public KeyCode characterSwap, characterSwapAlt;

	public enum ControlSchemePreset { Null, WASD, Arrows, NumPad, Controller }

	public ControlScheme (ControlSchemePreset preset) {
		switch (preset) {
			case (ControlSchemePreset.WASD):
				jump = KeyCode.Space;
				left = KeyCode.A;
				right = KeyCode.D;
				up = KeyCode.W;
				down = KeyCode.S;
				suicide = KeyCode.K;
				zoom = KeyCode.Z;
				characterSwap = KeyCode.Q;

				// Alts
				leftAlt = KeyCode.LeftArrow;
				rightAlt = KeyCode.RightArrow;
				jumpAlt = KeyCode.UpArrow;
				downAlt = KeyCode.DownArrow;
				break;
			case (ControlSchemePreset.Arrows):
				jump = KeyCode.Return;
				left = KeyCode.LeftArrow;
				right = KeyCode.RightArrow;
				up = KeyCode.UpArrow;
				down = KeyCode.DownArrow;
				suicide = KeyCode.RightControl;
				characterSwap = KeyCode.RightShift;
				break;
			case (ControlSchemePreset.NumPad):
				jump = KeyCode.KeypadEnter;
				left = KeyCode.Keypad4;
				right = KeyCode.Keypad6;
				up = KeyCode.Keypad8;
				down = KeyCode.Keypad5;
				suicide = KeyCode.KeypadPeriod;
				characterSwap = KeyCode.Plus;
				break;
		}


	}

	
}
