using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public abstract class Entity : MonoBehaviour {
	
	[Space(10)][Header("Vitals")]
	public int		healthCurrent = 10;				// The amount of health this entity currently has
	public int		healthMax = 10;					// The maximum amount of health this entity can have
	public bool		isDead = false;					// Is this entity dead?
	public bool		isInvulnerable;					// Is the player invulnerable

	public void Die() {
		if (isInvulnerable == false) {
			if (isDead == false) {
				isDead = true;
				healthCurrent = 0;
				OnDie();
			}
		}
	}

	public void SetHealth (int amount) {
		healthCurrent = amount;
	}

	public void Revive () {
		isDead = false;
		healthCurrent = healthMax;
		OnRevive();
	}

	public abstract void OnDie();
	public abstract void OnRevive();

}
