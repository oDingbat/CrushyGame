﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeartImage : MonoBehaviour {

	[Space (10)][Header ("References")]
	public PlayerController player;
	public GameManager gameManager;

	public int heartIndex;							// Index of the heart image

	[Space (10)][Header ("Prefabs")]
	public GameObject prefab_BurstParticle;			// Prefab for the burstParticles which are created when the Burst function is called

	SpriteRenderer heartBodySpriteRenderer;         // SpriteRenderer for the heart's body

	Vector2 desiredPos;

	private void Start() {
		// Setup references
		gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
		heartBodySpriteRenderer = transform.Find("HeartBody").GetComponent<SpriteRenderer>();
		
		desiredPos = transform.localPosition + new Vector3(3, 0);

		player.EventLostHeart += Burst;
	}

	private void Update() {
		// Lerp the position of the image
		float verticalPos = heartIndex * -0.75f;		// Vertical pos, allows the hearts to move vertically if a new heart is inserted in between them
		transform.localPosition = Vector3.Lerp(transform.localPosition, new Vector3(desiredPos.x, verticalPos), 2.5f * Time.deltaTime);
		
		heartBodySpriteRenderer.enabled = (transform.position.y < -2f ? false : true);
	}

	public void Burst (int index) {
		if (index == heartIndex) {
			// Destroys the heart
			heartBodySpriteRenderer.enabled = false;

			// Create Burst Particles
			int particleCount = 12;
			for (int i = 0; i < particleCount; i++) {
				float angle = 360f / (float)particleCount;
				float subAngle = angle * 0.25f;

				Vector2 newDirection = Quaternion.Euler(0, 0, (angle * (float)i) + Random.Range(-subAngle, subAngle)) * Vector2.right;

				GameObject newParticle = Instantiate(prefab_BurstParticle, transform.position + new Vector3(0, 0, -0.75f) + ((Vector3)newDirection.normalized * 0.125f), Quaternion.identity);
				Projectile newParticleProjectile = newParticle.GetComponent<Projectile>();
				newParticleProjectile.SetupProjectile(newDirection * Random.Range(15f, 20f));
			}
			
			player.EventLostHeart -= Burst;		// Unsubscribe from the player's event
			gameManager.heartImages.Remove(this);
			Destroy(gameObject);
		}
	}
}
