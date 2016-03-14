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
		
	void Update(){
		//Mindenképp meg kell hívni az alap Update-et hogy mozoghassunk, ugorhassunk stb...
		base.Update ();
		//Ezután meghívjuk azt a metódust amivel el lehet dobni a labdát, ill. kiütni a másiktól
		KickAndThrow("Kiutes_b",Player_r);
	}

	//Mind a két játékosnál van egy trigger collider
	//Ha azzal ütközünk és teljesülnek a feltételek, akkor kiüthetjük a labdát tőlük
	void OnTriggerStay2D(Collider2D coll){
		if (!iHaveTheBall && kickTime == 10 && coll.gameObject.tag == "Player_r"
		    && Player_r.GetComponent<Player_R_Controller> ().iHaveTheBall == true) {
			canKick = true;
		} else {
			canKick = false;
		}
	}

}
