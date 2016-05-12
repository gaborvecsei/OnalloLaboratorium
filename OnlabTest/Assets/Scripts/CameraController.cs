using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Camera controller.
/// Hozzáadhatunk egy listához gameobjectecet, hogy azoknak a 
/// pozícióját belevéve kalkulálja ki saját pozícióják, így a
/// listában lévő dolgok mindig látszanak a képernyőn.
/// </summary>

[RequireComponent (typeof (Camera))]
[RequireComponent (typeof (RectTransform))]

public class CameraController : MonoBehaviour {

	[HideInInspector]
	public Transform[] targets;
	public ArrayList targets2 = new ArrayList();
	public float boundingBoxPadding = 2f;
	public float minimumOrthographicSize = 8f;
	public float zoomSpeed = 20f;
	Vector3 velocity = Vector3.zero;
	public float smoothTime = 0.3f;

	//Meg lehet adni, hogy a kamera meddig mozoghasson a térben
	//Bizonyos határokat így nem tud átlépni
	public bool minMaxValues = false;
	public float maxX = Mathf.Infinity;
	public float minX = 0;
	public float maxY = Mathf.Infinity;
	public float minY = 0;

	Camera camera;

	void Awake ()
	{
		camera = GetComponent<Camera>();
		camera.orthographic = true;
		targets [0] = GameObject.Find ("Player_b").GetComponent<Transform> ();
		targets [1] = GameObject.Find ("Player_r").GetComponent<Transform> ();
	}

	void LateUpdate()
	{
		Rect boundingBox = CalculateTargetsBoundingBox();
		//Beállítjuk folyton a kamera pozícióját
		//De úgy hogy nem folyton változik hanem szépen odamegy valamennyi idő alatt amit megadhatunk
		transform.position = Vector3.SmoothDamp(transform.position, CalculateCameraPosition(boundingBox), ref velocity, smoothTime);
		//Beállítjuk folyton a kamera méretét
		camera.orthographicSize = CalculateOrthographicSize(boundingBox);
	}

	//Egy olyan "dobozt" készít amiben benne van az összes figyelt GameObject
	//Itt például a két játékos
	Rect CalculateTargetsBoundingBox()
	{
		float minX = Mathf.Infinity;
		float maxX = Mathf.NegativeInfinity;
		float minY = Mathf.Infinity;
		float maxY = Mathf.NegativeInfinity;

		foreach (Transform target in targets2) {
			Vector3 position = target.position;

			minX = Mathf.Min(minX, position.x);
			minY = Mathf.Min(minY, position.y);
			maxX = Mathf.Max(maxX, position.x);
			maxY = Mathf.Max(maxY, position.y);
		}
		return Rect.MinMaxRect(minX - boundingBoxPadding, maxY + boundingBoxPadding, maxX + boundingBoxPadding, minY - boundingBoxPadding);
	}

	/// <summary>
	/// A camera pozícióját számolja ki, azaz annak közép pontját, hogy hol legyen
	/// </summary>
	/// <returns>The position of the camera</returns>
	/// <param name="boundingBox">Bounding box.</param>
	Vector3 CalculateCameraPosition(Rect boundingBox)
	{
		Vector2 boundingBoxCenter = boundingBox.center;
		Vector3 newCameraPosition = new Vector3(boundingBoxCenter.x, boundingBoxCenter.y, camera.transform.position.z);

		if (minMaxValues == true) {
			//Nem léphet át bizonyos határokat a kamera pozíciója
			if (newCameraPosition.x > maxX)
				newCameraPosition.x = maxX;
			if (newCameraPosition.x < minX)
				newCameraPosition.x = minX;
			if (newCameraPosition.y > maxY)
				newCameraPosition.y = maxY;
			if (newCameraPosition.y < minY)
				newCameraPosition.y = minY;
		}

		return newCameraPosition;
	}

	/// <summary>
	/// A kamera ortografikus méretét számolja ki
	/// </summary>
	/// <returns>The orthographic size.</returns>
	/// <param name="boundingBox">Bounding box.</param>
	float CalculateOrthographicSize(Rect boundingBox)
	{
		float orthographicSize = camera.orthographicSize;
		Vector3 topRight = new Vector3(boundingBox.x + boundingBox.width, boundingBox.y, 0f);
		Vector3 topRightAsViewport = camera.WorldToViewportPoint(topRight);

		if (topRightAsViewport.x >= topRightAsViewport.y)
			orthographicSize = Mathf.Abs(boundingBox.width) / camera.aspect / 2f;
		else
			orthographicSize = Mathf.Abs(boundingBox.height) / 2f;

		return Mathf.Clamp(Mathf.Lerp(camera.orthographicSize, orthographicSize, Time.deltaTime * zoomSpeed), minimumOrthographicSize, Mathf.Infinity);
	}
}