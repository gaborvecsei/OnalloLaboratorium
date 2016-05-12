using UnityEngine;
using System.Collections;

public class Player_B_Controller : PlayerController {

	void Update(){
		//Mindenképp meg kell hívni az alap Update-et hogy mozoghassunk, ugorhassunk stb...
		base.Update ();
		//Ezután meghívjuk azt a metódust amivel el lehet dobni a labdát, ill. kiütni a másiktól
		KickAndThrow("Kiutes_b");
		MoveX = MovementX ("Horizontal_b");
		Jumping = IsJumping ("Jump_b");
		UsePowerUp ("EroHasznalas_b");
	}

	//Mind a két játékosnál van egy trigger collider
	//Ha azzal ütközünk és teljesülnek a feltételek, akkor kiüthetjük a labdát tőlük
	void OnTriggerStay2D(Collider2D coll){
		if (!iHaveTheBall && kickTime == 10 && coll.gameObject.tag == "Player_r"
		    && player_r.GetComponent<Player_R_Controller> ().iHaveTheBall == true) {
			canKick = true;
		} else {
			canKick = false;
		}
	}

}
