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
    private bool faceingLeft = true;
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
	private RaycastHit2D materialCheck;
	public LayerMask materialCheckMask;

	public bool iHaveTheBall = false;
	public int ballHoldTime = 100;

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
	public virtual bool Jump(){
		bool jump = Input.GetButtonDown ("Jump_b");
		return jump;
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
		jumping = Jump ();
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
		materialCheck = Physics2D.Raycast (transform.position, -Vector2.up, raycastDistance, materialCheckMask);
		//Kirajzoljuk a képernyőre
		Debug.DrawRay (transform.position, -Vector2.up * raycastDistance, Color.green);

		if(materialCheck.collider == null) {
			//rb.velocity = new Vector2 (moveX * MaxSpeed, rb.velocity.y);
			rb.AddForce (new Vector2 (moveX * MaxSpeed, 0));
		} else if ((materialCheck.collider != null) && (materialCheck.collider.tag == "NormalGround")) {
			rb.velocity = new Vector2 (moveX * MaxSpeed, rb.velocity.y);
		} else if ((materialCheck.collider != null) && (materialCheck.collider.tag == "SlipperyGround")) {
			rb.AddForce (new Vector2 (moveX * MaxSpeed, 0));
		}
	}

	void OnCollisionEnter2D(Collision2D coll){
		if (coll.gameObject.tag == "Ball") {
			Destroy (coll.gameObject);
			iHaveTheBall = true;
		}
	}

	IEnumerator CountDown(){
		if (ballHoldTime >= 0) {
			ballHoldTime--;
		}
		yield return new WaitForSeconds (1);
	}

}
