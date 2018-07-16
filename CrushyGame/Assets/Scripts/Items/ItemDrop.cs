using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[RequireComponent (typeof(BoxCollider2D))]
public class ItemDrop : Entity {

	[Space (10)][Header ("LayerMasks")]
	public LayerMask collisionMask;

	[Space (10)][Header ("Item Drop Variables")]
	public Item item;
	public int coinValue;

	[Space (10)][Header ("Pickup Flash Settings")]
	public GameObject pickupFlashPrefab;
	public Color pickupFlashColor = new Color(0.968f, 0.952f, 0.945f, 1f);

	[Space (10)][Header ("Movement")]
	public AudioClip clip;
	public AudioManager audioManager;

	[Space (10)][Header ("Movement")]
	public BoxCollider2D collider;
	public Vector2 velocity;
	float skinWidth = 0.01f;
	int horizontalRaycasts = 8;
	int verticalRaycasts = 4;

	GameManager gameManager;
	MonsterController monster;
	
	private void Start () {
		// Setup references
		gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
		audioManager = GameObject.FindGameObjectWithTag("AudioManager").GetComponent<AudioManager>();
		monster = GameObject.FindGameObjectWithTag("Monster").GetComponent<MonsterController>();
		collider = GetComponent<BoxCollider2D>();
	}

	private void OnTriggerEnter2D(Collider2D col) {
		if (isDead == false) {
			Debug.Log("TRIGGERED " + col.name);
			// Create pickupFlash
			GameObject newPickupFlash = Instantiate(pickupFlashPrefab, transform.position, Quaternion.identity);
			newPickupFlash.GetComponent<SpriteRenderer>().color = pickupFlashColor;

			// If has coin value, give player coin
			if (coinValue > 0) {
				gameManager.OnCoinGrabbed(coinValue);
			}

			// If is a collectable item, collect it
			gameManager.CollectItem(item);

			// Play sound
			if (clip != null) {
				audioManager.PlayClipAtPoint(clip, transform.position, 0.7f, 1f);
			}

			// Die
			Die();
		}
	}

	private void Update () {
		UpdateMovement();
	}

	public void UpdateMovement () {
		// Decelerate naturally
		velocity = Vector2.Lerp(velocity, Vector2.zero, 0.25f * Time.deltaTime);

		// Move dropped item horizontally
		float hitDistanceH = Mathf.Infinity;
		float dx = (velocity.x > 0 ? 1 : -1);
		for (int i = 0; i < horizontalRaycasts; i++) {
			float colliderYSmaller = collider.size.y - (skinWidth / 2);
			float raycastIncrement = colliderYSmaller / (float)(horizontalRaycasts - 1);       // Gets the increment between each raycast
			Vector2 origin = (Vector2)transform.position + new Vector2(((collider.size.x / 2) - skinWidth) * dx, -(colliderYSmaller / 2) + i * raycastIncrement) + collider.offset;
			Vector2 direction = Vector2.right * dx;
			//Debug.DrawRay(origin, direction, Color.red, 0);
			RaycastHit2D hit = Physics2D.Raycast(origin, direction, Mathf.Abs(velocity.x * Time.deltaTime) + skinWidth, collisionMask);

			if (hit.transform != null) {
				if (hit.distance - skinWidth < hitDistanceH) {
					hitDistanceH = hit.distance - skinWidth;
				}
			}
		}

		if (hitDistanceH != Mathf.Infinity) {
			transform.position += (Vector3)new Vector2(hitDistanceH * dx, 0);
			velocity.x = -velocity.x * 0.8f;
		} else {
			transform.position += (Vector3)new Vector2(velocity.x, 0) * Time.deltaTime;
		}

		// Move dropped item vertically
		float hitDistanceV = Mathf.Infinity;
		float dy = (velocity.y > 0 ? 1 : -1);
		for (int i = 0; i < verticalRaycasts; i++) {
			float colliderXSmaller = collider.size.x - (skinWidth / 2);
			float raycastIncrement = colliderXSmaller / (float)(verticalRaycasts - 1);       // Gets the increment between each raycast
			Vector2 origin = (Vector2)transform.position + new Vector2(-(colliderXSmaller / 2) + i * raycastIncrement, ((collider.size.y / 2) - skinWidth) * dy) + collider.offset;
			Vector2 direction = Vector2.up * dy;
			//Debug.DrawRay(origin, direction, Color.blue, 0);
			
			RaycastHit2D hit = Physics2D.Raycast(origin, direction, Mathf.Abs(velocity.y * Time.deltaTime) + skinWidth, collisionMask);

			if (hit.transform != null) {
				if (hit.distance - skinWidth < hitDistanceV) {
					hitDistanceV = hit.distance - skinWidth;
				}
			}
		}

		if (hitDistanceV != Mathf.Infinity) {
			transform.position += (Vector3)new Vector2(0, hitDistanceV * dy);
			velocity.y = -velocity.y * 0.8f;
		} else {
			transform.position += (Vector3)new Vector2(0, velocity.y) * Time.deltaTime;
		}
	}

	public override void OnDie () {
		Destroy(gameObject);
	}

	public override void OnRevive () {

	}

}
