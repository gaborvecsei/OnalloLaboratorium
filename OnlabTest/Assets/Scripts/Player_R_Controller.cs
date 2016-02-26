using UnityEngine;
using System.Collections;

public class Player_R_Controller : PlayerController {

	public override float MovementX(){
		float move = Input.GetAxis ("Horizontal_r");
		return move;
	}

	public override bool Jump(){
		bool jump = Input.GetButtonDown ("Jump_r");
		return jump;
	}
}
