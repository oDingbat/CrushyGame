using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextFloater : DynamicText {

	[Space(10)][Header ("Text Floater Settings")]
	public float speed = 7.5f;
	public float moveDistance = 4f;
	public float flashInterval = 0.075f;
	public int flashCount = 3;

	float startingHeight;
	bool doneMoving;

	private void Start () {
		startingHeight = transform.position.y;

		StartCoroutine(DelayedDeletion());
	}

	private IEnumerator DelayedDeletion () {
		while (doneMoving == false) {
			yield return new WaitForSeconds(0.1f);
		}

		speed = 0;

		yield return new WaitForSeconds(flashInterval * 2);

		for (int i = 0; i < flashCount; i++) {
			yield return new WaitForSeconds(flashInterval);

			textContainer.gameObject.SetActive(false);

			yield return new WaitForSeconds(flashInterval);

			textContainer.gameObject.SetActive(true);
		}

		yield return new WaitForSeconds(flashInterval);

		Destroy(gameObject);
	}

	private void Update () {
		if (transform.position.y >= startingHeight + moveDistance || transform.position.y > 9.5f) {
			doneMoving = true;
		}
		
		transform.position += new Vector3(0, speed * Time.deltaTime);       // Move the text floater upwards

		FixTextContainerPosition();
	}


}
