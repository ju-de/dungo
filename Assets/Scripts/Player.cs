using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

	public float moveSpeed = 8f;

	Vector3 movement;
	Rigidbody body;

	void Awake() {
		body = GetComponent<Rigidbody>();
	}
	
	void FixedUpdate() {
		float h = Input.GetAxisRaw("Horizontal");
		float v = Input.GetAxisRaw("Vertical");

		movement.Set(h, 0f, v);
		movement = Quaternion.Euler(0, 45, 0) * movement.normalized * moveSpeed * Time.deltaTime;
		body.MovePosition(transform.position + movement);
	}
}
