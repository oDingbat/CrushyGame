using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;
using System.Linq;

public class PlayerController : Entity {
	
	[Space(10)][Header ("LayerMasks")]
	public LayerMask environmentMask;
	public LayerMask environmentPlatformMask;

	[Space(10)]
	[Header("Control Settings")]
	public ControlScheme controlScheme;
	public ControlScheme.ControlSchemePreset controlSchemePreset;

	[Space(10)][Header ("References")] 
	public Camera camera;						
	public BoxCollider2D collider;							// The collider for the player
	public Rigidbody2D playerPhysicsSimulator;				// The physics capsule which interacts with ragdolls and other physics details
	public Animator spriteAnimator;                         // The spriteAnimator responsible for the animations of the player's character
	public SpriteRenderer spriteRenderer;
	public GameManager gameManager;
	public MonsterController monster;
	public AudioManager audioManager;
	public Transform[] touchButtons;

	[Space (10)][Header ("Attributes")]
	public Attributes attributesBase;
	public Attributes attributesCombined;

	public GameObject corpse;

	[Space (10)][Header ("Audio Settings")]
	public AudioClip clip_footstep;
	public AudioClip clip_airjump;
	public AudioClip clip_die;
	public AudioClip clip_lightning;
	public bool clipJustPlayed_footstep;
	public List<Sprite> spriteAudioQueue_footstep;

	[Space (10)][Header ("Magnetism")]
	public List<MagnetizedItemDrop> magnetizedItems = new List<MagnetizedItemDrop>();

	// Raycast stuff
	int horizontalRaycasts = 6;
	int verticalRaycasts = 3;
	float skinWidth = 0.01f;

	[Space(10)] [Header("Character Settings")]
	public Color characterColor;
	public int characterIndex;
	
	public float inputMovement;
	public Vector2 velocity;
	float speed = 10;
	float jumpForce = 19.25f;
	float timeLastJumped;
	public float timeLastPressedJump;
	float jumpForgiveness = 0.1f;
	public bool isEnabled;              // Is the playerController currently enabled?
	float touchingWallTestScale = 0.85f;
	int touchingWallRaycasts = 5;
	int airJumpsLeft;
	int deathCount;
	
	public bool grounded = false;
	public bool touchingWall = false;
	public float touchingWallDirection;
	public Vector2 desiredPosition;

	public GameObject prefab_Lightning;
	public GameObject prefab_airJumpParticle;

	[System.Serializable]
	public class MagnetizedItemDrop {
		public ItemDrop itemDrop;
		public float lightningCooldown;

		public MagnetizedItemDrop (ItemDrop _itemDrop) {
			itemDrop = _itemDrop;
			lightningCooldown = 0f;
		}
	}

	// Events
	public event Action<PlayerController> EventChangeCharacter;
	public event Action<int> EventLostHeart;

	private void Start () {
		// Setup references
		collider = GetComponent<BoxCollider2D>();
		spriteAnimator = transform.Find("Animator").GetComponent<Animator>();
		gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
		spriteRenderer = spriteAnimator.transform.GetComponent<SpriteRenderer>();
		audioManager = GameObject.FindGameObjectWithTag("AudioManager").GetComponent<AudioManager>();

		// Setup physics simulator
		playerPhysicsSimulator = transform.Find("(PhysicsSimulator)").GetComponent<Rigidbody2D>();
		playerPhysicsSimulator.GetComponent<CapsuleCollider2D>().offset = collider.offset;
		playerPhysicsSimulator.GetComponent<CapsuleCollider2D>().size = new Vector2(collider.size.x, collider.size.y - 0.5f);
		playerPhysicsSimulator.transform.parent = transform.parent;

		// Set control scheme
		if (controlSchemePreset != ControlScheme.ControlSchemePreset.Null) {
			controlScheme = new ControlScheme(controlSchemePreset);
		}

		// Set combinedAttributes
		attributesCombined = new Attributes(attributesBase);
	}

	private void Update () {
		UpdateInput();
		UpdateUI();

		if (isDead == false) {
			UpdateMovement();
			UpdateAnimator();
			UpdateAudio();
			UpdatePhysicsSimulator();

			UpdateKillzone();
		} else {
			if (corpse != null) {
				transform.position = corpse.transform.Find("Ragdoll_Head").position;
			}

			if (attributesBase.hearts <= 0 && Input.GetKeyDown(controlScheme.jump)) {
				SceneManager.LoadScene(0);
			}
		}
	}

