using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

	public float moveSpeed;
	public float movementSmoothing;
	public float rotationSmoothing;

	public GameObject meleeWeapon;

	private Vector3 movement;
	private Rigidbody body;
	private Animator animator;
	private Collider weaponCollider;

	public float currentSpeed = 0f;

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
		Vector3 input = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));

		if (input != Vector3.zero) {
			animator.SetBool("Running", true);
		} else {
			animator.SetBool("Running", false);
		}

		currentSpeed = Mathf.Lerp(
			currentSpeed,
			input == Vector3.zero ? 0f : moveSpeed,
			movementSmoothing * Time.deltaTime);

		movement = Quaternion.Euler(0, 45, 0) * input.normalized * currentSpeed * Time.deltaTime;

		// if (currentSpeed > 0.1f) {
			body.MovePosition(transform.position + movement);
		// }

		if (movement != Vector3.zero) {
			float targetRotation = Quaternion.LookRotation(movement).eulerAngles.y;
			Vector3 targetRotationVec = new Vector3(0f, targetRotation, 0f);

			// Stop lerping when the difference is below the threshold
			if (Mathf.Abs(targetRotation - transform.eulerAngles.y) < 1f) {
				transform.eulerAngles = targetRotationVec;
			} else {
				transform.eulerAngles = Vector3.Lerp(
					transform.eulerAngles,
					targetRotationVec,
					rotationSmoothing * Time.deltaTime);
			}
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
