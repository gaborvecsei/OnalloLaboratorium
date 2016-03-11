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

	protected bool iHaveTheBall = false;
	private int ballHoldTime = 100;
	float timeCount;

	//Ez jelzi hogy kinél van a labda
	public GameObject ballParticle;
	public GameObject ballGo;

	void Start()
	{
		rb = GetComponent<Rigidbody2D> ();
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

	//A megszerzett labdát el lehet vele hajítani
	public virtual void BallThrowing(){
		if (Input.GetButtonDown ("Kiutes_b")) {
			//Csak akkor dobhatjuk el ha nálunk is van
			if (iHaveTheBall) {
				//Referencia a labda GameObject-re
				GameObject go;
				//Attól függően, hogy jobbra vagy balra nézünk, olyan irányba dobjuk a labdát
				if (faceingLeft) {
					//Kicsit távolabb létrehozzuk a GameObject-et, hogy ne ütközzön megint a Collider-ünkkel
					go = Instantiate (ballGo, transform.position + new Vector3 (-1, 0, 0), Quaternion.identity) as GameObject;
					//Erő ráhatás a hajítás érdekében
					go.GetComponent<Rigidbody2D> ().AddForce (new Vector2 (-500, 500));
				} else {
					go = Instantiate (ballGo, transform.position + new Vector3 (1, 0, 0), Quaternion.identity) as GameObject;
					go.GetComponent<Rigidbody2D> ().AddForce (new Vector2 (500, 500));
				}
				//Ha már nincs nálunk, mivel eldobtuk, akkor az azt jelző particle-t ki kell törülnünk
				Destroy(transform.Find("BallParticle(Clone)").gameObject);
				//És most már nincs is nálunk a labda
				iHaveTheBall = false;
			}
		}
	}

		
	//Inputot kezeli és ami nem a fizikával kapcsolatos
	void Update()
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
		BallThrowing();

		//Visszaszámol másodpercenként amíg nálunk van a labda
		if(iHaveTheBall && Time.time >= (timeCount+1f)){
			timeCount = Time.time;
			ballHoldTime--;
			Debug.Log (gameObject.name + ": " + ballHoldTime);

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

	//A játékos mozgatása, úgy hogy figyelembe veszi a platformok anyagát
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

	IEnumerator CountDown(){
		if (ballHoldTime >= 0) {
			ballHoldTime--;
		}
		yield return new WaitForSeconds (1);
	}

}
