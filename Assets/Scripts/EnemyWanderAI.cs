using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class EnemyWanderAI : MonoBehaviour {

	NavMeshAgent agent;

	bool enabled = false;
	float wait = 0f;
	float timer = 0f;

	public float wanderDistance;
	public float waitTime;

	void Awake() {
		agent = GetComponent<NavMeshAgent>();
	}

	void FixedUpdate() {
		if (!enabled) return;

		timer += Time.deltaTime;
		if (timer > wait) {
			// get random location around current location and walk to it
			Vector3 rand = Random.insideUnitSphere * wanderDistance;
			rand.y = 0f;
			Vector3 target = transform.position + rand;
			agent.destination = target;

			timer = 0f;
			wait = Random.Range(waitTime - 1f, waitTime + 1f);
		}
	}

	public bool Enabled {
		get { return enabled; }
		set { enabled = value; }
	}
}
