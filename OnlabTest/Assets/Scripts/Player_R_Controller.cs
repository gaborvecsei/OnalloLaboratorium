using UnityEngine;
using System.Collections;

public class Player_R_Controller : PlayerController {

	void Update(){
		//Mindenképp meg kell hívni az alap Update-et hogy mozoghassunk, ugorhassunk stb...
		base.Update ();
		KickAndThrow("Kiutes_r");
		MoveX = MovementX ("Horizontal_r");
		Jumping = IsJumping ("Jump_r");
		UsePowerUp ("EroHasznalas_r");
	}


	void OnTriggerStay2D(Collider2D coll){
		if (!iHaveTheBall && kickTime == 10 && coll.gameObject.tag == "Player_b"
			&& player_b.GetComponent<Player_B_Controller>().iHaveTheBall == true) {
			canKick = true;
		} else {
			canKick = false;
		}
	}
}