	private void FixedUpdate () {
		UpdateMagnetism();
	}

	private void UpdateInput() {
		if (isEnabled) {
			// Movement
			List<Touch> touches = Input.touches.ToList();
			Vector2 screenRes = new Vector2(Screen.width, Screen.height);
			List<Transform> touchedButtons = new List<Transform>();

			// Get touch buttons
			foreach (Touch t in touches) {
				Transform touchedButton = null;
				float closestDistance = Mathf.Infinity;

				Vector2 tPos = camera.ScreenToWorldPoint(t.position);

				// See if touch t is touching any buttons
				foreach (Transform b in touchButtons) {
					float currentDist = Vector2.Distance(tPos, b.position);
					if (currentDist < 2f && currentDist < closestDistance) {
						touchedButton = b;
						closestDistance = currentDist;
					}
				}

				// If we touched a button, add it to the list
				if (touchedButton != null && touchedButtons.Contains(touchedButton) == false) {
					touchedButtons.Add(touchedButton);
				}
			}

			bool left = Input.GetKey(controlScheme.left) || Input.GetKey(controlScheme.leftAlt) || (Input.GetAxis("G_Horizontal") < -0.01f ? true : false) || touchedButtons.Exists(b => b.name == "(Arrow Left)");
			bool right = Input.GetKey(controlScheme.right) || Input.GetKey(controlScheme.rightAlt) || (Input.GetAxis("G_Horizontal") > 0.01f ? true : false) || touchedButtons.Exists(b => b.name == "(Arrow Right)");

			inputMovement = (left != right) ? (left ? -1 : 1) : 0;

			// Jumping
			bool jump = Input.GetKeyDown(controlScheme.jump) || Input.GetKeyDown(controlScheme.jumpAlt) || Input.GetButtonDown("G_Jump") ? true : false || touchedButtons.Exists(b => b.name == "(Arrow Up)");
			if (jump) {
				timeLastPressedJump = Time.time;
			}

			// Platform Dropping
			bool down = Input.GetKeyDown(controlScheme.down) || Input.GetKeyDown(controlScheme.downAlt) || (Input.GetAxis("G_Vertical") < -0.01f ? true : false) || touchedButtons.Exists(b => b.name == "(Arrow Down)");
			if (down) {
				AttemptDropDown();
			}

			if (jump && attributesCombined.hearts <= 0) {
				SceneManager.LoadScene(0);
			}
		}

		// Character Swap
		if (Input.GetKeyDown(controlScheme.characterSwap)) {
			EventChangeCharacter(this);
		}

		// Suiciding
		if (Input.GetKeyDown(controlScheme.suicide) || Input.GetButtonDown("G_Suicide") ? true : false) {
			if (isDead == false) {
				Die();
			}
		}
	}

	private void UpdateUI () {

	}

