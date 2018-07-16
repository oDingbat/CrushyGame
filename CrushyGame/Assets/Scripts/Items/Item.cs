using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Item {

	public string name;
	public RuntimeAnimatorController animController;
	public Attributes attributes;
	public bool isCollectable = false;

}
