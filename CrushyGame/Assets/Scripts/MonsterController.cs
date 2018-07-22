using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MonsterController : MonoBehaviour {

	[Space (10)][Header ("Monster Settings")]
	public string monsterName;                  // The name of the monster (Used to create the monster at the beginning via fetching prefabs)

	[Space (10)][Header ("References")]
	public PlayerController player;
	public AudioManager audioManager;

	[Space (10)][Header ("LayerMasks")]
	public LayerMask entityAndRagdollMask;		// LayerMask for both players and ragdolls
	public LayerMask entityMask;				// LayerMask for players
	public LayerMask environmentMask;           // LayerMask for environment
	public LayerMask environmentPlatformMask;	// LayerMask with environment and platforms

	[Space (10)][Header ("Mouth Settings")]
	public float mouthSpeedCurrent;             // The current speed of the mouth
	public float mouthSpeedIncrement;           // The amount the mouthSpeedCurrent increments per cycle
	public int teethCount;                      // The number of teeth (only counting 1 row) ie: teethCount 10 = 10 teeth on top and another 10 on bottom
	public int teethCountMax;                   // The maximum number of teeth allowed
	public int teethHeightPositions;            // The number of height positions for the teeth. 3 = teeth can be at 3 different possible heights, all multiples of teethHeightInterval
	public float teethHeightInterval;           // The distance between each possible teeth height offset
	public float mouthOpenSpace;                // The minimum distance between the top and bottom teeth when the mouth is open
	public float toothExpansionRate;            // The rate at which the teeth will expand (1 = once every cycle)
	
	[Space (10)][Header ("Mouth Prefabs")]
	public GameObject prefab_toothTop;			// The prefab for teeth along the top row
	public GameObject prefab_toothBottom;		// The prefab for teeth along the bottom row

	[Space (10)][Header ("Drop Settings")]
	public int coinGapProximity;                // The proximity between the coin and the next gap for the coin to be placed
	public int currentGap;						// The index of the current gap
	public int nextGap;                         // The index of the tooth that will have a gap NEXT cycle
	public List<GameObject> itemPool;           // The list of items which can be dropped
	public float itemDropRate;                  // The number of cycles between each item drop
	public List<ItemDrop> itemDrops;			// List of item drops currently dropped

	[Space (10)][Header ("Audio Settings")]
	public AudioClip clip_smashTeeth;

	// Mouth calculated values
	float toothWidth;							// The width of an individual tooth
	float toothHeight;                          // The height of an individual tooth
	float jawTopVelocity;						// The velocity of the top jaw
	float jawBottomVelocity;                    // The veloicty of the bottom jaw
	float mouthHeight;                          // The distance between the top jaw and the bottom jaw when the mouth is fully open
	int cycleCount;                             // The current number cycle the mouth is on
	public float mouthWidth;					// The total mouth width (calculated for gameManager)
	
	[Space (10)][Header ("Mouth References")]
	public Transform mouth;                     // Reference for the mouth piece
	public SpriteRenderer mouthSpriteRenderer;  // Reference for the mouth's sprite renderer
	public Transform mouthInner;                // Reference for the mouthInner piece
	public SpriteRenderer mouthInnerSpriteRenderer;  // Reference for the mouthInner's sprite renderer
	public Transform mouthWallLeft;             // Reference for the left mouth wall
	public Transform mouthWallRight;            // Reference for the right mouth wall
	public Transform jawTop;					// Reference for the top jaw
	public Transform jawBottom;                 // Reference for the bottom jaw
	public List<Transform> teethTop;            // List of all teeth along the top jaw
	public List<Transform> teethBottom;         // List of all teeth along the bottom jaw
	public List<Transform> teeth;               // List of all teeth the monster has (including top and bottom jaws)
	public List<BoxCollider2D> teethColliders;	// List of all teeth colliders (including top and bottom jaws)
	public List<float> teethOffsets;			// List of all of the offsets for every tooth (length = teethCount * 2)
	public List<float> teethMorphDistances;     // The distance between each tooth's previousOffset and newOffset (used for individual tooth morph speed)
	public Transform jawUILeft;                 // Reference for jawUILeft which contains UI elements locked to the left of the jaw
	public Transform jawUIRight;                 // Reference for jawUIRight which contains UI elements locked to the right of the jaw

	[Space (10)][Header ("Head References")]
	public Transform head;                      // Reference for the head piece
	public List<Transform> eyes;                // Reference for all of the eyes the monster has

	[Space (10)][Header ("Chin References")]
	public Transform chin;                      // Reference for the chin piece

	[Space (10)][Header ("Clover")]
	public GameManager gameManager;				// GameManager reference
	public int cloverHintRate;					// The rate at which the clover will provide a hint for the next gap
	public Transform cloverHint;				// The hint indicator which is positioned next to the gap

	public MouthPhase mouthPhase;
	public enum MouthPhase { Opening, Morphing, Closing, Smashing, Expanding }

	// Events
	public event Action <Vector2> EventTeethSlam;
	public event Action EventPlayerLived;

	[Space (10)][Header ("Prefabs")]
	public GameObject prefab_coin;
	public GameObject prefab_heart;
	public GameObject prefab_projectileTeethSmash;

	private void Start () {
		gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
		audioManager = GameObject.FindGameObjectWithTag("AudioManager").GetComponent<AudioManager>();

		CreateMonster();
		
		StartCoroutine(MouthPhaseCoroutine());
	}

	private void Update () {
		UpdateEyes();
	}

	private void UpdateEyes () {
		foreach (Transform eye in eyes) {
			Vector2 lookDirection = (player.transform.position - eye.parent.position).normalized * 0.8f;
			eye.localPosition = Vector2.Lerp(eye.transform.localPosition, lookDirection, 7.5f * Time.deltaTime);
		}
	}

	void CreateMonster () {
		string directory = "Prefabs/Monsters/" + monsterName + "/";

		// Get prefabs
		prefab_toothTop = Resources.Load(directory + "ToothTop", typeof(GameObject)) as GameObject;
		prefab_toothBottom = Resources.Load(directory + "ToothBottom", typeof(GameObject)) as GameObject;

		// Get Jaws
		jawTop = transform.Find("[JawTop]");
		jawBottom = transform.Find("[JawBottom]");
		jawUILeft = jawTop.Find("[JawUILeft]");
		jawUIRight = jawTop.Find("[JawUIRight]");

		// Get Mouth Walls
		mouthWallLeft = transform.Find("[MouthWallLeft]");
		mouthWallRight = transform.Find("[MouthWallRight]");

		// Create Mouth
		mouth = Instantiate(Resources.Load(directory + "Mouth", typeof(GameObject)) as GameObject, transform).transform;
		mouth.name = "Mouth";
		mouthSpriteRenderer = mouth.GetComponent<SpriteRenderer>();
		mouthInner = Instantiate(Resources.Load(directory + "MouthInner", typeof(GameObject)) as GameObject, transform).transform;
		mouthInner.name = "MouthInner";
		mouthInnerSpriteRenderer = mouthInner.GetComponent<SpriteRenderer>();
		Transform newMouthInnerDetail = Instantiate(Resources.Load(directory + "MouthInnerDetail", typeof(GameObject)) as GameObject, mouthInner).transform;

		// Create Head
		head = Instantiate(Resources.Load(directory + "Head", typeof(GameObject)) as GameObject, transform).transform;
		head.name = "[Head]";

		// Create Chin
		chin = Instantiate(Resources.Load(directory + "Chin", typeof(GameObject)) as GameObject, transform).transform;
		chin.name = "[Chin]";

		// Get Eyes in head
		foreach (Transform t in head.GetComponentsInChildren<Transform>()) {
			if (t.gameObject.tag == "MonsterEye") {
				eyes.Add(t);
			}
		}

		// Create Teeth
		toothWidth = (prefab_toothTop.GetComponent<SpriteRenderer>().sprite.textureRect.width / 12);        // Get tooth width by dividing tooth pixel width by 12 (12 pixels = 1 unit)
		toothHeight = (prefab_toothTop.GetComponent<SpriteRenderer>().sprite.textureRect.height / 12);        // Get tooth height by dividing tooth pixel height by 12 (12 pixels = 1 unit)
		float toothOffsetLeft = (-toothWidth / 2) * (teethCount - 1);
		for (int i = 0; i < teethCount; i++) {
			Transform newToothTop = Instantiate(prefab_toothTop, jawTop.transform.position + new Vector3(toothOffsetLeft + (toothWidth * i), 0), Quaternion.identity).transform;
			newToothTop.parent = jawTop.Find("[Teeth]");
			teethTop.Add(newToothTop);

			Transform newToothBottom = Instantiate(prefab_toothBottom, jawBottom.transform.position + new Vector3(toothOffsetLeft + (toothWidth * i), 0), Quaternion.identity).transform;
			newToothBottom.parent = jawBottom.Find("[Teeth]");
			teethBottom.Add(newToothBottom);

			teethOffsets.Add(0);
			teethOffsets.Add(0);
		}

		teeth.AddRange(teethTop);
		teeth.AddRange(teethBottom);

		teethColliders.Clear();
		foreach (Transform tooth in teeth) {
			teethColliders.Add(tooth.GetComponent<BoxCollider2D>());
		}

		// Resize Gums
		ResizeGums();
	}

	private void SetMouthSize () {
		mouthHeight = ((teethHeightPositions - 1) * teethHeightInterval) + mouthOpenSpace;
		float jawDistance = Mathf.Abs(jawTop.transform.localPosition.y - jawBottom.transform.localPosition.y) + ((teethHeightPositions - 1) * teethHeightInterval) + 3;

		mouthWidth = (teethCount * toothWidth) + 2;
		mouthSpriteRenderer.size = new Vector2(mouthWidth, jawDistance + 1);
		mouth.transform.position = new Vector2(0, ((jawTop.transform.localPosition.y + jawBottom.transform.localPosition.y) / 2) + (teethHeightInterval * 0.5f * (teethHeightPositions - 1)));

		mouthInnerSpriteRenderer.size = new Vector2((teethCount * toothWidth) + 2, jawDistance + 1);
		mouthInner.transform.position = new Vector2(0, ((jawTop.transform.localPosition.y + jawBottom.transform.localPosition.y) / 2) + (teethHeightInterval * 0.5f * (teethHeightPositions - 1)));

		// Set mouth walls
		mouthWallLeft.transform.position = new Vector3((teethCount * toothWidth * -0.5f) - 0.5f, 0);
		mouthWallRight.transform.position = new Vector3((teethCount * toothWidth * 0.5f) + 0.5f, 0);

		// Position Jaw UI
		jawUILeft.transform.localPosition = new Vector3(teethCount * -toothWidth * 0.5f - 0.833333f, 0, 0);
		jawUIRight.transform.localPosition = new Vector3(teethCount * toothWidth * 0.5f + 0.833333f, 0, 0);

		// Position head
		head.transform.position = new Vector3(0, mouth.transform.position.y + (jawDistance + 1) / 2);

		// Position chin
		chin.transform.position = new Vector3(0, mouth.transform.position.y + (-jawDistance - 1) / 2);
	}

	private IEnumerator MouthPhaseCoroutine() {
		// Set Mouth Size
		SetMouthSize();

		yield return new WaitForSeconds(2);

		while (true) {
			// Clover hint
			if (cycleCount != 0 && cycleCount % cloverHintRate == 0) {
				bool hasClover = (gameManager.itemsCollected.Exists(i => i.name == "Clover"));
				if (hasClover == true) {
					cloverHint.gameObject.SetActive(true);
					cloverHint.parent = teeth[nextGap];
					cloverHint.localPosition = Vector3.zero;
				}
			}

			// Morphing
			//Debug.Log("Morphing");
			if (mouthPhase == MouthPhase.Morphing) {
				GetNewTeethOffsets();
				StartCoroutine(MorphCountdownCoroutine());
			}
			
			while (mouthPhase == MouthPhase.Morphing) {
				yield return new WaitForSeconds(0.016666f);
				MorphTeeth();
			}
			
			yield return new WaitForSeconds(0.05f + (teethCount * 0.095f) + (2f / Mathf.Sqrt(mouthSpeedCurrent)) + ((teethHeightPositions * teethHeightInterval) * 0.07f));
			
			// Closing
			//Debug.Log("Closing");
			while (mouthPhase == MouthPhase.Closing) {
				yield return new WaitForSeconds(0.016666f);
				MoveJaw(jawTop, new Vector2(0, 0.5f), ref jawTopVelocity, mouthSpeedCurrent);
				MoveJaw(jawBottom, new Vector2(0, -0.5f), ref jawBottomVelocity, mouthSpeedCurrent);

				if (Vector2.Distance(jawTop.position, jawBottom.position) <= 1) {
					if (player.isDead == false || player.attributesCombined.hearts > 0) {
						cycleCount++;
						if (EventPlayerLived != null) { EventPlayerLived(); }
					}
					cloverHint.gameObject.SetActive(false);
					if (EventTeethSlam != null) { EventTeethSlam(new Vector2(UnityEngine.Random.Range(-2.5f, 2.5f), -7.5f)); }
					audioManager.PlayClipAtPoint(clip_smashTeeth, jawTop.position - new Vector3(0, -0.5f, 0), 1f, 1f);
					mouthPhase = MouthPhase.Opening;
					jawTopVelocity = jawBottomVelocity = 0;
					CreateTeethSmashParticles();
				}
			}
			
			yield return new WaitForSeconds(0.375f + (0.75f / Mathf.Sqrt(mouthSpeedCurrent)));
			

			// Player death handling
			if (player.isDead == true) {
				if (player.attributesCombined.hearts <= 0) {   // if the player is dead foreal, smash that boi
					mouthPhase = MouthPhase.Smashing;
					while (mouthPhase == MouthPhase.Smashing) {
						//Debug.Log("Smashing");
						// Up
						bool looping = true;
						while (looping) {
							yield return new WaitForSeconds(0.016666f);
							MoveJaw(jawTop, new Vector2(0, 0.5f + 1.5f), ref jawTopVelocity, mouthSpeedCurrent * 2);
							MoveJaw(jawBottom, new Vector2(0, -0.5f), ref jawBottomVelocity, mouthSpeedCurrent * 2);

							if (Vector2.Distance(jawTop.position, jawBottom.position) >= 2.5f) {
								jawBottomVelocity = jawTopVelocity = 0;
								if (EventTeethSlam != null) { EventTeethSlam(new Vector2(UnityEngine.Random.Range(-1.5f, 1.5f), 2.5f)); }
								audioManager.PlayClipAtPoint(clip_smashTeeth, jawTop.position - new Vector3(0, -0.5f, 0), 0.325f, 1.5f);
								looping = false;
							}
						}

						// Down
						looping = true;
						while (looping) {
							yield return new WaitForSeconds(0.016666f);
							MoveJaw(jawTop, new Vector2(0, 0.5f), ref jawTopVelocity, mouthSpeedCurrent * 2);
							MoveJaw(jawBottom, new Vector2(0, -0.5f), ref jawBottomVelocity, mouthSpeedCurrent * 2);

							if (Vector2.Distance(jawTop.position, jawBottom.position) <= 1f) {
								mouthSpeedCurrent += mouthSpeedIncrement;
								jawBottomVelocity = jawTopVelocity = 0;
								if (EventTeethSlam != null) { EventTeethSlam(new Vector2(UnityEngine.Random.Range(-1.5f, 1.5f), -5f)); }
								audioManager.PlayClipAtPoint(clip_smashTeeth, jawTop.position - new Vector3(0, -0.5f, 0), 0.75f, 1f);
								CreateTeethSmashParticles();
								looping = false;
							}
						}

						yield return new WaitForSeconds(0.025f);
					}
				}
			}

			// Expand (if we should)
			if (cycleCount != 0 && cycleCount % toothExpansionRate == 0) {
				if (teethCount < teethCountMax) {
					yield return new WaitForSeconds(0.125f);
					audioManager.PlayClipAtPoint(clip_smashTeeth, jawTop.position - new Vector3(0, -0.5f, 0), 1f, 0.625f);
					ExpandTeeth(1);
				}
			}

			yield return new WaitForSeconds(0.125f + (0.5f / Mathf.Sqrt(mouthSpeedCurrent)));

			// Opening
			//Debug.Log("Opening");
			ResizeGums();

			bool hasDroppedItem = false;

			while (mouthPhase == MouthPhase.Opening) {
				yield return new WaitForSeconds(0.016666f);
				MoveJaw(jawTop, new Vector2(0, 0.5f + mouthHeight - 0.75f), ref jawTopVelocity, mouthSpeedCurrent);
				MoveJaw(jawBottom, new Vector2(0, -1.25f), ref jawBottomVelocity, mouthSpeedCurrent);

				if (hasDroppedItem == false && Vector2.Distance(jawTop.position, jawBottom.position) > ((teethHeightPositions - 1) * teethHeightInterval) + (mouthOpenSpace * 0.75f)) {
					// Spawn a new coin
					hasDroppedItem = true;
					DropItem();
				}
				
				if (Vector2.Distance((Vector2)jawTop.position, new Vector2(0, 0.5f + mouthHeight - 0.75f)) == 0) {
					mouthPhase = MouthPhase.Morphing;
					if (EventTeethSlam != null) { EventTeethSlam(new Vector2(UnityEngine.Random.Range(-2.5f, 2.5f), 5f)); }
					audioManager.PlayClipAtPoint(clip_smashTeeth, jawTop.position - new Vector3(0, -0.5f, 0), 0.75f, 1.5f);
					jawTopVelocity = jawBottomVelocity = 0;
				}
			}


			yield return new WaitForSeconds(0.125f + (0.25f / Mathf.Sqrt(mouthSpeedCurrent)));
		}
	}

	private IEnumerator MorphCountdownCoroutine () {
		yield return new WaitForSeconds(15f / mouthSpeedCurrent);
		mouthPhase = MouthPhase.Closing;
	}

	public void MoveJaw (Transform teethSection, Vector2 desiredPos, ref float jawVelocity, float acceleration) {
		List<RaycastHit2D> pushedEntities = new List<RaycastHit2D>();
		List<Collider2D> pulledPlayers = new List<Collider2D>();

		float timeScale = 1f / 60f;
		
		// Movement
		float desiredPositionDirection = desiredPos.y - teethSection.localPosition.y;
		jawVelocity += Mathf.Sign(desiredPositionDirection) * timeScale * acceleration;
		jawVelocity = Mathf.Clamp(jawVelocity, -Mathf.Abs(desiredPositionDirection) / timeScale, Mathf.Abs(desiredPositionDirection) / timeScale);

		Vector2 deltaP = new Vector2(0, jawVelocity) * timeScale;

		// Move Vertically
		float deltaY = deltaP.y;
		int raycasts = 5;
		float skinWidth = 0.05f;

		int toothIndexCurrent = 0;

		foreach (Transform tooth in teethSection.transform.Find("[Teeth]")) {
			toothIndexCurrent++;
			bool collisionRequired = false;     // Is collision required for this tooth? Only if it's x distance from any player/item is less than a set distance

			if (Mathf.Abs(tooth.position.x - player.transform.position.x) < toothWidth) {
				collisionRequired = true;
			} else {
				foreach (ItemDrop d in itemDrops) {
					if (Mathf.Abs(tooth.position.x - d.transform.position.x) < toothWidth) {
						collisionRequired = true;
						break;
					}
				}
			}
			
			if (collisionRequired == true) {

				BoxCollider2D toothCollider = teethColliders[toothIndexCurrent];

				// Pushing
				for (int i = 0; i < raycasts; i++) {
					float colliderXSmaller = toothCollider.size.x - (skinWidth / 2);
					float raycastIncrement = colliderXSmaller / (float)(raycasts - 1);       // Gets the increment between each raycast
					Vector2 origin = (Vector2)tooth.position + new Vector2(-(colliderXSmaller / 2) + i * raycastIncrement, ((toothCollider.size.y / 2) - skinWidth) * Mathf.Sign(deltaY)) + toothCollider.offset;
					Vector2 direction = new Vector2(0, Mathf.Sign(deltaY));
					//Debug.DrawRay(origin, direction, Color.red, 0);

					RaycastHit2D[] hits = Physics2D.RaycastAll(origin, direction, Mathf.Abs(deltaY) + skinWidth, entityAndRagdollMask);

					foreach (RaycastHit2D hit in hits) {
						if (hit.distance > skinWidth / 2) {
							Transform hitTransform = (hit.transform.gameObject.layer != LayerMask.NameToLayer("Rigibody") ? hit.transform : hit.transform.parent);
							if (pushedEntities.Exists(h => h.transform == hitTransform) == false) {
								pushedEntities.Add(hit);
								if (hit.transform.gameObject.layer != LayerMask.NameToLayer("Rigidbody")) {
									float deltaMinusDistance = (deltaY + (-Mathf.Sign(deltaY) * (hit.distance - skinWidth)));
									hitTransform.position += (Vector3)new Vector2(0, deltaMinusDistance);
								} else {
									hit.transform.GetComponent<Rigidbody2D>().velocity = new Vector2(hit.transform.GetComponent<Rigidbody2D>().velocity.x, jawVelocity * 12.5f);
								}
							}
						}
					}
				}

				// Pulling
				if (Mathf.Sign(deltaY) == -1) {
					Collider2D[] newPulledPlayers = Physics2D.OverlapBoxAll((Vector2)tooth.position + new Vector2(0, toothCollider.size.y / 2), new Vector2(toothCollider.size.x - (1f / 24f), 0.125f), 0, entityMask);
					foreach (Collider2D pulledPlayer in newPulledPlayers) {
						if (pulledPlayers.Contains(pulledPlayer) == false) {
							pulledPlayers.Add(pulledPlayer);
						}
					}
				}
			}
		}

		// Kill Players if now in environment (ie: in wall/other elevator)
		foreach (RaycastHit2D hit in pushedEntities) {
			if (hit.transform.gameObject.layer != LayerMask.NameToLayer("Ragdoll")) {
				if (Physics2D.OverlapBox((Vector2)hit.transform.position + hit.transform.GetComponent<BoxCollider2D>().offset, hit.transform.GetComponent<BoxCollider2D>().size - new Vector2(skinWidth, skinWidth), 0, environmentMask)) {
					hit.transform.GetComponent<Entity>().Die();
					hit.transform.position -= (Vector3)new Vector2(0, deltaY);
				}
			}
		}

		// Move teeth
		teethSection.position += (Vector3)deltaP;

		// Pulling
		foreach (Collider2D player in pulledPlayers) {
			if (player.transform.GetComponent<PlayerController>()) {
				PullEntity(player.GetComponent<Entity>(), new Vector2(0, deltaY));
			}
		}

		// Set Mouth Size
		SetMouthSize();
	}

	public class PlayerDelta {
		public Transform player;
		public float deltaPos;

		public PlayerDelta(Transform p, float d) {
			player = p;
			deltaPos = d;
		}
	}

	public void MorphTeeth() {
		
		float timeScale = 1f / 60f;

		List<PlayerDelta> pushedEntities = new List<PlayerDelta>();
		List<PlayerDelta> pulledPlayers = new List<PlayerDelta>();

		// Raycast info
		int raycasts = 5;
		float skinWidth = 0.05f;

		for (int j = 0; j < teeth.Count; j++) {
			Transform tooth = teeth[j];

			float desiredLocalHeight = teethOffsets[j];
			
			// Movement
			float desiredHeightDirection = desiredLocalHeight - tooth.localPosition.y;
			Vector2 deltaP = new Vector2(0, Mathf.Clamp(Mathf.Sign(desiredHeightDirection) * teethMorphDistances[j] * timeScale * (mouthSpeedCurrent / 5f), -Mathf.Abs(desiredHeightDirection), Mathf.Abs(desiredHeightDirection)));

			// Move Vertically
			float deltaY = deltaP.y;

			BoxCollider2D toothCollider = teethColliders[j];

			// Pushing
			for (int i = 0; i < raycasts; i++) {
				float colliderXSmaller = toothCollider.size.x - (skinWidth / 2);
				float raycastIncrement = colliderXSmaller / (float)(raycasts - 1);       // Gets the increment between each raycast
				Vector2 origin = (Vector2)tooth.position + new Vector2(-(colliderXSmaller / 2) + i * raycastIncrement, ((toothCollider.size.y / 2) - skinWidth) * Mathf.Sign(deltaY)) + toothCollider.offset;
				Vector2 direction = new Vector2(0, Mathf.Sign(deltaY));
				//Debug.DrawRay(origin, direction, Color.red, 0);

				RaycastHit2D[] hits = Physics2D.RaycastAll(origin, direction, Mathf.Abs(deltaY) + skinWidth, entityAndRagdollMask);

				foreach (RaycastHit2D hit in hits) {
					if (hit.distance > skinWidth / 2) {
						Transform hitTransform = (hit.transform.gameObject.layer != LayerMask.NameToLayer("Rigidbody") ? hit.transform : hit.transform.parent);
						if (pushedEntities.Exists(p => p.player.transform == hitTransform) == false) {
							pushedEntities.Add(new PlayerDelta(hitTransform, deltaY));
							// Move Player/Ragdoll
							if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Rigibody")) {
								hit.transform.GetComponent<Rigidbody2D>().velocity = new Vector2(hit.transform.GetComponent<Rigidbody2D>().velocity.x, direction.y * mouthSpeedCurrent);
							}
						} else {
							PlayerDelta thisPD = pushedEntities.Single(p => p.player.transform == hitTransform);
							if (thisPD.deltaPos < deltaY) {
								thisPD.deltaPos = deltaY;
							}
						}
					}
				}
			}

			// Pulling
			if (Mathf.Sign(deltaY) == -1) {
				Collider2D[] newPulledPlayers = Physics2D.OverlapBoxAll((Vector2)tooth.position + new Vector2(0, toothCollider.size.y / 2), new Vector2(toothCollider.size.x - (0.25f), 0.125f), 0, entityMask);
				foreach (Collider2D pulledPlayer in newPulledPlayers) {
					if (pulledPlayers.Exists(p => p.player == pulledPlayer.transform) == false) {
						pulledPlayers.Add(new PlayerDelta(pulledPlayer.transform, deltaY));
					}
				}
			}
			
			// Move tooth
			tooth.localPosition += (Vector3)deltaP;
		}

		// Pushing
		foreach (PlayerDelta pushedPlayer in pushedEntities) {
			pushedPlayer.player.position += new Vector3(0, pushedPlayer.deltaPos);
		}

		// Pulling
		foreach (PlayerDelta pulledPlayer in pulledPlayers) {
			if (pushedEntities.Exists(p => p.player == pulledPlayer.player) == false) { // If this player was already pushed, dont pull him
				PullEntity(pulledPlayer.player.GetComponent<Entity>(), new Vector2(0, pulledPlayer.deltaPos));
			}
		}

		// Kill Players if now in environment (ie: in wall/other elevator)
		foreach (PlayerDelta pushedPlayer in pushedEntities) {
			if (pushedPlayer.player.gameObject.layer != LayerMask.NameToLayer("Ragdoll")) {
				if (Physics2D.OverlapBox((Vector2)pushedPlayer.player.transform.position + pushedPlayer.player.transform.GetComponent<BoxCollider2D>().offset, pushedPlayer.player.transform.GetComponent<BoxCollider2D>().size - new Vector2(skinWidth, skinWidth), 0, environmentMask)) {
					pushedPlayer.player.transform.GetComponent<Entity>().Die();
					pushedPlayer.player.transform.position -= (Vector3)new Vector2(0, pushedPlayer.deltaPos);
				}
			}
		}

		// Set Mouth Size
		SetMouthSize();

		// Resize Gums
		ResizeGums();
	}

	public void PullEntity (Entity entity, Vector2 deltaP) {
		// Move entity vertically

		int verticalRaycasts = 5;
		float skinWidth = 0.0125f;
		BoxCollider2D entityCollider = entity.GetComponent<BoxCollider2D>();

		float hitDistanceV = Mathf.Infinity;
		float dy = (deltaP.y > 0 ? 1 : -1);
		for (int i = 0; i < verticalRaycasts; i++) {
			float colliderXSmaller = entityCollider.size.x - (skinWidth / 2);
			float raycastIncrement = colliderXSmaller / (float)(verticalRaycasts - 1);       // Gets the increment between each raycast
			Vector2 origin = (Vector2)entity.transform.position + new Vector2(-(colliderXSmaller / 2) + i * raycastIncrement, ((entityCollider.size.y / 2) - skinWidth) * dy) + entityCollider.offset;
			Vector2 direction = Vector2.up * dy;
			//Debug.DrawRay(origin, direction, Color.blue, 0);
			
			RaycastHit2D hit = Physics2D.Raycast(origin, direction, Mathf.Abs(deltaP.y) + skinWidth, environmentPlatformMask);

			if (hit.transform != null) {
				if (hit.distance - skinWidth < hitDistanceV) {
					hitDistanceV = hit.distance - skinWidth;
				}
			}
		}

		if (hitDistanceV != Mathf.Infinity) {
			entity.transform.position += (Vector3)new Vector2(0, hitDistanceV * dy);
			deltaP.y = 0;
		} else {
			entity.transform.position += (Vector3)new Vector2(0, deltaP.y);
		}
	}

	private void ResizeGums () {
		// Resize Gums
		foreach (Transform tooth in teeth) {
			Transform toothGum = tooth.Find("Gum");
			float GumMaxHeight = ((teethHeightPositions - 1) * teethHeightInterval) + 0.5f;
			bool isTop = (tooth.parent.parent.name == "[JawTop]" ? true : false);
			if (isTop == true) {
				toothGum.transform.localScale = new Vector3(1, (1f / 6f) + GumMaxHeight - tooth.localPosition.y);
				toothGum.transform.localPosition = new Vector3(0, 0.5f + (toothGum.transform.localScale.y / 2) - (1f / 12f));
			} else {
				toothGum.transform.localScale = new Vector3(1, (1f / 6f) + tooth.localPosition.y + 0.5f);
				toothGum.transform.localPosition = new Vector3(0, -0.5f - (toothGum.transform.localScale.y / 2) + (1f / 12f));
			}
		}
	}

	private void DropItem () {
		int randomToothIndex;
		List<int> possibleToothIndices = new List<int>();
		for (int i = 0; i < teethCount; i++) {
			if (teeth[i].localPosition.y == teeth[i + teethCount].localPosition.y && i != nextGap) {
				if (Mathf.Abs(i - nextGap) <= coinGapProximity) {       // Make sure the distance is within coin gap proximity
					possibleToothIndices.Add(i);
				}
			}
		}

		// If we can, remove some extra indices that a pretty far from the gap
		for (int x = 0; x < possibleToothIndices.Count; x++) {
			if (possibleToothIndices.Count > 1) {
				float dist = Mathf.Abs(possibleToothIndices[x] - nextGap);
				if (Mathf.Abs(possibleToothIndices[x] - nextGap) > 1) {
					if (UnityEngine.Random.Range(0f, dist) > 0.75f) {
						possibleToothIndices.RemoveAt(x);
						x--;
					}
				}
			}
		}

		if (possibleToothIndices.Count > 0) {
			randomToothIndex = possibleToothIndices[UnityEngine.Random.Range(0, possibleToothIndices.Count)];

			// Drop either a coin or an item from the itemPool
			if ((cycleCount == 0 || itemPool.Count <= 0 || cycleCount % itemDropRate != 0) && cycleCount != 5) {    // Drop coin/heart or item?
				float heartsMissing = 7 - player.attributesCombined.hearts;
				float luckCoefficient = Mathf.Clamp(Mathf.Sqrt(Mathf.Abs(player.attributesCombined.luck) * 10f) * Mathf.Sign(player.attributesCombined.luck) * 0.01f, -0.1f, 0.1f) * (heartsMissing);
				float probability = (0.25f / (float)player.attributesCombined.hearts) + luckCoefficient;
				//Debug.Log("Probability: " + (probability * 100) + "%  -  LuckCoefficient: " + (luckCoefficient * 100) + "%");
				GameObject randomItem = (UnityEngine.Random.Range(0f, 1) <= probability && heartsMissing > 0 ? prefab_heart : prefab_coin);
				GameObject newItem = (GameObject)Instantiate(randomItem, teeth[randomToothIndex].position + new Vector3((1f / 24f), -0.25f), Quaternion.identity);
				itemDrops.Add(newItem.GetComponent<ItemDrop>());
			} else {
				if (itemPool.Count > 0) {       // Make sure we have items left
					int randomItemIndex = UnityEngine.Random.Range(0, itemPool.Count);
					GameObject newItem = Instantiate(itemPool[randomItemIndex], teeth[randomToothIndex].position + new Vector3((1f / 24f), -0.25f), Quaternion.identity);
					itemPool.RemoveAt(randomItemIndex);
					itemDrops.Add(newItem.GetComponent<ItemDrop>());
				}
			}
		}
	}

	private void ExpandTeeth (int teethExpansionCount) {
		int expansionRight = 0;
		int expansionLeft = 0;

		// Create New Teeth
		float toothOffset = (toothWidth / 2) * (teethCount) + (toothWidth / 2);
		for (int i = 0; i < teethExpansionCount; i++) {
			float Direction = Mathf.Sign(UnityEngine.Random.Range(-1f, 1f));

			float expansionCurrent = (Direction == 1 ? expansionRight : expansionLeft);
			float randomToothOffset = UnityEngine.Random.Range(1, teethHeightPositions) * teethHeightInterval;

			Transform newToothTop = Instantiate(prefab_toothTop, jawTop.transform.position + new Vector3((toothOffset * Direction) + (toothWidth * expansionCurrent), randomToothOffset), Quaternion.identity).transform;
			newToothTop.parent = jawTop.Find("[Teeth]");
			
			Transform newToothBottom = Instantiate(prefab_toothBottom, jawBottom.transform.position + new Vector3((toothOffset * Direction) + (toothWidth * expansionCurrent), randomToothOffset), Quaternion.identity).transform;
			newToothBottom.parent = jawBottom.Find("[Teeth]");
			
			if (Direction == 1) {
				teethTop.Add(newToothTop);
				teethBottom.Add(newToothBottom);
				teethOffsets.Insert(teethCount + (expansionRight + Mathf.Abs(expansionLeft)), randomToothOffset);
				teethOffsets.Add(randomToothOffset);
			} else {
				teethTop.Insert(0, newToothTop);
				teethBottom.Insert(0, newToothBottom);
				teethOffsets.Insert(0, randomToothOffset);
				teethOffsets.Insert(teethCount + (expansionRight + 1 + Mathf.Abs(expansionLeft)), randomToothOffset);
			}
			
			expansionLeft += (Direction == -1 ? -1 : 0);
			expansionRight += (Direction == 1 ? 1 : 0);
		}

		teeth.Clear();
		teeth.AddRange(teethTop);
		teeth.AddRange(teethBottom);

		teethColliders.Clear();
		foreach (Transform tooth in teeth) {
			teethColliders.Add(tooth.GetComponent<BoxCollider2D>());
		}

		float netExpansion = (float)expansionLeft + (float)expansionRight;

		// Offset Old Teeth
		foreach (Transform tooth in teeth) {
			tooth.transform.localPosition += new Vector3(-toothWidth * 0.5f * netExpansion, 0);
		}

		if (EventTeethSlam != null) { EventTeethSlam(new Vector2(Mathf.Sign(netExpansion) * 25f, UnityEngine.Random.Range(-5f, 5f))); }

		player.transform.position += new Vector3(-toothWidth * 0.5f * netExpansion, 0);		// Move Player

		// Change the teethCount
		teethCount += teethExpansionCount;

		// Resize Gums
		ResizeGums();

		// Set Mouth Size
		SetMouthSize();
	}

	private void GetNewTeethOffsets () {
		// Set up teethOffsets
		List<float> teethOffsetsBefore = new List<float>(teethOffsets);     // Copy the old teeth offsets (for finding distances)
		teethOffsets = new List<float>();
		for (int x = 0; x < teethCount; x++) {
			teethOffsets.Add(UnityEngine.Random.Range(0, teethHeightPositions) * teethHeightInterval);
		}
		
		teethOffsets.AddRange(teethOffsets);

		// Set gap offset for gap tooth
		currentGap = nextGap;

		// Move a tooth to make a gap for the player to fit
		if ((teethOffsets[currentGap] + 1) > ((teethHeightPositions - 1) * teethHeightInterval)) {
			if ((teethOffsets[currentGap + teethCount] - 1) < 0) {
				teethOffsets[currentGap + teethCount] = teethOffsets[currentGap + teethCount] - 0.5f;
				teethOffsets[currentGap] = teethOffsets[currentGap] + 0.5f;
			} else {
				teethOffsets[currentGap + teethCount] = teethOffsets[currentGap + teethCount] - 1;
			}
		} else {
			teethOffsets[currentGap] = teethOffsets[currentGap] + 1;
		}
		
		// Get Morph Distances
		teethMorphDistances.Clear();
		for (int i = 0; i < teethOffsets.Count; i++) {
			teethMorphDistances.Add(Mathf.Abs(teethOffsetsBefore[i] - teethOffsets[i]));
		}

		mouthSpeedCurrent += mouthSpeedIncrement;
		nextGap = UnityEngine.Random.Range(0, teethCount);
	}
	
	private void CreateTeethSmashParticles () {
		Transform toothLeftmost = teeth[0];
		Transform toothRightmost = teeth[teethCount - 1];

		float p = 1f / 12f;
		

		if (currentGap != 0) {
			CreateIndividualSmashParticles(toothLeftmost, -1f, Vector3.zero);

			// Gap Right Side particles
			if (currentGap == teethCount - 1) {
				float gapOffsetRight = teeth[currentGap].localPosition.y - teeth[currentGap - 1].localPosition.y;
				gapOffsetRight = (gapOffsetRight == 0 ? -p : (gapOffsetRight == 1 ? p : 0));
				if (gapOffsetRight != 0) {
					CreateIndividualSmashParticles(teeth[currentGap - 1], 1f, new Vector3(0, gapOffsetRight, 0));
				}
			}
		}

		if (currentGap != teethCount - 1) {
			CreateIndividualSmashParticles(toothRightmost, 1f, Vector3.zero);

			// Gap Right Side particles
			if (currentGap == 0) {
				float gapOffsetLeft = teeth[currentGap].localPosition.y - teeth[currentGap + 1].localPosition.y;
				gapOffsetLeft = (gapOffsetLeft == 0 ? -p : (gapOffsetLeft == teethHeightInterval ? p : 0));
				if (gapOffsetLeft != 0) {
					CreateIndividualSmashParticles(teeth[currentGap + 1], -1f, new Vector3(0, gapOffsetLeft, 0));
				}
			}
		}
	}

	private void CreateIndividualSmashParticles (Transform tooth, float direction, Vector3 extraOffset) {
		Vector3 offset = new Vector3(((toothWidth / 2) + (1f / 12f)) * direction, -toothHeight, -0.125f) + extraOffset;
		
		for (int i = 0; i < 3; i++) {
			Vector2 randomVelocity = Quaternion.Euler(0, 0, -15 + (15 * i) + UnityEngine.Random.Range(-2.5f, 2.5f)) * new Vector2(UnityEngine.Random.Range(10f, 15f) * direction * Mathf.Sqrt(mouthSpeedCurrent), 0);

			// Create projectile
			GameObject newProjectile = Instantiate(prefab_projectileTeethSmash, tooth.position + offset, Quaternion.identity);
			Projectile newProjectileComponent = newProjectile.GetComponent<Projectile>();
			newProjectileComponent.SetupProjectile(randomVelocity);
		}
	}

	private void OnDrawGizmosSelected () {
		// Clamp values
		teethCount = (int)Mathf.Clamp(teethCount, 1, Mathf.Infinity);
	}

}