	private void UpdateMovement () {
		velocity = Vector2.Lerp(velocity, new Vector2(inputMovement * (float)attributesCombined.speed, velocity.y), (grounded == true ? (inputMovement == 0 ? (float)attributesCombined.deceleration * 2f : (float)attributesCombined.acceleration * 0.625f) : (inputMovement == 0 ? (float)attributesCombined.deceleration * 1.25f : (float)attributesCombined.acceleration * 0.625f)) * Time.deltaTime);

		// Jumping
		if (timeLastJumped + 0.025f < Time.time && timeLastPressedJump + jumpForgiveness > Time.time) { // Make sure we didnt just jump, give jumping a little wiggle room
			if (grounded == true) {
				timeLastPressedJump = 0;
				velocity.y = jumpForce;
				timeLastJumped = Time.time;
				grounded = false;
				audioManager.PlayClipAtPoint(clip_footstep, transform.position, 0.5f, 1f);
			} else if (touchingWall == true) {
				timeLastPressedJump = 0;
				velocity = new Vector2(-touchingWallDirection * 0.5f, 1.5f).normalized * jumpForce * 1.125f;
				timeLastJumped = Time.time;
				audioManager.PlayClipAtPoint(clip_footstep, transform.position, 0.5f, 1f);
			} else if (airJumpsLeft > 0) {       // Feather jumping
				airJumpsLeft--;
				timeLastPressedJump = 0;
				velocity.y = Mathf.Clamp(jumpForce * 0.8f, velocity.y, jumpForce);
				timeLastJumped = Time.time;
				grounded = false;

				audioManager.PlayClipAtPoint(clip_airjump, transform.position, 0.75f, 1.75f);

				// Airjump Fx
				int airJumpParticleCount = 8;
				float spreadAngle = 52.5f;
				float angleIncrement = spreadAngle / (float)airJumpParticleCount;
				for (int i = 0; i < airJumpParticleCount; i++) {
					Projectile newAirJumpParticle = Instantiate(prefab_airJumpParticle, transform.position + (Vector3)collider.offset + new Vector3(0, -collider.size.y / 2) + new Vector3(0, 0, -0.5f), Quaternion.identity).GetComponent<Projectile>();

					float angleCurrent = angleIncrement * i;
					Vector2 randomVelocity = Quaternion.Euler(0, 0, (-spreadAngle / 2) + angleCurrent + UnityEngine.Random.Range(-2.5f, 2.5f)) * new Vector2(0, UnityEngine.Random.Range(-45f, -35));

					newAirJumpParticle.SetupProjectile(randomVelocity);
				}
			}
		}

		if (grounded == false && touchingWall == true && inputMovement == touchingWallDirection) {        // Climbing
			float climbingCoefficient = Mathf.Clamp(attributesCombined.climbing, 0, 10) * 6.5f;

			if (velocity.y <= 0) {
				velocity += new Vector2(0, (-65f + climbingCoefficient) * Time.deltaTime);     // Apply gravity
			} else {
				velocity += new Vector2(0, -65f * Time.deltaTime);     // Apply gravity
				
			}

			velocity.y = Mathf.Lerp(velocity.y, Mathf.Clamp(velocity.y, (-65f + climbingCoefficient), Mathf.Infinity), 10 * Time.deltaTime);
		} else {
			velocity += new Vector2(0, -65f * Time.deltaTime);     // Apply gravity
		}

		// Move player horizontally
		float hitDistanceH = Mathf.Infinity;
		float dx = (velocity.x > 0 ? 1 : -1);
		for (int i = 0; i < horizontalRaycasts; i++) {
			float colliderYSmaller = collider.size.y - (skinWidth / 2);
			float raycastIncrement = colliderYSmaller / (float)(horizontalRaycasts - 1);       // Gets the increment between each raycast
			Vector2 origin = (Vector2)transform.position + new Vector2(((collider.size.x / 2) - skinWidth) * dx, -(colliderYSmaller / 2) + i * raycastIncrement) + collider.offset;
			Vector2 direction = Vector2.right * dx;
			//Debug.DrawRay(origin, direction, Color.red, 0);
			RaycastHit2D hit = Physics2D.Raycast(origin, direction, Mathf.Abs(velocity.x * Time.deltaTime) + skinWidth, environmentMask);

			if (hit.transform != null) {
				if (hit.distance - skinWidth < hitDistanceH) {
					hitDistanceH = hit.distance - skinWidth;
				}
			}
		}

		if (hitDistanceH != Mathf.Infinity) {
			transform.position += (Vector3)new Vector2(hitDistanceH * dx, 0);
				velocity.x = 0;
		} else {
			transform.position += (Vector3)new Vector2(velocity.x, 0) * Time.deltaTime;
		}

		// Move player vertically
		float hitDistanceV = Mathf.Infinity;
		float dy = (velocity.y > 0 ? 1 : -1);
		for (int i = 0; i < verticalRaycasts; i++) {
			float colliderXSmaller = collider.size.x - (skinWidth / 2);
			float raycastIncrement = colliderXSmaller / (float)(verticalRaycasts - 1);       // Gets the increment between each raycast
			Vector2 origin = (Vector2)transform.position + new Vector2(-(colliderXSmaller / 2) + i * raycastIncrement, ((collider.size.y / 2) - skinWidth) * dy) + collider.offset;
			Vector2 direction = Vector2.up * dy;
			//Debug.DrawRay(origin, direction, Color.blue, 0);

			LayerMask chosenMask = ((dy == -1 && !Input.GetKey(controlScheme.down)) ? environmentPlatformMask : environmentMask);
			RaycastHit2D hit = Physics2D.Raycast(origin, direction, Mathf.Abs(velocity.y * Time.deltaTime) + skinWidth, chosenMask);

			if (hit.transform != null) {
				if (hit.distance - skinWidth < hitDistanceV) {
					hitDistanceV = hit.distance - skinWidth;
				}
			}
		}

		if (hitDistanceV != Mathf.Infinity) {
			transform.position += (Vector3)new Vector2(0, hitDistanceV * dy);
			if (velocity.y < 0) {
				if (grounded == false) {
					audioManager.PlayClipAtPoint(clip_footstep, transform.position, 0.5f, 1f);
				}
				grounded = true;
				airJumpsLeft = attributesCombined.airJumps;
			}
			velocity.y = 0;
		} else {
			grounded = false;
			transform.position += (Vector3)new Vector2(0, velocity.y) * Time.deltaTime;
		}

		// Get touching wall information left
		bool touchingWallThisFrame = false;
		for (int d = -1; d <= 1; d += 2) {
			for (int i = 0; i < touchingWallRaycasts; i++) {
				float colliderYScaled = collider.size.y * touchingWallTestScale;
				float colliderYInitial = -(colliderYScaled / 2);
				float raycastIncrement = colliderYScaled / (float)(touchingWallRaycasts - 1);       // Gets the increment between each raycast
				Vector2 origin = (Vector2)transform.position + new Vector2(((collider.size.x / 2) - skinWidth) * d, colliderYInitial + i * raycastIncrement) + collider.offset;
				Vector2 direction = Vector2.right * d;
				//Debug.DrawRay(origin, direction, Color.green, 0);
				RaycastHit2D hit = Physics2D.Raycast(origin, direction, 0.025f + skinWidth, environmentMask);

				if (hit.transform != null) {
					touchingWallThisFrame = true;
					touchingWallDirection = d;
				}
			}
		}
		
		if (touchingWallThisFrame == true) {
			touchingWall = true;
		} else {
			touchingWallDirection = 0;
			touchingWall = false;
		}
	}

