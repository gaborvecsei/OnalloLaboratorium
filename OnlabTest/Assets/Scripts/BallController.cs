using UnityEngine;
using System.Collections;

/// <summary>
/// A labdának a controllere.
/// Figyeli, hogy leesik e a labda és leesne akkor
/// újraterem a pályán valahol random.
/// </summary>

[RequireComponent (typeof (Rigidbody2D))]
[RequireComponent (typeof (CircleCollider2D))]

public class BallController : MonoBehaviour {

	void OnTriggerEnter2D(Collider2D coll){
		//Ha leesik akkor újraterem valahol a pályán
		if (coll.gameObject.tag == "Hole") {
			float rndXPos = Random.Range (9.0f, -9.0f);
			float YPos = 5.0f;
			transform.position = new Vector2 (rndXPos, YPos);
		}
	}

}
