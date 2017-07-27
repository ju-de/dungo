using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

	public float moveSpeed = 8f;
	public float attackMoveSpeedFactor = 0.4f;

	Vector3 movement;
	Rigidbody body;
	Animator animator;

	void Awake() {
		body = GetComponent<Rigidbody>();
		animator = GetComponent<Animator>();
	}

	void Update() {
		if (Input.GetMouseButton(0)) {
            animator.SetBool("Attack", true);
        } else {
			animator.SetBool("Attack", false);
		}
	}

	void FixedUpdate() {
		float h = Input.GetAxisRaw("Horizontal");
		float v = Input.GetAxisRaw("Vertical");

		if (h != 0 || v != 0) {
			float realMoveSpeed = animator.GetBool("Attack") ?
					moveSpeed * attackMoveSpeedFactor :
					moveSpeed;
			movement.Set(h, 0f, v);
			movement = Quaternion.Euler(0, 45, 0) * movement.normalized * realMoveSpeed * Time.deltaTime;
			body.MovePosition(transform.position + movement);

			Vector3 rotation = this.transform.rotation.eulerAngles;
			rotation.y = Quaternion.LookRotation(movement).eulerAngles.y;
			this.transform.eulerAngles = rotation;

			animator.SetBool("Running", true);
		} else {
			animator.SetBool("Running", false);
		}
	}
}
