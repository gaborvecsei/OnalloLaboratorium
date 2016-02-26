using UnityEngine;
using System.Collections;

public class Player_B_Controller : PlayerController {

	public override float MovementX(){
		float move = Input.GetAxis ("Horizontal_b");
		return move;
	}

	public override bool Jump(){
		bool jump = Input.GetButtonDown ("Jump_b");
		return jump;
	}
}
