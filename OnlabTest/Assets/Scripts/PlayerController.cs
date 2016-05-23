using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[RequireComponent (typeof (Rigidbody2D))]
[RequireComponent (typeof (Transform))]
//Kell ahhoz, hogy a platformon maradjon
[RequireComponent (typeof (BoxCollider2D))]
//Kell ahhoz, hogy a másik játékostól ki tudja ütnia  labdát
[RequireComponent (typeof (CircleCollider2D))]

/// <summary>
/// Játékosok "alap" osztálya.
/// Itt kerül megvalósításra minden ami velük kapcsolatos,
/// Ezután az itt megvalósított függvényeket hívják meg a saját osztájukban a saját
/// inputjukkal.
/// </summary>
public class PlayerController : MonoBehaviour {

	//Játékos mozgási sebessége
    private float maxSpeed = 5f;
	public float MaxSpeed
	{
		get { return maxSpeed;}
		set { maxSpeed = value;}
	}
	//X irányú elmozdulása a játékosnak
	private float moveX = 0f;
	public float MoveX{
		get { return moveX;}
		set { moveX = value;}
	}
    //Merre néz a játékos
    protected bool faceingLeft = true;
    //A talajjal érintkezik e
    private bool isGrounded = false;
    //Ez a segéd Gameobject ami megnézi, hogy a talajon van e
    public Transform groundcheck;
    //Mit tekintünk a földnek, az editorban, több mindent is megadhatunk
    public LayerMask whatIsGround;
    //Milyen erővel ugorjon el a játékos
    private float jumpForce = 600f;
    public float JumpForce
    {
        get { return jumpForce; }
        set { jumpForce = value; }
    }
	//Ha true akkor ugrás van
	private bool jumping = false;
	public bool Jumping {
		get {return jumping;}
		set {jumping = value;}
	}

	[HideInInspector]
	public Rigidbody2D rb;
	//Megnézi, hogy milyen fajta platformon vagyunk: (normális, csúszós, ragadós stb)
	private RaycastHit2D materialCheckBase;
	public LayerMask materialCheckMask;
	//Nálunk van e a labda vagy nem
	public bool iHaveTheBall = false;
	//Ez nő amikor nálunk van a labda
	private int ballHoldTime = 0;
	public int BallHoldTime
	{
		get { return ballHoldTime; }
		set { ballHoldTime = value; }
	}
	//Számlálók
	float timeCount_hold;
	float timeCount_kick;
	//Ennyi időnek kell eltelnie 2 ütés között
	public int kickTime = 10;
	//Mikor true lesz akkor lehet a másiktól elütni a labdát
	public bool canKick = false;
	//Ez jelzi hogy kinél van a labda
	public GameObject ballParticle;
	public GameObject ballGo;

	protected GameObject player_r;
	protected GameObject player_b;
	private GameObject otherPlayer;
	//Power up-ok listája
	private string[] powerUps = { "Speed", "FreezeTime", "HighJump" };
	//Aktuális power up amivel rendelkezik
	private string powerUp = "None";
	private bool gotPowerUp = false;
	private float timeCount_powerUp;
	private int powerUpTime = 0;

	private GameObject mainCamera;
	private Transform thisTransform;

	//Editorban lehet hozzáadni
	//Ezek jelennek meg amikr felvettünk egy erőt
	public Sprite powerUpSprt_speed;
	public Sprite powerUpSprt_highjump;
	public Sprite powerUpSprt_freezetime;
	public Image powerUpImage;

	public GameObject kickBallAnim;
	private TrailRenderer trail;


