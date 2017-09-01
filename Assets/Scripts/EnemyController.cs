using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour {

	public float visionAngleRange;
	public float aggroDistance;
	public float deaggroDistance;

	Rigidbody body;
	NavMeshAgent agent;
	Animator animator;
	GameObject player;

	bool aggro = false;

	void Awake() {
		body = GetComponent<Rigidbody>(); 
		agent = GetComponent<NavMeshAgent>();
		animator = GetComponent<Animator>();
		player = GameObject.FindWithTag("Player");
	}

	void FixedUpdate() {
		bool los = !Physics.Linecast(transform.position + new Vector3(0f, 1f, 0f),		// add 1 to y axis to avoid hitting the floor
									 player.transform.position + new Vector3(0f, 1f, 0f),
									 1 << 8);		// index 8 is Blocking layer
		float distance = Vector3.Distance(transform.position, player.transform.position);
		
		Vector3 playerDirection = Quaternion.LookRotation(player.transform.position - transform.position).eulerAngles;
		float directionDelta = Mathf.Abs(transform.rotation.eulerAngles.y - playerDirection.y);

		if (directionDelta < visionAngleRange && los && distance < aggroDistance) {
			aggro = true;
		} else if (!los && distance > deaggroDistance) {
			aggro = false;
		}
		if (aggro) {
			agent.isStopped = false;
			agent.destination = player.transform.position;
		} else {
			agent.isStopped = true;
		}
		animator.SetBool("Walking", !agent.isStopped);
	}

	public void TakeDamage(int amount, Vector3 direction) {
		Debug.Log("Took damage: " + amount);

		body.AddForce(new Vector3(100, 0, 0));
	}
}
