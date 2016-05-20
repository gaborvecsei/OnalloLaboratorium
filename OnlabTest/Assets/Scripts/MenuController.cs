using UnityEngine;
using System.Collections;

/// <summary>
/// Menu controller.
/// Ez csak figyeli, hogy elindítottuk e a játékot
/// </summary>

public class MenuController : MonoBehaviour {
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown (KeyCode.KeypadEnter)) {
			Application.LoadLevel ("MainScene");
		}
	}
}
