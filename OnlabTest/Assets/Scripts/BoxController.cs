using UnityEngine;
using System.Collections;

public class BoxController : MonoBehaviour {

	private RaycastHit2D groundCheck;
	public LayerMask whatIsGround;


	// Use this for initialization
	void Start () {
	
	}

	void OnCollisionEnter2D(Collision2D coll){
		if (coll.gameObject != null) {
			if (transform.Find ("Balloon").gameObject != null) {
				Destroy (transform.Find ("Balloon").gameObject);
			}
		}
	}
}
