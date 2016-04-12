using UnityEngine;
using System.Collections;

[RequireComponent (typeof (Rigidbody2D))]

public class PlayerController : MonoBehaviour {

    private float maxSpeed = 5f;

	public float MaxSpeed
	{
		get { return maxSpeed;}
		set { maxSpeed = value;}
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

	[HideInInspector]
	public Rigidbody2D rb;
	//X irányú elmozdulása a játékosnak
	private float moveX = 0f;
	//Ha true akkor ugrás van
	private bool jumping = false;

	//Megnézi, hogy milyen fajta platformon vagyunk: (normális, csúszós, ragadós stb)
	private RaycastHit2D materialCheckBase;
	public LayerMask materialCheckMask;

	//Nálunk van e a labda vagy nem
	public bool iHaveTheBall = false;
	//Ez csökken amikor nálunk van
	private int ballHoldTime = 100;
	float timeCount;
	public int kickTime = 10;
	public bool canKick = false;

	//Ez jelzi hogy kinél van a labda
	public GameObject ballParticle;
	public GameObject ballGo;

	protected GameObject Player_r;
	protected GameObject Player_b;

	void Start()
	{
		rb = GetComponent<Rigidbody2D> ();
		Player_b = GameObject.Find ("Player_b");
		Player_r = GameObject.Find ("Player_r");
	}

    //A sprite a másik irányba nézését oldja meg
    void Flip()
    {
        faceingLeft = !faceingLeft;
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }

	//Azt figyeli, hogy mi a bemenet
	//A leszármazottaknál be lehet állítani, hogy mit figyeljen bemenetnek
	public virtual float MovementX(){
		float move = Input.GetAxis ("Horizontal_b");
		return move;
	}

	//Az ugrás detektálásáért (Inputjáért) felelős
	public virtual bool Jumping(){
		bool jump = Input.GetButtonDown ("Jump_b");
		return jump;
	}
		
	//Inputot kezeli és ami nem a fizikával kapcsolatos
	//publikussá tettem, hogy a leszármazottak is lássák, és meg tudják hívni
	public void Update()
	{
		isGrounded = Physics2D.OverlapCircle (groundcheck.transform.position, 0.3f, whatIsGround);
		//Beolvassuk az X irányú elmozdulásokat
		moveX = MovementX ();
		//Mindig oda nézzen amerre megy
		if (moveX > 0 && faceingLeft)
		{
			Flip ();
		}
		else if (moveX < 0 && !faceingLeft)
		{
			Flip ();
		}

		//Ha megnyomtuk az ugrás gombot akkor ugorhatun
		jumping = Jumping ();

		//Visszaszámol másodpercenként amíg nálunk van a labda
		if(iHaveTheBall && Time.time >= (timeCount+1f)){
			timeCount = Time.time;
			ballHoldTime--;
			//Debug.Log (gameObject.name + ": " + ballHoldTime);

		}

		if (!iHaveTheBall && Time.time >= (timeCount+1f) && kickTime < 10) {
			timeCount = Time.time;
			kickTime++;
			//Debug.Log (gameObject.name + ": " + kickTime);
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
			rb.AddForce (new Vector2 (moveX * MaxSpeed, 0));
		}else if ((materialCheckBase.collider != null) && (materialCheckBase.collider.tag == "NormalGround")) {
			//Ha sima platformon állunk akkor nem kell erőbehatás
			rb.velocity = new Vector2 (moveX * MaxSpeed, rb.velocity.y);
		} else if ((materialCheckBase.collider != null) && (materialCheckBase.collider.tag == "SlipperyGround")) {
			//Viszont ha csúszós platformon állunk akkor kell erő hatás, mivel így érvényesül a Physics material
			rb.AddForce (new Vector2 (moveX * MaxSpeed, 0));
		} else {
			rb.velocity = new Vector2 (moveX * MaxSpeed, rb.velocity.y);
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