	private void UpdateKillzone() {
		// If the player leaves the stage too far they will be killed by the killzone
		if (transform.position.y > 25 || transform.position.y < -15 || transform.position.x > 25 || transform.position.x < -25) {
			Die();
		}
	}

	private void UpdateMagnetism () {
		bool hasMagnetism = attributesCombined.magnetism > 0;

		if (hasMagnetism == true) {
			List<GameObject> items = monster.itemDrops.Select(i => i.gameObject).ToList();

			float magnetismDistance = Mathf.Sqrt(attributesCombined.magnetism) * 2f;

			// Fetch magnetizable items
			foreach (GameObject item in items) {
				ItemDrop itemDropClass = item.GetComponent<ItemDrop>();

				if (itemDropClass && magnetizedItems.Exists(m => m.itemDrop == itemDropClass) == false) {		// Is there an itemDropClass we aren't already magnetizing
					if (Vector2.Distance(item.transform.position, transform.position) < magnetismDistance) {
						magnetizedItems.Add(new MagnetizedItemDrop(itemDropClass));
					}
				}
			}

			// Adjusting magnetized items
			for (int i = 0; i < magnetizedItems.Count; i++) {
				MagnetizedItemDrop magnetizedItem = magnetizedItems[i];

				// Pulling
				if (magnetizedItem.itemDrop == null || Vector2.Distance(magnetizedItem.itemDrop.transform.position, transform.position) > magnetismDistance) {
					magnetizedItems.RemoveAt(i);
					i--;
				} else {
					Vector3 direction = transform.position - magnetizedItem.itemDrop.transform.position;
					direction = direction.normalized * Mathf.Pow(1 / direction.magnitude, 2f) * 8f;
					magnetizedItem.itemDrop.velocity = Vector2.Lerp(magnetizedItem.itemDrop.velocity, direction * Mathf.Sqrt(attributesCombined.magnetism) * 0.75f, 10 * Time.fixedDeltaTime);
				}

				// Lightning
				if (magnetizedItem.itemDrop != null) {
					float itemDistance = Mathf.Clamp(Vector2.Distance(transform.position, magnetizedItem.itemDrop.transform.position), 0.25f, 10);
					magnetizedItem.lightningCooldown = Mathf.Clamp(magnetizedItem.lightningCooldown - Mathf.Pow(10f / itemDistance, 1.25f) * Mathf.Sqrt(attributesCombined.magnetism) * Time.fixedDeltaTime, 0, Mathf.Infinity);
					if (magnetizedItem.lightningCooldown <= 0) {
						// Add cooldown
						magnetizedItem.lightningCooldown += UnityEngine.Random.Range(0.25f, 1.5f);

						// Create lightning
						Lightning newLightning = Instantiate(prefab_Lightning, transform.position + new Vector3(0, 0, -0.5f), Quaternion.identity).GetComponent<Lightning>();
						newLightning.target = magnetizedItem.itemDrop.transform;

						// Play clip
						audioManager.PlayClipAtPoint(clip_lightning, transform.position, 0.1f, 1.125f, 0.5f);
					}
				}
			}
		}
	}

