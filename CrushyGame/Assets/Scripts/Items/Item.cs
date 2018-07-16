using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Item {

	public string name;									// The name of the item
	public RuntimeAnimatorController animController;	// The animationController of the item
	public Attributes attributes;						// The attributes of the item
	public bool isCollectable = false;					// Is this item collectable

}
