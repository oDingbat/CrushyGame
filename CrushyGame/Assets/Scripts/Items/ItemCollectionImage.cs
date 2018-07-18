using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemCollectionImage : MonoBehaviour {
	
	public SpriteRenderer spriteRenderer;
	Vector2 desiredPos;

	bool doneMoving = false;

	private void Start () {
		// Setup references
		spriteRenderer = GetComponent<SpriteRenderer>();

		// Find desired position
		desiredPos = transform.localPosition + new Vector3(-3, 0);
	}

	private void FixedUpdate () {
		if (doneMoving == false) {
			// Lerp position towards desired position
			transform.localPosition = Vector3.Lerp(transform.localPosition, desiredPos, 2.5f * Time.fixedDeltaTime);

			if (Vector2.Distance(transform.localPosition, desiredPos) < 0.05f) {    // Stop moving if we're close enough
				transform.localPosition = desiredPos;
				doneMoving = true;
			}
		}

		// Hide the sprite if it is too low
		spriteRenderer.enabled = (transform.position.y < -2f ? false : true);
	}
	
}
