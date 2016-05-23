using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/// <summary>
/// Game manager.
/// Figyeli, hogy ki a nyertes és hogy mennyi még a játékidő
/// </summary>

public class GameManager : MonoBehaviour {

	//Ezen a text-en jelenítjük meg a nyertest
	public Text winnerText;
	public Text winnerLabel;
	public Text restartLabel;
	//Játékidő - 200másodperc
	public static int gameTime = 200;
	public static int maxBallHoldTime = 100;
	float timeCount = 0f;
	public GameObject player_r;
	public GameObject player_b;
	int score_r = 0;
	int score_b = 0;

	GameObject[] players;

	Color blue = new Color (0,174,239);
	Color red = new Color (237,28,36);
	Color green = new Color (36,221,11);


	void Start(){
		winnerText.enabled = false;
		winnerLabel.enabled = false;
		restartLabel.enabled = false;
	}

	void Update () {
		score_r = player_r.GetComponent<PlayerController> ().BallHoldTime;
		score_b =player_b.GetComponent<PlayerController> ().BallHoldTime;

		//Közben folyton figyeljük, hogy nyert e már valaki
		if (score_r >= maxBallHoldTime) {
			showWinnerText ("RED", red);
		} else if (score_b >= maxBallHoldTime) {
			showWinnerText ("BLUE", blue);
		}

		if (gameTime > 0) {
			if (Time.time >= (timeCount + 1f)) {
				timeCount = Time.time;
				gameTime--;
			}
		} else {
			//Megnézzük, hogy ki a nyertes
			CheckTheWinner ();
		}

		if (Input.GetKeyDown (KeyCode.Return)) {
			Application.LoadLevel ("MainScene");
		}
	}

	/// <summary>
	/// Checks the winner.
	/// </summary>
	void CheckTheWinner(){
		if (score_b > score_r ) {
			showWinnerText ("BLUE", blue);
		} else if (score_r > score_b) {
			showWinnerText ("RED", red);
		} else {
			showWinnerText ("DRAW", green);
		}
	}

	/// <summary>
	/// Shows the winner text.
	/// </summary>
	/// <param name="winner">Winner.</param>
	/// <param name="winnerColor">Winner's color</param>
	void showWinnerText(string winner, Color winnerColor){
		//Megállítjuk a játékot
		Time.timeScale = 0;
		winnerText.color = winnerColor;
		winnerText.text = winner;
		winnerText.enabled = true;
		winnerLabel.enabled = true;
		restartLabel.enabled = true;
	}
}
