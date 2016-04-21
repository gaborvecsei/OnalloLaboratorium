using UnityEngine;
using System.Collections;

public class BoxSpawningController : MonoBehaviour {

	//A scene-n belül hol jelenhet meg
	//publikus, hogy könnyebben lehessen beállítani az editorban
	public float spwPosXMax;
	public float spwPosXMin;
	//Milyen gyakran eshet le doboz
	float spwTime;
	//A doboz prefabja
	public GameObject boxPref;
	//Tároljuk az összes ledobott Box-ot ami még a pályán van
	public ArrayList activeBoxesList = new ArrayList();


	void Start () {
		StartCoroutine (WaitForSpawn ());
	}

	//Ezzel hozzuk létre az új dobozt amiben valamilyen erő van
	void BoxSpawning(){
		//Random pozícióba generálása
		float rndXPos = Random.Range (spwPosXMin, spwPosXMax);
		//Random pozícióba dobjuk le de olyan magasságból ahol a Spawner GameObject van (nem látható)
		Vector2 dropPos = new Vector2 (rndXPos, transform.position.y);

		//Így lehet betölteni egy GameObjectet Editor nélkül
		//Object prefab = AssetDatabase.LoadAssetAtPath("Assets/Prefabs/Box.prefab", typeof(GameObject));

		//Létrehozása az új GameObject-nek
		GameObject go = Instantiate (boxPref, dropPos, Quaternion.identity) as GameObject;
		activeBoxesList.Add (go);
		//Ha túl sok doboz lennea pályán lent
		DestroyOldBox();
		//Indulhat újra a visszaszámlálás
		StartCoroutine (WaitForSpawn ());

	}

	//Megelőzi, hogy túl sok Box legyen a scene-n
	void DestroyOldBox(){
		if (activeBoxesList.Count >= 4) {
			int listSize = activeBoxesList.Count;
			GameObject go = activeBoxesList [0] as GameObject;
			Destroy(go);
			activeBoxesList.RemoveAt (0);
		}
	}

	//Coroutine ami vár random ideig aztán meghívja azt a függvényt ami elkészít egy dobozt
	IEnumerator WaitForSpawn(){
		spwTime = Random.Range (3, 5);
		yield return new WaitForSeconds (spwTime);
		BoxSpawning ();
	}
}
