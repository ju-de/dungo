using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera : MonoBehaviour {

	public Transform target;
	public float smoothing = 20f;

	public Vector3 offset;

	void Awake () {
		// offset = new Vector3(-5, 7, -5);
	}
	
	void FixedUpdate () {
		Vector3 nextPos = target.position + offset;
		transform.position = Vector3.Lerp(transform.position, nextPos, 20f * Time.deltaTime);
	}
}
