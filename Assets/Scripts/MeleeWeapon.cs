using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeWeapon : MonoBehaviour {

	public GameObject player;

	void OnTriggerEnter(Collider other) {
		if (other.gameObject != player) {
			Debug.Log("Hit!");
		}
	}
}
