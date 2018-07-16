using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour {

	[Space (10)][Header ("LayerMasks")]
	public LayerMask collisionMask;
	public TrailRenderer trailRenderer;
	
	[Space (10)][Header ("Attributes")]
	public float deceleration;      // The value used to decelerate the projectile over time\
	public float bounciness;        // The bounciness coefficient of the projectile
	public int ricochetCount;       // The number of ricochets the projectile has left
	public float lifespan;          // The lifespan of the projectile

	Vector2 velocity;        // The current velocity of the projectile
	bool isBroken = false;

	private void Start () {
		trailRenderer = GetComponent<TrailRenderer>();
	}

	private void Update () {
		if (isBroken == false) {        // Is the projetile already broken?
			UpdateMovement();
			UpdateLifespan();
		}
	}

	private void UpdateMovement() {
		// Setup raycast
		Vector2 origin = transform.position;
		Vector2 deltaMove = velocity * Time.deltaTime;
		RaycastHit2D hit = Physics2D.Raycast(origin, deltaMove, deltaMove.magnitude, collisionMask);

		if (hit.transform != null) {
			if (ricochetCount > 0) {
				velocity = Vector2.Reflect(velocity * bounciness, hit.normal);          // Bounce projectile off of surface
				velocity = Quaternion.Euler(0, 0, Random.Range(-10f, 10f)) * velocity;	// Give velocity a randomized rotation offset from bouncing
				transform.position = (Vector3)hit.point + (Vector3)(velocity.normalized * 0.0025f) + new Vector3(0, 0, -0.5f);			// Move projectile towards where it hit
			} else {
				StartCoroutine(BreakProjectile());              // Break the projectile
			}
		} else {
			transform.position += (Vector3)deltaMove;           // Didn't hit anything? Move projectile forward
		}

		// Deceleration
		velocity = velocity.normalized * Mathf.Lerp(velocity.magnitude, 0, deceleration * Time.deltaTime);

		// Velocity too low breaking
		if (velocity.magnitude < 2.5f) {
			StartCoroutine(BreakProjectile());              // Break the projectile
		}
	}

	private void UpdateLifespan () {
		// Lifespan projectile breaking
		lifespan = Mathf.Clamp(lifespan - Time.deltaTime, 0, Mathf.Infinity);		// Reduce lifespan over time
		if (lifespan < 0) {
			StartCoroutine(BreakProjectile());              // Break the projectile
		}
	}

	private IEnumerator BreakProjectile () {
		if (isBroken == false) {
			isBroken = true;

			if (trailRenderer != null) {	// If there is a trail renderer, pause so it can catch up, then destroy gameObject
				yield return new WaitForSeconds(trailRenderer.time);
			}

			Destroy(gameObject);
		}
	}

	public void SetupProjectile (Vector2 initialVelocity) {
		velocity = initialVelocity;
	}

}
