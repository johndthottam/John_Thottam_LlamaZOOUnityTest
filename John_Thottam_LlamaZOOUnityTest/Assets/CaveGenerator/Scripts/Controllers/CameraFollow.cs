using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour {

	[SerializeField]
	private GameObject player;

	private Vector3 offset;

	void Start ()
	{
		player = GameObject.Find("cave_player(Clone)");
		offset = transform.position - player.transform.position;
	}

	void LateUpdate ()
	{
		transform.position = player.transform.position + offset;
	}
}
