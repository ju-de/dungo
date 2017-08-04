using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

	public float moveSpeed;
	public float rotationSmoothing;

	public GameObject meleeWeapon;

	private Vector3 movement;
	private Rigidbody body;
	private Animator animator;
	private Collider weaponCollider;

	private bool isAttacking = false;

	void Awake() {
		body = GetComponent<Rigidbody>();
		animator = GetComponent<Animator>();
		weaponCollider = meleeWeapon.GetComponent<Collider>();
	}

	void Update() {
		if (Input.GetMouseButton(0)) {
            animator.SetBool("Attack", true);
			isAttacking = true;
        } else {
			animator.SetBool("Attack", false);
		}
	}

	void FixedUpdate() {
		float h = Input.GetAxisRaw("Horizontal");
		float v = Input.GetAxisRaw("Vertical");

		if (h != 0 || v != 0) {
			movement.Set(h, 0f, v);
			movement = Quaternion.Euler(0, 45, 0) * movement.normalized * moveSpeed * Time.deltaTime;
			body.MovePosition(transform.position + movement);

			animator.SetBool("Running", true);
		} else {
			animator.SetBool("Running", false);
		}

		if (movement != Vector3.zero) {
			Quaternion newRot = Quaternion.LookRotation(movement);
			transform.rotation = Quaternion.Lerp(transform.rotation, newRot, rotationSmoothing * Time.deltaTime);
		}
	}

	public bool IsAttacking {
		get { return isAttacking; }
		set { isAttacking = value; }
	}

	void BeginHit() {
		weaponCollider.enabled = true;
	}

	void EndHit() {
		weaponCollider.enabled = false;
	}
}
