using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

	public float moveSpeed;
	public float movementSmoothing;
	public float rotationSmoothing;
	public float rollSpeedMultiplier;

	public GameObject meleeWeapon;

	private Vector3 movement;
	private Vector3 faceDirection;
	private Rigidbody body;
	private Animator animator;
	private Collider weaponCollider;

	private bool isRolling = false;

	public float currentSpeed = 0f;

	void Awake() {
		body = GetComponent<Rigidbody>();
		animator = GetComponent<Animator>();
		weaponCollider = meleeWeapon.GetComponent<Collider>();
	}

	void Update() {
		if (!isRolling) {
			animator.SetBool("Attack", Input.GetMouseButton(0));
		}
		if (Input.GetKeyDown(KeyCode.Space)) {
			animator.SetTrigger("Roll");
		}
	}

	void FixedUpdate() {
		Vector3 input = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));

		// Set animator param
		animator.SetBool("Running", input != Vector3.zero);

		if (input != Vector3.zero && !isRolling) {	// prevent direction change during roll
			faceDirection = Quaternion.Euler(0, 45, 0) * input.normalized;
		}

		// Handle movement
		float targetSpeed = isRolling ?
								(rollSpeedMultiplier * moveSpeed) :
								(input == Vector3.zero ? 0f : moveSpeed);
		if (Mathf.Abs(targetSpeed - currentSpeed) < 0.1f) {		// lerp threshold
			currentSpeed = targetSpeed;
		} else {
			currentSpeed = Mathf.Lerp(
			currentSpeed,
			targetSpeed,
			movementSmoothing * Time.deltaTime);
		}
		movement = faceDirection * currentSpeed * Time.deltaTime;
		body.MovePosition(transform.position + movement);

		// Handle rotation
		if (faceDirection != Vector3.zero) {
			Quaternion targetRotation = Quaternion.LookRotation(faceDirection);
			if (Mathf.Abs(targetRotation.eulerAngles.y - transform.eulerAngles.y) < 1f) {	// lerp threshold
				transform.rotation = targetRotation;
			} else {
				transform.rotation = Quaternion.Lerp(
					transform.rotation,
					targetRotation,
					rotationSmoothing * Time.deltaTime);
			}
		}
	}

	void BeginHit() {
		weaponCollider.enabled = true;
	}

	void EndHit() {
		weaponCollider.enabled = false;
	}

	void BeginRoll() {
		isRolling = true;
	}

	void EndRoll() {
		isRolling = false;
	}
}
