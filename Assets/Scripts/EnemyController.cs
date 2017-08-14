using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour {

	Rigidbody body;

	void Awake() {
		body = GetComponent<Rigidbody>(); 
	}

	public void TakeDamage(int amount, Vector3 direction) {
		Debug.Log("Took damage: " + amount);

		body.AddForce(new Vector3(100, 0, 0));
	}
}
