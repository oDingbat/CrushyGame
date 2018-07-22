using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class RagdollWizard : MonoBehaviour {

	public bool createRagdoll;
	public string ragdollName;

	private void OnDrawGizmosSelected () {
		if (createRagdoll == true) {
			createRagdoll = false;
			Debug.Log("Creating Ragdoll");

			transform.name = "Ragdoll (" + ragdollName + ")";

			// Load ragdoll sprites
			Sprite[] characterSpreadsheetSprites = Resources.LoadAll<Sprite>("Art/Characters/CharacterSpreadsheet");

			foreach (Transform ragdollPiece in transform) {
				string pieceName = ("Character_" + ragdollName + "_" + ragdollPiece.name);
				ragdollPiece.GetComponent<SpriteRenderer>().sprite = characterSpreadsheetSprites.Single(s => s.name == pieceName);
			}


		}
	}


}
