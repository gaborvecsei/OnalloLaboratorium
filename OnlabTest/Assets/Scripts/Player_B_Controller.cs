using UnityEngine;
using System.Collections;

public class Player_B_Controller : PlayerController {

	public override float MovementX(){
		float move = Input.GetAxis ("Horizontal_b");
		return move;
	}

	public override bool Jumping(){
		bool jump = Input.GetButtonDown ("Jump_b");
		return jump;
	}
		
	public override void BallThrowing(){
		if (Input.GetButtonDown ("Kiutes_b")) {
			if (iHaveTheBall) {
				GameObject go;
				if (faceingLeft) {
					go = Instantiate (ballGo, transform.position + new Vector3 (-1, 0, 0), Quaternion.identity) as GameObject;
					go.GetComponent<Rigidbody2D> ().AddForce (new Vector2 (-500, 500));
				} else {
					go = Instantiate (ballGo, transform.position + new Vector3 (1, 0, 0), Quaternion.identity) as GameObject;
					go.GetComponent<Rigidbody2D> ().AddForce (new Vector2 (500, 500));
				}
				Destroy(transform.Find("BallParticle(Clone)").gameObject);
				iHaveTheBall = false;
			}
		}
	}

}
