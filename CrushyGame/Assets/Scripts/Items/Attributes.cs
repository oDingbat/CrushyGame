using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Attributes {

	// Integers
	public int speed;
	public int acceleration;
	public int deceleration;
	public int jumpHeight;
	public int magnetism;
	public int weight;
	public int airJumps;
	public int luck;
	public int hearts;
	public int heartsMax;
	public int cursedHearts;
	public int climbing;

	public Attributes () {
		speed = 0;
		acceleration = 0;
		deceleration = 0;
		jumpHeight = 0;
		magnetism = 0;
		weight = 0;
		airJumps = 0;
		luck = 0;
		hearts = 0;
		heartsMax = 0;
		cursedHearts = 0;
		climbing = 0;
	}

	public Attributes(Attributes copiedAttribute) {
		speed = copiedAttribute.speed;
		acceleration = copiedAttribute.acceleration;
		deceleration = copiedAttribute.deceleration;
		jumpHeight = copiedAttribute.jumpHeight;
		magnetism = copiedAttribute.magnetism;
		weight = copiedAttribute.weight;
		airJumps = copiedAttribute.airJumps;
		luck = copiedAttribute.luck;
		hearts = copiedAttribute.hearts;
		heartsMax = copiedAttribute.heartsMax;
		cursedHearts = copiedAttribute.cursedHearts;
		climbing = copiedAttribute.climbing;

		ClampValues();
	}

	public void Add (Attributes attributesAdded) {
		speed += attributesAdded.speed;
		acceleration += attributesAdded.acceleration;
		deceleration += attributesAdded.deceleration;
		jumpHeight += attributesAdded.jumpHeight;
		magnetism += attributesAdded.magnetism;
		weight += attributesAdded.weight;
		airJumps += attributesAdded.airJumps;
		luck += attributesAdded.luck;
		hearts += attributesAdded.hearts;
		heartsMax += attributesAdded.heartsMax;
		cursedHearts += attributesAdded.cursedHearts;
		climbing += attributesAdded.climbing;
	}

	public void Subtract (Attributes attributesSubtracted) {
		speed -= attributesSubtracted.speed;
		acceleration -= attributesSubtracted.acceleration;
		acceleration -= attributesSubtracted.deceleration;
		jumpHeight -= attributesSubtracted.jumpHeight;
		magnetism -= attributesSubtracted.magnetism;
		weight -= attributesSubtracted.weight;
		airJumps -= attributesSubtracted.airJumps;
		luck -= attributesSubtracted.luck;
		hearts -= attributesSubtracted.hearts;
		heartsMax -= attributesSubtracted.heartsMax;
		cursedHearts -= attributesSubtracted.cursedHearts;
		climbing -= attributesSubtracted.climbing;
	}

	public void ClampValues () {
		speed = (int)Mathf.Clamp(speed, 1f, Mathf.Infinity);
		acceleration = (int)Mathf.Clamp(acceleration, 1f, Mathf.Infinity);
		deceleration = (int)Mathf.Clamp(deceleration, 0f, Mathf.Infinity);
		jumpHeight = (int)Mathf.Clamp(jumpHeight, 1f, Mathf.Infinity);
		hearts = (int)Mathf.Clamp(hearts, 0, heartsMax);
		heartsMax = (int)Mathf.Clamp(heartsMax, 1, 10);
		cursedHearts = (int)Mathf.Clamp(cursedHearts, 0, 10 - heartsMax);
	}

	public static Attributes Combine(List<Attributes> attributesList) {
		// Returns a new single Attributes object which is a combination of all attributes in attributesList
		Attributes newAttributesCombined = new Attributes();

		foreach (Attributes a in attributesList) {
			newAttributesCombined.Add(a);
		}

		return newAttributesCombined;
	}

}
