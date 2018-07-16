using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PixelPerfector : MonoBehaviour {

	float p = 12f;		// pixels per unity

	void Update () {
		transform.position = new Vector3(Mathf.Round(transform.parent.position.x * p) / p, Mathf.Round(transform.parent.position.y * p) / p);
	}

	public void Activate () {
		transform.position = new Vector3(Mathf.Round(transform.parent.position.x * p) / p, Mathf.Round(transform.parent.position.y * p) / p);
	}

}
