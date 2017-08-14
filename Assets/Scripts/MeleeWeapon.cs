using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeWeapon : MonoBehaviour {

	public GameObject player;

	void OnTriggerEnter(Collider other) {
		if (other.gameObject != player) {
			EnemyController enemy = other.GetComponent<EnemyController>();

			if (enemy != null) {
				Debug.Log("Hit!");
				enemy.TakeDamage(10, player.transform.eulerAngles);
			}

		}
	}
}