	/// <summary>
	/// Start this instance.
	/// </summary>
	void Start()
	{
		mainCamera = GameObject.Find ("Main Camera");
		player_r = GameObject.Find ("Player_r");
		player_b = GameObject.Find ("Player_b");
		rb = GetComponent<Rigidbody2D> ();
		//Megnézi, hogy mi kik vagyunk és 
		string myName = this.gameObject.name;
		if (myName == "Player_r")
			otherPlayer = player_b;
		else
			otherPlayer = player_r;
		
		//A játékosok saját magukat adják hozzá a listához
		thisTransform = GetComponent<Transform> ();
		mainCamera.GetComponent<CameraController> ().targets2.Add (thisTransform);
		powerUpImage = powerUpImage.GetComponent<Image> ();
		powerUpImage.enabled = false;

		//Először nem kell, hogy csíkot húzzon maga után csak ha gyorsasága van
		trail = GetComponent<TrailRenderer> ();
		trail.enabled = false;

	}

    /// <summary>
	/// A sprite a másik irányba nézését oldja meg
    /// </summary>
    void Flip()
    {
        faceingLeft = !faceingLeft;
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }
		
	/// <summary>
	/// Az X irányú mozgatást figyeli
	/// Ez a metódus van meghívva a különböző színű játékosoknál
	/// </summary>
	/// <returns>Value of movement</returns>
	/// <param name="inputButton">játékos Input button-ja</param>
	public float MovementX(string inputButton){
		float move = Input.GetAxis (inputButton);
		return move;
	}
		
	/// <summary>
	/// Az ugrást figyeli.
	/// Ez a metódus van meghívva a különböző színű játékosoknál.
	/// </summary>
	/// <returns><c>true</c> ha ugrik éppen a játékos, egyébként <c>false</c>.</returns>
	/// <param name="inputButton">Input button.</param>
	public bool IsJumping(string inputButton){
		bool jump = Input.GetButtonDown (inputButton);
		return jump;
	}
		
	/// <summary>
	/// Inputot kezeli és ami nem a fizikával kapcsolatos.
	/// Publikussá tettem, hogy a leszármazottak is lássák, és meg tudják hívni.
	/// 
	/// </summary>
	public void Update()
	{
		isGrounded = Physics2D.OverlapCircle (groundcheck.transform.position, 0.3f, whatIsGround);
		//Mindig oda nézzen amerre megy
		if (MoveX > 0 && faceingLeft)
		{
			Flip ();
		}
		else if (MoveX < 0 && !faceingLeft)
		{
			Flip ();
		}

		//Visszaszámol másodpercenként amíg nálunk van a labda
		if(iHaveTheBall && Time.time >= (timeCount_hold+1f)){
			timeCount_hold = Time.time;
			ballHoldTime++;
			//Debug.Log (gameObject.name + ": " + ballHoldTime);

		}

		if (!iHaveTheBall && Time.time >= (timeCount_kick+1f) && kickTime < 10) {
			timeCount_kick = Time.time;
			kickTime++;
			//Debug.Log (gameObject.name + ": " + kickTime);
		}

		//X sec elteltével már elmúlik az erő
		if (gotPowerUp && Time.time >= (timeCount_powerUp + 1f)) {
			timeCount_powerUp = Time.time;
			powerUpTime--;
			//Ha túllépi az idő korlátot
			if (powerUpTime < 0) {
				ResetPower ();
				powerUpTime = 5;
			}
		}
	}

	/// <summary>
	/// Fizikai rész van ebben
	/// </summary>
	void FixedUpdate()
	{
		//Ugrik a játékos
		if (jumping == true && isGrounded == true) {
			rb.AddForce (new Vector2 (0, JumpForce));
			//Ne tudjon még 1X ugrani
			jumping = false;
		}

		//Játékos mozgása
		Mooving ();

	}