	public bool CheckIfGroundIsPlatform (float deltaDownwards) {
		bool isPlatform = true;
		for (int i = 0; i < verticalRaycasts; i++) {
			float colliderXSmaller = collider.size.x - (skinWidth / 2);
			float raycastIncrement = colliderXSmaller / (float)(verticalRaycasts - 1);       // Gets the increment between each raycast
			Vector2 origin = (Vector2)transform.position + new Vector2(-(colliderXSmaller / 2) + i * raycastIncrement, ((collider.size.y / 2) - skinWidth) * -1) + collider.offset;
			Vector2 direction = Vector2.up * -1;
			
			RaycastHit2D hit = Physics2D.Raycast(origin, direction, Mathf.Abs(deltaDownwards) + skinWidth, environmentPlatformMask);

			if (hit.transform && hit.transform.gameObject.layer != LayerMask.NameToLayer("Platform")) {
				isPlatform = false;
			}
		}

		return isPlatform;
	}

	public void AttemptDropDown () {
		if (grounded == true) {
			float deltaDownwards = -0.125f;
			bool groudIsPlatform = CheckIfGroundIsPlatform(deltaDownwards);

			if (groudIsPlatform == true) {
				transform.position += new Vector3(0, deltaDownwards);
			}
		}
	}

	public void PullPlayer (Vector2 deltaP) {
		// Move player vertically
		float hitDistanceV = Mathf.Infinity;
		float dy = (deltaP.y > 0 ? 1 : -1);
		for (int i = 0; i < verticalRaycasts; i++) {
			float colliderXSmaller = collider.size.x - (skinWidth / 2);
			float raycastIncrement = colliderXSmaller / (float)(verticalRaycasts - 1);       // Gets the increment between each raycast
			Vector2 origin = (Vector2)transform.position + new Vector2(-(colliderXSmaller / 2) + i * raycastIncrement, ((collider.size.y / 2) - skinWidth) * dy) + collider.offset;
			Vector2 direction = Vector2.up * dy;
			//Debug.DrawRay(origin, direction, Color.blue, 0);

			LayerMask chosenMask = ((dy == -1 && !Input.GetKey(controlScheme.down)) ? environmentPlatformMask : environmentMask);
			RaycastHit2D hit = Physics2D.Raycast(origin, direction, Mathf.Abs(deltaP.y) + skinWidth, chosenMask);

			if (hit.transform != null) {
				if (hit.distance - skinWidth < hitDistanceV) {
					hitDistanceV = hit.distance - skinWidth;
				}
			}
		}

		if (hitDistanceV != Mathf.Infinity) {
			transform.position += (Vector3)new Vector2(0, hitDistanceV * dy);
			if (deltaP.y < 0) {
				audioManager.PlayClipAtPoint(clip_footstep, transform.position, 0.5f, 1f);
				grounded = true;
				airJumpsLeft = attributesCombined.airJumps;
			}
			deltaP.y = 0;
		} else {
			grounded = false;
			transform.position += (Vector3)new Vector2(0, deltaP.y);
		}
	}

	private void UpdateAnimator () {
		string characterName = spriteAnimator.runtimeAnimatorController.name;
		Debug.Log(characterName);

		if (spriteAnimator.enabled == true) {
			if (grounded == true) {
				if (touchingWall == true) {
					spriteAnimator.speed = 1;
					spriteAnimator.transform.localScale = new Vector3(Mathf.Sign(-touchingWallDirection), 1, 1);
				} else {
					spriteAnimator.speed = 1;
					spriteAnimator.transform.localScale = new Vector3(Mathf.Sign(velocity.x), 1, 1);
				}
				
				if (Mathf.Abs(velocity.x) < 0.125f) {
					spriteAnimator.speed = 1;
					spriteAnimator.Play(characterName + "_Standing");
				} else {
					spriteAnimator.speed = Mathf.Abs(velocity.x) / 10 * 2.5f;
					spriteAnimator.Play(characterName + "_Walking");
				}
			} else {
				spriteAnimator.speed = 1;
				if (touchingWall == true) {
					spriteAnimator.transform.localScale = new Vector3(-touchingWallDirection, 1, 1);
					spriteAnimator.Play(characterName + "_WallSliding");
				} else {
					spriteAnimator.transform.localScale = new Vector3(Mathf.Sign(velocity.x), 1, 1);
					if (velocity.y > 0.25f) {
						spriteAnimator.Play(characterName + "_Jumping");
					} else {
						spriteAnimator.Play(characterName + "_Falling");
					}
				}
			}
		}

	}
 
