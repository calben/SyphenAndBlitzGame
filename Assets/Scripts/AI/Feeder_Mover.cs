using UnityEngine;
using System.Collections;

public class Feeder_Mover : AI_Mover {

	public float killSpeed;
	public float timeUntilBored;
	public float smellingRadius;
	public float pullRadius;
	public float pullForce;
	public GameObject EnviroTile;

	Transform tempTarget;
	bool isFeeding;
	int resourcesEaten;
	float tempSpeed;
	StatusUpdate myStatus;



	// Use this for initialization
	void Start ()
	{
		//setting agent
		this.agent = GetComponent<NavMeshAgent> ();

		this.myStatus = GetComponentInChildren<StatusUpdate> ();

		gameObject.renderer.material.color = Color.magenta;

		this.prevWaypoint = this.waypoint;
		
	}


	// Update is called once per frame
	void Update () 
	{
		
		move ();

		if(health <= 0)
		{

			kill ();

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
				
				transform.position = new Vector3(transform.position.x, 1.0f, transform.position.z);
				
			}
			
		}
		
	}

	public void setTempTarget(Transform nextWaypoint)
	{

		this.tempTarget = nextWaypoint;

	}



	/*
	IEnumerator falcuhnPuuull()
	{
		tempSpeed = agent.speed;
		agent.speed = 0.0f;

		foreach(Collider collider in Physics.OverlapSphere(gameObject.transform.position, pullRadius))
		{
			if(collider.gameObject.tag.Equals("EnviroTile"))
			{
				Debug.Log ("NOMNOM PULL");
				
				Vector3 directedForce = transform.position - collider.transform.position;
				
				collider.rigidbody.AddForce (directedForce.normalized * pullForce * Time.deltaTime);
			}

		}

		yield return new WaitForSeconds (4.0f);
		notInterested ();

			void FixedUpdate()
	{
		if(interested){
		
			tempSpeed = agent.speed;
			agent.speed = 0.0f;
			
			foreach(Collider collider in Physics.OverlapSphere(gameObject.transform.position, pullRadius))
			{
				if(collider.gameObject.tag.Equals("EnviroTile"))
				{

					Vector3 directedForce = transform.position - collider.transform.position;
					
					collider.rigidbody.AddForce (directedForce.normalized * pullForce * Time.deltaTime);
				}
			
			}
		
			notInterested ();
		}

	}

	}*/

	public void kill()
	{

		int i = 0;
		GameObject tempGameObj;
		while(i < resourcesEaten)
		{

			tempGameObj = Instantiate(EnviroTile, transform.position, Quaternion.identity) as GameObject;

		}

		Destroy (this.gameObject);

	}

	protected void OnCollisionEnter(Collision other)
	{
		
		if(other.gameObject.tag.Equals("EnviroTile"))
		{

			Destroy (other.gameObject);
			resourcesEaten++;
			//StartCoroutine(flashGreen());
			//StartCoroutine(falconPull());
			return;

		}

		if(other.gameObject.tag.Equals("Player"))
		{
			GameObject.Find ("GameManager").GetComponent<GameManager>().decreaseHealth("Feeder");
			damage();
			return;

		}

		if(other.gameObject.rigidbody != null && other.gameObject.rigidbody.velocity.magnitude >= killSpeed)
		{

			damage ();

		}
	}

	public void damage()
	{

		health = health - damageTaken;
		StartCoroutine( flashRed ());

	}

	public override void isInterested()
	{
		
		this.interested = true;

		myStatus.updateText (true);
		
		react ();
		
	}
	
	
	protected override void react()
	{
		
		this.waypoint = this.tempTarget;
		
	}


	public void notInterested()
	{

		this.agent.speed = this.tempSpeed;
		Debug.Log (this.agent.speed);

		this.interested = false;

		myStatus.updateText (false);

		this.waypoint = this.prevWaypoint;

	}

	//change this to flashColour(Color tempColour) so you do all the colours!
	IEnumerator flashRed()
	{
		
		gameObject.renderer.material.color = Color.red;
		
		yield return new WaitForSeconds(0.2f);
		
		gameObject.renderer.material.color = Color.magenta;
		
	}


	IEnumerator flashGreen()
	{
		
		gameObject.renderer.material.color = Color.green;
		
		yield return new WaitForSeconds(0.2f);
		
		gameObject.renderer.material.color = Color.magenta;
		
	}

	IEnumerator falconPull()
	{
		Debug.Log ("wait for it");
		yield return new WaitForSeconds (1.3f);

		this.tempSpeed = this.agent.speed;
		this.agent.speed = 0f;

		Debug.Log ("a few more seconds");
		yield return new WaitForSeconds(1.0f);

		Debug.Log ("ok go");
		notInterested ();

	}

}