	/// <summary>
	/// A játékos mozgatása, úgy hogy figyelembe veszi
	/// a platformok anyagát (kell hozzá a MovementX())
	/// </summary>
	private void Mooving(){
		//Megnézi, hogy milyen fajta talaj van alatta RayCasttal
		float raycastDistance = 0.8f;
		materialCheckBase = Physics2D.Raycast (transform.position, -Vector2.up, raycastDistance, materialCheckMask);
		RaycastHit2D materialCheckRight = Physics2D.Raycast (transform.position + new Vector3(0.3f,0,0), -Vector2.up, raycastDistance, materialCheckMask);
		RaycastHit2D materialCheckLeft = Physics2D.Raycast (transform.position + new Vector3(-0.3f,0,0), -Vector2.up, raycastDistance, materialCheckMask);

		//Kirajzoljuk a képernyőre a materialCheckBase-t, materialCheckRight-et, materialCheckLeft-et
		Debug.DrawRay (transform.position, -Vector2.up * raycastDistance, Color.green);
		Debug.DrawRay (transform.position + new Vector3(0.3f,0,0), -Vector2.up * raycastDistance, Color.green);
		Debug.DrawRay (transform.position + new Vector3(-0.3f,0,0), -Vector2.up * raycastDistance, Color.green);

		//Hogyha nincs alattunk semmi, de jobbról vagy balról igen, akkor biztosra vehetjük, hogy egy platform szélén állunk
		if ((materialCheckBase.collider == null && materialCheckRight.collider != null)
			|| (materialCheckBase.collider == null && materialCheckLeft.collider != null)) {
			rb.AddForce (new Vector2 (MoveX * MaxSpeed, 0));
		} else if ((materialCheckBase.collider != null) && (materialCheckBase.collider.tag == "NormalGround")) {
			//Ha sima platformon állunk akkor nem kell erőbehatás
			rb.velocity = new Vector2 (MoveX * MaxSpeed, rb.velocity.y);
		} else if ((materialCheckBase.collider != null) && (materialCheckBase.collider.tag == "SlipperyGround")) {
			//Viszont ha csúszós platformon állunk akkor kell erő hatás, mivel így érvényesül a Physics material
			rb.AddForce (new Vector2 (MoveX * MaxSpeed, 0));
		} else if ((materialCheckBase.collider != null) && (materialCheckBase.collider.tag == "StickyGround")) {
			//Viszont ha csúszós platformon állunk akkor kell erő hatás, mivel így érvényesül a Physics material
			rb.AddForce (new Vector2 (MoveX * MaxSpeed, 0));
		} else {
			rb.velocity = new Vector2 (MoveX * MaxSpeed, rb.velocity.y);
		}
	}

	/// <summary>
	/// Raises the collision enter2 event.
	/// </summary>
	/// <param name="coll">Coll.</param>
	void OnCollisionEnter2D(Collision2D coll){
		//Ha a labdával ütközünk
		if (coll.gameObject.tag == "Ball") {
			Destroy (coll.gameObject);
			iHaveTheBall = true;
			GameObject go;
			//Létrehozunk egy particle-t ami azt jelzi, hogy kinél vana  labda
			go = Instantiate (ballParticle, transform.position, Quaternion.identity) as GameObject;
			//Beállítjuk, hogy a gyereke legyen a játékosnak így a pozíciójuk meg fog egyezni, és együtt mozognak majd
			go.transform.parent = this.transform;
		}

		//Mikor egy erőt tartalmazó dobozzal ütközünk
		if (coll.gameObject.tag == "Box") {
			Destroy (coll.gameObject);
			powerUp = GenerateRandomPower ();
		}
	}

	void OnTriggerEnter2D(Collider2D coll){
		//Ha leesünk a lyukba
		if (coll.gameObject.tag == "Hole") {
			//Ha van nálunk labda akkor elejtjük
			DropTheBall ();
			StartCoroutine (WaitForRespawn());
		}
	}

