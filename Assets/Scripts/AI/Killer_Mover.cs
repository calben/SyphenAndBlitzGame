using UnityEngine;
using System.Collections;

public class Killer_Mover : AI_Mover {

	protected StatusUpdate myStatus;

	public float killSpeed;

	// Use this for initialization
	void Start () {

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

	}


	protected void OnCollisionEnter(Collision other)
	{

		if(other.rigidbody == null)
		{

			return;

		}

		if (other.rigidbody.velocity.magnitude >= killSpeed)
		{
			if(other.gameObject.tag.Equals("Player"))
			{

				myStatus.updateText(true);

			}

			health = health - damageTaken;

			StartCoroutine(flashRed ());

		}
		
	}


	protected override void react()
	{

		updateWaypoint (GameObject.FindGameObjectWithTag ("Player").gameObject.transform);

	}


	public override void isInterested()
	{
		
		this.interested = true;

		this.myStatus.updateText (true);
		
		react ();
		
	}


	public void notInterested()
	{
		
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

}
