using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {

	public float speed;

	Vector3 velocity;
	private Rigidbody rb;

	void Start ()
	{
		rb = GetComponent<Rigidbody>();
	}
		
	void Update () {
		velocity = new Vector3 (Input.GetAxisRaw ("Horizontal"), 0, Input.GetAxisRaw ("Vertical")).normalized * 10;
	}

	void FixedUpdate() {
		rb.MovePosition (rb.position + velocity * Time.fixedDeltaTime);
	}
	//use for smooth rolling motion
	//void FixedUpdate ()
	//{
	//	float moveHorizontal = Input.GetAxis ("Horizontal");
	//	float moveVertical = Input.GetAxis ("Vertical");
	//
	//	Vector3 movement = new Vector3 (moveHorizontal, 0.0f, moveVertical);
	//
	//	rb.AddForce (movement * speed);
	//}
}