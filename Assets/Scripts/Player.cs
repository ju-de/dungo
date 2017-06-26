using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

	public float moveSpeed = 8f;

	Vector3 movement;
	Rigidbody body;
	GameObject playerModel;

	void Awake() {
		body = GetComponent<Rigidbody>();
		playerModel = this.transform.Find("Model").gameObject;
	}
	
	void FixedUpdate() {
		float h = Input.GetAxisRaw("Horizontal");
		float v = Input.GetAxisRaw("Vertical");

		if (h != 0 || v != 0) {
			movement.Set(h, 0f, v);
			movement = Quaternion.Euler(0, 45, 0) * movement.normalized * moveSpeed * Time.deltaTime;
			body.MovePosition(transform.position + movement);

			Vector3 rotation = playerModel.transform.rotation.eulerAngles;
			rotation.y = Quaternion.LookRotation(movement).eulerAngles.y;
			playerModel.transform.eulerAngles = rotation;
		}		
	}
}
