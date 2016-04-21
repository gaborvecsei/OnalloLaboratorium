using UnityEngine;
using System.Collections;

[RequireComponent (typeof (Rigidbody2D))]

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

	//private string[] powerUps = { "Speed", "FreezeTime", "HighJump" };
	private string[] powerUps = { "FreezeTime", "FreezeTime", "FreezeTime" };
	public string powerUp = "None";
	public bool gotPowerUp = false;
	private float timeCount_powerUp;
	public int powerUpTime = 0;

	void Start()
	{
		player_r = GameObject.Find ("Player_r");
		player_b = GameObject.Find ("Player_b");
		rb = GetComponent<Rigidbody2D> ();
		//Megnézi, hogy mi kik vagyunk és 
		string myName = this.gameObject.name;
		if (myName == "Player_r")
			otherPlayer = player_b;
		else
			otherPlayer = player_r;

	}

    //A sprite a másik irányba nézését oldja meg
    void Flip()
    {
        faceingLeft = !faceingLeft;
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }

	//Az X irányú mozgatást figyeli
	//Ez a metódus van meghívva a különböző színű játékosoknál
	public float MovementX(string inputButton){
		float move = Input.GetAxis (inputButton);
		return move;
	}

	//Az ugrást figyeli
	//Ez a metódus van meghívva a különböző színű játékosoknál
	public bool IsJumping(string inputButton){
		bool jump = Input.GetButtonDown (inputButton);
		return jump;
	}
		
	//Inputot kezeli és ami nem a fizikával kapcsolatos
	//publikussá tettem, hogy a leszármazottak is lássák, és meg tudják hívni
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

		//5 sec elteltével már elmúlik az erő
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

	//Fizikai része van ebben benne
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

	//A játékos mozgatása, úgy hogy figyelembe veszi a platformok anyagát (kell hozzá a MovementX())
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
		}else if ((materialCheckBase.collider != null) && (materialCheckBase.collider.tag == "NormalGround")) {
			//Ha sima platformon állunk akkor nem kell erőbehatás
			rb.velocity = new Vector2 (MoveX * MaxSpeed, rb.velocity.y);
		} else if ((materialCheckBase.collider != null) && (materialCheckBase.collider.tag == "SlipperyGround")) {
			//Viszont ha csúszós platformon állunk akkor kell erő hatás, mivel így érvényesül a Physics material
			rb.AddForce (new Vector2 (MoveX * MaxSpeed, 0));
		} else {
			rb.velocity = new Vector2 (MoveX * MaxSpeed, rb.velocity.y);
		}
	}

	//Ütközés detektálása
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

	//Miután felszedtük a dobozt ez generál nekünk valamilyen erőt
	public string GenerateRandomPower(){
		string power = powerUps [Random.Range (0, powerUps.Length)];
		return power;
	}

	//Leellenőrzi, hogy milyen erő van nálunk és a szerint cselekszik
	//Az időt amíg az erő megvan azt külön külön állíthatjuk be
	public void CheckMyPowerUp(){
		switch (powerUp){
		case "Speed":
			MaxSpeed = 10f;
			powerUpTime = 5;
			break;
		case "FreezeTime":
			//A másik játékos számára megállítja az időt
			otherPlayer.GetComponent<Rigidbody2D> ().constraints = RigidbodyConstraints2D.FreezeAll;
			powerUpTime = 10;
			break;
		case "HighJump":
			//Nagyobbat tud ugrani
			JumpForce = 850f;
			powerUpTime = 5;
			break;
		default:
			MaxSpeed = 5f;
			JumpForce = 600f;
			otherPlayer.GetComponent<Rigidbody2D> ().constraints = RigidbodyConstraints2D.FreezeRotation;
			break;
		}
	}

	//Reset the thing to the default
	public void ResetPower(){
		gotPowerUp = false;
		MaxSpeed = 5f;
		JumpForce = 600f;
		powerUp = "None";
		otherPlayer.GetComponent<Rigidbody2D> ().constraints = RigidbodyConstraints2D.FreezeRotation;
	}

	//Ha van PowerUp-unk és lenyomjuk a gombot akkor használjuk csak el
	public void UsePowerUp(string inputStr){
		if (Input.GetButtonDown (inputStr) && powerUp != "None") {
			gotPowerUp = true;
			CheckMyPowerUp ();
		}
	}

	//A labda eldobásáért és a kiütéséért felelős metódus
	public void KickAndThrow(string inputStr, GameObject otherPlayer){
		if (Input.GetButtonDown (inputStr)) {
			if (iHaveTheBall) {
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
			//Ha nincs nálunk a labda és jelezve van, hogy ki tudom ütni akkor üssük ki a másiktól a labdát
			else if(!iHaveTheBall && canKick){
				GameObject go;
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
				//Meg is kell semmisíteni a jelző particle-t
				Destroy(GameObject.FindGameObjectWithTag("BallParticle"));
			}
		}
	}

}