	/// <summary>
	/// Kiveszi magát a kamera listájából, hogy őt ne kövesse,
	/// Kiszámolja  a random újraéledési helyet,
	/// Az idő letelte után beállítja a pozícióját az új helyre és
	/// Újra hozzáadja megát a kamera listájához.
	/// </summary>
	IEnumerator WaitForRespawn(){
		//Megálljon egyhelyben
		GetComponent<Rigidbody2D> ().constraints = RigidbodyConstraints2D.FreezeAll;
		//Ne lássuk a sprite-ot a játékban
		gameObject.GetComponent<SpriteRenderer> ().enabled = false;
		//Nem kell a kamerának követnie
		mainCamera.GetComponent<CameraController> ().targets2.Remove (thisTransform);
		//Ez a várakozási idő amíg nem kerül vissza a játékba
		float waitTime = 3.0f;
		//Randm X pozíció
		float rndXPos = Random.Range (9.0f, -9.0f);
		//Állandó y pozíció, mivel egy magasságból akarjuk csak leejteni
		float YPos = 5.0f;
		//Várunk
		yield return new WaitForSeconds (waitTime);
		//Új pozíciót adunk neki (random)
		transform.position = new Vector2 (rndXPos, YPos);
		//Újra látható miután letelt az idő
		gameObject.GetComponent<SpriteRenderer> ().enabled = true;
		//Újra követheti a kamera
		mainCamera.GetComponent<CameraController> ().targets2.Add (thisTransform);
		//A rotationt mindig freezelni kell, nehogy eldőljön
		GetComponent<Rigidbody2D> ().constraints = RigidbodyConstraints2D.FreezeRotation;
	}

	/// <summary>
	/// Generál egy erőt miután felszedtük egy dobozt
	/// </summary>
	/// <returns>Random Power Up</returns>
	public string GenerateRandomPower(){
		//Meg kell jelenítseni a képet
		powerUpImage.enabled = true;
		//Random szám 0 és 100 között
		int rnd = Random.Range (0, 100);
		//Külömböző véletlenszerűséggel adunk erőt
		//Kissebb valószínűséggel adunk idő megállítást
		if (rnd < 20) {
			powerUpImage.sprite = powerUpSprt_freezetime;
			return powerUps [1];
		} else if (rnd < 60) {
			powerUpImage.sprite = powerUpSprt_highjump;
			return powerUps [2];
		} else {
			powerUpImage.sprite = powerUpSprt_speed;
			return powerUps [0];
		}
	}
		
	/// <summary>
	/// Leellenőrzi, hogy milyen erő van nálunk és a szerint
	/// cselekszik. Minden erőhöz, meg van adva hogy mit tesz, és külön
	/// beállítható az az idő amíg "él" egy erő a játékosnál.
	/// </summary>
	public void CheckMyPowerUp(){
		//Mivel felhasználtuk így már nem kell megjeleníteni
		powerUpImage.enabled = false;
		switch (powerUp){
		case "Speed":
			MaxSpeed = 10f;
			powerUpTime = 7;
			//Be kell kapcsolni a csík húzását
			trail.enabled = true;
			break;
		case "FreezeTime":
			//A másik játékos számára megállítja az időt
			otherPlayer.GetComponent<Rigidbody2D> ().constraints = RigidbodyConstraints2D.FreezeAll;
			powerUpTime = 5;
			break;
		case "HighJump":
			//Nagyobbat tud ugrani
			JumpForce = 850f;
			powerUpTime = 7;
			break;
		default:
			MaxSpeed = 5f;
			JumpForce = 600f;
			otherPlayer.GetComponent<Rigidbody2D> ().constraints = RigidbodyConstraints2D.FreezeRotation;
			break;
		}
	}

	/// <summary>
	/// Reseteli a Power Upot, minden default lesz
	/// </summary>
	public void ResetPower(){
		gotPowerUp = false;
		MaxSpeed = 5f;
		JumpForce = 600f;
		powerUp = "None";
		otherPlayer.GetComponent<Rigidbody2D> ().constraints = RigidbodyConstraints2D.FreezeRotation;
		powerUpImage.enabled = false;
		//Ki kell kapcsolni a csíkot
		trail.enabled = false;
	}

