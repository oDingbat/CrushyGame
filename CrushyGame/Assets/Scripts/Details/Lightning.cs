using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof(TrailRenderer))]
public class Lightning : MonoBehaviour {

	public Transform target;
	public TrailRenderer trailRenderer;

	public float stepDistance;
	public float stepRandomness;
	public float stepAngle;

	Vector2 targetLastPos;

	private void Start () {
		trailRenderer = GetComponent<TrailRenderer>();

		StartCoroutine(StartLightning());
	}

	private IEnumerator StartLightning () {

		yield return new WaitForSeconds(trailRenderer.time);

		while (target != null) {
			targetLastPos = target.position;
			
			if (Vector2.Distance(transform.position, new Vector3(target.position.x, target.position.y, -0.5f)) < stepDistance * 0.825f) {
				transform.position = new Vector3(target.position.x, target.position.y, -0.5f);
				break;
			} else {
				Vector2 targetDirection = new Vector3(target.position.x, target.position.y, -0.5f) - transform.position;

				Quaternion randomRotation = Quaternion.Euler(0, 0, Random.Range(-stepAngle / 2f, stepAngle / 2f));
				Vector2 randomMovementDelta = randomRotation * targetDirection.normalized * (stepDistance + Random.Range(-stepRandomness, stepRandomness));

				transform.position += (Vector3)randomMovementDelta;
			}

			yield return new WaitForSeconds(trailRenderer.time / 16f);
		}
		
		transform.position = (Vector3)targetLastPos + new Vector3(0, 0, -0.5f);

		yield return new WaitForSeconds(trailRenderer.time);

		Destroy(gameObject);
	}

}
