using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RagdollController : MonoBehaviour {
	
	public LayerMask collisionMask;
	Transform head;

	public bool IsRagdollSurvivable () {
		head = transform.Find("Ragdoll_Head");

		if (Physics2D.OverlapCircle(head.position, 0.0125f, collisionMask)) {
			return false;
		} else {
			return true;
		}
	}

}
