using UnityEngine;
using System.Collections;

public class Player_R_Controller : PlayerController {

	public override float MovementX(){
		float move = Input.GetAxis ("Horizontal_r");

		return move;
	}

	public override bool Jumping(){
		bool jump = Input.GetButtonDown ("Jump_r");
		return jump;
	}

	void Update(){
		//Mindenképp meg kell hívni az alap Update-et hogy mozoghassunk, ugorhassunk stb...
		base.Update ();

		KickAndThrow("Kiutes_r",Player_b);
	}


	void OnTriggerStay2D(Collider2D coll){
		if (!iHaveTheBall && kickTime == 10 && coll.gameObject.tag == "Player_b"
			&& Player_b.GetComponent<Player_B_Controller>().iHaveTheBall == true) {
			canKick = true;
		} else {
			canKick = false;
		}
	}
}
