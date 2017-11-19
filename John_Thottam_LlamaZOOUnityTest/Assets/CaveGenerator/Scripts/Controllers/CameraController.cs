using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {

	private GameObject cave_player;
	private Camera[] cameras;

	private Vector3 offset;

	void Start ()
	{
		if (cameras == null) {
			//respawns = GameObject.FindGameObjectsWithTag ("Main Camera");
			cameras = Camera.allCameras;
		}
			
		foreach (Camera cam in cameras)
		{
			if (cam.name != "camera_cave_player(Clone)" && cam.name!= "MiniMapCamera(Clone)") 
			{
				cam.enabled = false;
			}
		}

		
		cave_player= GameObject.Find("cave_player(Clone)");
		offset = transform.position - cave_player.transform.position;
	}

	void LateUpdate ()
	{
		transform.position = cave_player.transform.position + offset;
	}
}