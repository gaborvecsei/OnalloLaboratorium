using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CameraController : MonoBehaviour {

	GameObject player_b;
	GameObject player_r;
	ArrayList players = new ArrayList();

	float minX;
	float maxX;
	float minY;
	float maxY;

	Quaternion rot;
	Vector3 pos;
	public Vector3 angles;

	public float camSpeed;
	public float camDist;

	Vector3 finalLookAt;
	Vector3 cameraBuffer;
	float camSize;

	void Start(){
		
		player_b = GameObject.Find ("Player_b");
		player_r = GameObject.Find ("Player_r");
		players.Add (player_b);
		players.Add (player_r);
	}

	void Update() {

		CalculateBounds();
		CalculateCameraPosAndSize();

	}

	void CalculateBounds(){
		minX = Mathf.Infinity;
		maxX = -Mathf.Infinity;
		minY = Mathf.Infinity;
		maxY = -Mathf.Infinity;
	
		foreach (GameObject player in players) {
			Vector3 tempPlayer = player.transform.position;
			//X Bounds
			if (tempPlayer.x < minX)
				minX = tempPlayer.x;
			if (tempPlayer.x > maxX)
				maxX = tempPlayer.x;
			//Y Bounds
			if (tempPlayer.y < minY)
				minY = tempPlayer.y;
			if (tempPlayer.y > maxY)
				maxY = tempPlayer.y;
		}
	}

	void CalculateCameraPosAndSize() {
		Vector3 cameraCenter = Vector3.zero;

		foreach(GameObject player in players){
			cameraCenter += player.transform.position;
		}

		Vector3 finalCameraCenter = cameraCenter / players.Count;
		//Rotates and Positions camera around a point
		rot = Quaternion.Euler(angles);
		pos = rot * new Vector3(0f, 0f, -camDist) + finalCameraCenter; 
		this.transform.rotation = rot;
		this.transform.position = Vector3.Lerp(transform.position, pos, camSpeed * Time.deltaTime);
		finalLookAt = Vector3.Lerp (finalLookAt, finalCameraCenter, camSpeed * Time.deltaTime);
		this.transform.LookAt(finalLookAt);
		//Size
		float sizeX = maxX - minX + cameraBuffer.x;
		float sizeY = maxY - minY + cameraBuffer.y;
		camSize = (sizeX > sizeY ? sizeX : sizeY);
		Camera.main.orthographicSize = camSize * 0.5f;
	}
}
