using UnityEngine;
using System.Collections;


[RequireComponent (typeof (Rigidbody2D))]
[RequireComponent (typeof (CircleCollider2D))]

public class BallController : MonoBehaviour {

	/// <summary>
	/// Raises the collision enter2 d event.
	/// </summary>
	/// <param name="coll">Coll.</param>
	void OnCollisionEnter2D(Collision2D coll){
		//Ha leesik akkor újraterem valahol a pályán
		if (coll.gameObject.tag == "Hole") {
			float rndXPos = Random.Range (9.0f, -9.0f);
			float YPos = 5.0f;
			transform.position = new Vector2 (rndXPos, YPos);
		}
	}

}
