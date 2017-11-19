using UnityEngine;
using System.Collections;

public class AutoRoamingAI : MonoBehaviour {

	public float roamRadius = 45;
	public float roamTimer = 4;

	private Transform target;
	private UnityEngine.AI.NavMeshAgent agent;
	private float timer;

	// Use this for initialization
	void OnEnable () {
		agent = GetComponent<UnityEngine.AI.NavMeshAgent> ();
		timer = roamTimer;
	}

	// Update is called once per frame
	void Update () {
		timer += Time.deltaTime;

		if (timer >= roamTimer) {
			Vector3 newPos = RandomNavSphere(transform.position, roamRadius, -1);
			agent.SetDestination(newPos);
			timer = 0;
		}
	}

	public static Vector3 RandomNavSphere(Vector3 origin, float dist, int layermask) {
		Vector3 randDirection = Random.insideUnitSphere * dist;

		randDirection += origin;

		UnityEngine.AI.NavMeshHit navHit;

		UnityEngine.AI.NavMesh.SamplePosition (randDirection, out navHit, dist, layermask);

		return navHit.position;
	}
}