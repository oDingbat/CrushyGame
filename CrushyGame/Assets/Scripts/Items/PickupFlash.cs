using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupFlash : MonoBehaviour {

	float transitionSpeed = 6.75f;

	void Update () {
		transform.position += new Vector3(0, 3f) * Time.deltaTime * transitionSpeed;
		transform.localScale = new Vector3(1, Mathf.Clamp01(transform.localScale.y - Time.deltaTime * transitionSpeed), 1);

		if (transform.localScale.y == 0) {
			Destroy(gameObject);
		}
	}

}
