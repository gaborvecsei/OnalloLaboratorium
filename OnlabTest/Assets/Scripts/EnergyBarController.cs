using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Ez mutatja, hogy kinek mennyi pontja gyűlt össze,
/// azaz, hogy meddig volt nála a labda.
/// </summary>

public class EnergyBarController: MonoBehaviour {

	private GameObject player_r;
	private GameObject player_b;
	//Sliderrel olduk meg az energia grafikus megmutatását
	public Slider energyBar_r;
	public Slider energyBar_b;
	public Slider gameTimeBar;

	void Start(){
		//Azért kell, hogy ugyan annyi legyen a max értéke mint a játékidőnek
		gameTimeBar.maxValue = GameManager.gameTime;
		energyBar_r.maxValue = GameManager.maxBallHoldTime;
		player_r = GameObject.Find ("Player_r");
		player_b = GameObject.Find ("Player_b");
	}

	//Mindkét játékosnál "lekérjük", az adatot hogy meddig volt a labda
	//Aztán azt megjelenítjük a Slider-en
	void Update(){
		
		energyBar_r.value = player_r.GetComponent<PlayerController> ().BallHoldTime;
		energyBar_b.value = player_b.GetComponent<PlayerController> ().BallHoldTime;
		int gameTimeValue = GameManager.gameTime;
		gameTimeBar.value = gameTimeValue;
	}
}