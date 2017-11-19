using UnityEngine;
using System.Collections;

public class MapItem : MonoBehaviour
{

	void Update ()
	{
		transform.Rotate(Vector3.up, 30f* Time.deltaTime);
	}

	void OnCollisionEnter (Collision col)
	{
		if(col.gameObject.name == "cave_player(Clone)" || col.gameObject.name == "AI_Character(Clone)")
		{
			Destroy(this.gameObject);
		}
	}
}