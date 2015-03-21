using UnityEngine;
using System.Collections;

public class Killer_Mover : AI_Mover {

	protected StatusUpdate myStatus;

	public float killSpeed;
	
	enum State
	{
		roaming,
		chasing
	}

	State currState;

	// Use this for initialization
	void Start () {

		StartCoroutine(changeState (State.roaming));

		//setting agent
		this.agent = GetComponent<NavMeshAgent> ();

		myStatus = GetComponentInChildren<StatusUpdate> ();

		this.prevWaypoint = this.waypoint;

		gameObject.renderer.material.color = Color.black;
			
	}

	
	// Update is called once per frame
	void Update () 
	{

		move ();


		if(this.health <= 0)
		{

			Destroy (this.gameObject);

		}

		if(gameObject.rigidbody.velocity.magnitude >= 2.0f)
		{
			gameObject.rigidbody.velocity = gameObject.rigidbody.velocity * 0.5f;

		}


		RaycastHit hit;

		if(Physics.Raycast (transform.position, -Vector3.up, out hit, 100.0f))
		{

			if(hit.distance <= 0.5f)
			{
			
				transform.position = new Vector3(transform.position.x, 2.0f, transform.position.z);

			}

		}

	}


	protected void OnCollisionEnter(Collision other)
	{

		if(other.rigidbody == null)
		{
			return;

		}else if(other.gameObject.tag.Equals("Player"))
		{
			GameObject.Find ("GameManager").GetComponent<GameManager>().decreaseHealth("Killer");
			myStatus.updateText(true);
			
		}else if(other.gameObject.rigidbody != null && other.gameObject.rigidbody.velocity.magnitude >= killSpeed)
		{
			
			damage ();
			
		}

	}

	public void damage()
	{

		health = health - damageTaken;
		
		StartCoroutine(flashRed ());

	}

	protected override void react()
	{

		updateWaypoint (GameObject.FindGameObjectWithTag ("Player").gameObject.transform);

	}


	public override void isInterested()
	{
		StartCoroutine(changeState (State.chasing));

		this.interested = true;

		this.myStatus.updateText (true);

		react ();
		
	}


	public void notInterested()
	{
		StartCoroutine(changeState (State.roaming));
		
		this.interested = false;

		this.myStatus.updateText (false);
		
		updateWaypoint(this.prevWaypoint);

	}


	IEnumerator flashRed()
	{

		gameObject.renderer.material.color = Color.red;
		
		yield return new WaitForSeconds(0.2f);
		
		gameObject.renderer.material.color = Color.black;
		
	}


	IEnumerator changeState(State newState)
	{

		yield return new WaitForSeconds (1.0f);

		this.currState = newState;

	}

}