	/// <summary>
	/// Amikor van Powe Up-unk akkor használhatjuk el.
	/// Ezt a függvényt minden játékos a saját osztályában hívja meg a saját inputtal
	/// </summary>
	/// <param name="inputStr">Editorban beállított input gomb</param>
	public void UsePowerUp(string inputStr){
		if (Input.GetButtonDown (inputStr) && powerUp != "None") {
			gotPowerUp = true;
			CheckMyPowerUp ();
		}
	}

	/// <summary>
	/// A labda eldobásáért feleős. Ezzel a játékosok el tudják dobni a labdát vagy
	/// pedig ki tudják ütni a másiktól (gyan azzal a gombbal)
	/// Ezt a függvényt hívja meg mindegyik játékos a "saját" class-ában, a saját inputjával 
	/// </summary>
	/// <param name="inputStr">Editorban beállított input gomb</param>
	public void KickAndThrow(string inputStr){
		if (Input.GetButtonDown (inputStr)) {
			if (iHaveTheBall) {
				ThrowTheBall ();
			} else if (!iHaveTheBall && canKick) {
				//Ha nincs nálunk a labda és jelezve van, hogy ki tudom ütni akkor üssük ki a másiktól a labdát
				KickTheBall ();
			}
		}
	}

	void KickTheBall(){
		GameObject go;
		GameObject go_kickBallAnim;
		kickTime = 0;
		canKick = false;
		//Ha kiütöttük akkor már nincs a másiknál a labda
		otherPlayer.GetComponent<PlayerController> ().iHaveTheBall = false;
		//Random erő az ütéshez
		Vector2 randomForce = new Vector2 (Random.Range (-250, 250), Random.Range (400, 1000));
		Vector3 otherPlayerPos = otherPlayer.transform.position;
		//Most a labda ott fog teremni ahol a másik játékos van
		go = Instantiate (ballGo, otherPlayerPos + new Vector3 (0, 1f, 0), Quaternion.identity) as GameObject;
		//Az előzőleg kitalált erővel elütjük
		go.GetComponent<Rigidbody2D> ().AddForce (randomForce);
		//Lejátszuk az animációt ami a kiütést szemlélteti
		go_kickBallAnim = Instantiate(kickBallAnim, transform.position, Quaternion.identity) as GameObject;
		//Meg is kell semmisíteni a jelző particle-t
		Destroy(otherPlayer.transform.Find("BallParticle(Clone)").gameObject);
	}

	void ThrowTheBall(){
			GameObject go;
			if (faceingLeft) {
				go = Instantiate (ballGo, transform.position + new Vector3 (-1, 0, 0), Quaternion.identity) as GameObject;
				go.GetComponent<Rigidbody2D> ().AddForce (new Vector2 (-500, 500));
			} else {
				go = Instantiate (ballGo, transform.position + new Vector3 (1, 0, 0), Quaternion.identity) as GameObject;
				go.GetComponent<Rigidbody2D> ().AddForce (new Vector2 (500, 500));
			}
			Destroy (transform.Find ("BallParticle(Clone)").gameObject);
			iHaveTheBall = false;
	}


	/// <summary>
	/// Ha mi birtokoljuk a labdát, akkor ezzel el lehet "ejteni"
	/// (Olyan mintha a másik ütné ki tőlünk)
	/// </summary>
	public void DropTheBall(){
		if (iHaveTheBall) {
			GameObject go;
			//Ha kiütöttük akkor már nincs a másiknál a labda
			iHaveTheBall = false;
			//Random erő az ütéshez
			Vector2 randomForce = new Vector2 (Random.Range (-250, 250), Random.Range (400, 1000));
			go = Instantiate (ballGo, transform.position + new Vector3 (0, 1f, 0), Quaternion.identity) as GameObject;
			//Az előzőleg kitalált erővel elütjük
			go.GetComponent<Rigidbody2D> ().AddForce (randomForce);
			//Meg is kell semmisíteni a jelző particle-t
			Destroy (transform.Find ("BallParticle(Clone)").gameObject);
		}
	}

}
