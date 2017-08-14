using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera : MonoBehaviour {

	public Transform target;
	public float smoothing = 20f;
	public Vector3 offset;

	private Vector3 vel = Vector3.zero;

	void Awake () {
	}
	
	void FixedUpdate () {
		Vector3 nextPos = target.position + offset;
		transform.position = Vector3.SmoothDamp(transform.position, nextPos, ref vel, smoothing * Time.deltaTime);
	}
}
