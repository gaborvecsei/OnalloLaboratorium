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
	//Y irányú elmozdulásért felelős
	private float moveY = 0f;
	//Ha true akkor ugrás van
	private bool jumping = false;

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

		//Ha így mozgatom akkor nem hat rá a physics material
		rb.velocity = new Vector2 (moveX * MaxSpeed, rb.velocity.y);
		/*//Így hat a mozgására a phy mat (tehát tud csúszni)
		if (rb.velocity.x <= 5 && rb.velocity.x >= -5) {
			rb.AddForce (new Vector2 (moveX * MaxSpeed * 5, 0));
		}*/


	}

    

	



}
