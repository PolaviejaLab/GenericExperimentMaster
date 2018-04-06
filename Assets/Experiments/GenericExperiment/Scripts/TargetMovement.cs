using UnityEngine;
using System.Collections;

public class TargetMovement : MonoBehaviour {


	public GameObject target; 
	private float speed;
	private Vector3 movement;


	void Start () {
		NewMovements ();
	}
	
	void Update () {
		target.transform.Translate (movement*speed);
		if (target.transform.position.x > 4.75 || 
		    target.transform.position.z > 4.75 || 
		    target.transform.position.x < -4.75 || 
		    target.transform.position.z < -4.75) {
			NewMovements ();
			}
	}
	void NewMovements () {
		movement = new Vector3(Random.Range(-15, 15), 0, Random.Range(-15, 15));
		speed = Random.Range (0.0005f, 0.0010f);
	}
}