	private void UpdateAudio () {
		bool isFootstepQueue = false;
		foreach (Sprite spriteAudioQueue in spriteAudioQueue_footstep) {
			if (spriteAudioQueue == spriteRenderer.sprite) {
				isFootstepQueue = true;
			}
		}

		if (isFootstepQueue == false) {
			clipJustPlayed_footstep = false;
		}

		if (clipJustPlayed_footstep == false && isFootstepQueue == true) {
			clipJustPlayed_footstep = true;
			audioManager.PlayClipAtPoint(clip_footstep, transform.position, 0.5f, 1f);
		}
	}

	private void UpdatePhysicsSimulator () {
		playerPhysicsSimulator.velocity = (transform.position - playerPhysicsSimulator.transform.position) * 50f;
	}

	public override void OnDie() {
		// Hide the player and their colliders etc.
		spriteAnimator.gameObject.SetActive(false);
		playerPhysicsSimulator.gameObject.SetActive(false);
		collider.enabled = false;

		// Send out event for losing hearts
		if (EventLostHeart != null) {
			EventLostHeart(attributesCombined.hearts - 1);
		}

		// Remove one heart
		attributesCombined.hearts--;

		// Create ragdoll
		GameObject prefabRagdoll = Resources.Load<GameObject>("Prefabs/Ragdolls/Ragdoll (" + spriteAnimator.runtimeAnimatorController.name + ")");
		corpse = (GameObject)Instantiate(prefabRagdoll, transform.position + new Vector3(0, 0.05f, 0), Quaternion.identity);
		
		// Give the ragdoll velocity similar to that of the player's
		foreach (Transform bodyPart in corpse.transform) {
			Rigidbody2D bodyPartRigidbody = bodyPart.GetComponent<Rigidbody2D>();
			if (bodyPartRigidbody != null) {
				bodyPartRigidbody.velocity = velocity * 2.75f * (bodyPartRigidbody.mass / 50);
			}
		}

		deathCount++;   // Increment deathCounter

		// Play clip
		audioManager.PlayClipAtPoint(clip_die, transform.position, 1f, 1f);

		// If the player still has some hearts, revive them after a set delay
		if (attributesCombined.hearts > 0) {
			StartCoroutine(DelayedRevival());
		}
	}

	public IEnumerator DelayedRevival () {
		yield return new WaitForSeconds(1f + (2f / Mathf.Sqrt(monster.mouthSpeedCurrent)));

		// Make sure we wait if the monster is currently morphing
		while (monster.mouthPhase == MonsterController.MouthPhase.Morphing) {
			yield return new WaitForSeconds(0.05f);
		}
		
		// Destroy old corpse
		Destroy(corpse);

		// Position player
		transform.position = monster.teeth[monster.currentGap + monster.teethCount].position + new Vector3(0, 1.125f, 0);

		Revive();		// Revive the player
	}

	public override void OnRevive () {
		spriteAnimator.gameObject.SetActive(true);
		collider.enabled = true;
		playerPhysicsSimulator.gameObject.SetActive(true);
		
		velocity = Vector2.zero;
		
		StartCoroutine(DelayedRevivalFlicker(deathCount));
	}

	public IEnumerator DelayedRevivalFlicker (int deathCountCurrent) {
		// Flicker's the player's spriteRenderer representing the player has just respawned
		int flickerCount = 3;		// Number of times the player's renderer will flicker
		for (int i = 0; i < flickerCount; i++) {
			yield return new WaitForSeconds(0.075f);

			if (deathCountCurrent != deathCount) { break; }
			spriteAnimator.gameObject.SetActive(false);		// Flicker Off

			yield return new WaitForSeconds(0.075f);

			if (deathCountCurrent != deathCount) { break; }
			spriteAnimator.gameObject.SetActive(true);		// Flicker On
		}
	}

}
