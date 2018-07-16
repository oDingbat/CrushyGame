using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemCollectionImage : MonoBehaviour {
	
	public SpriteRenderer spriteRenderer;
	Vector2 desiredPos;

	private void Start () {
		// Setup references
		spriteRenderer = GetComponent<SpriteRenderer>();

		// Find desired position
		desiredPos = transform.localPosition + new Vector3(-3, 0);
	}

	private void Update () {
		// Lerp position towards desired position
		transform.localPosition = Vector3.Lerp(transform.localPosition, desiredPos, 2.5f * Time.deltaTime);

		// Hide the sprite if it is too low
		spriteRenderer.enabled = (transform.position.y < -2f ? false : true);
	}
	
}
