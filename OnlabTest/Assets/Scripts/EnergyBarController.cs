using UnityEngine;
using UnityEngine.UI;

public class EnergyBarController: MonoBehaviour {

	private GameObject player_r;
	private GameObject player_b;
	public Slider energyBar_r;
	public Slider energyBar_b;


	void Update(){
		player_r = GameObject.Find ("Player_r");
		player_b = GameObject.Find ("Player_b");
		energyBar_r.value = player_r.GetComponent<PlayerController> ().BallHoldTime;
		energyBar_b.value = player_b.GetComponent<PlayerController> ().BallHoldTime;
	}
}