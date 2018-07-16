using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

	public Vector2 velocityCurrent;

	public Vector2 desiredPosition;
	public Vector2 restingPosition;

	float pixelUnit = (1f / 12f);

	private void Start () {
		restingPosition = transform.position;
		desiredPosition = restingPosition;

		//GameObject.FindGameObjectWithTag("Monster").GetComponent<MonsterController>().EventTeethSlam += AddScreenshake;
	}

	private void Update () {
		float deltaTimeClamped = Mathf.Max(Time.deltaTime, 0.00001f);

		Vector2 desiredVelocity = new Vector3(restingPosition.x, restingPosition.y) - transform.position;
		velocityCurrent = Vector2.Lerp(velocityCurrent, desiredVelocity / deltaTimeClamped, 5f * Time.deltaTime);
		
		// Move the camera
		desiredPosition += velocityCurrent * Time.deltaTime;

		// Position the camera
		transform.position = new Vector3(Mathf.Round(desiredPosition.x / pixelUnit) * pixelUnit, Mathf.Round(desiredPosition.y / pixelUnit) * pixelUnit, transform.position.z);
	}

	public void AddScreenshake (Vector2 velocity) {
		velocityCurrent += velocity;
	}

}
