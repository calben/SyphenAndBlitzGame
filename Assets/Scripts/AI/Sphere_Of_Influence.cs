using UnityEngine;
using System.Collections;

public class Sphere_Of_Influence : MonoBehaviour {

	bool isRunning;

	public float timeUntilLost;

	void Start()
	{

		this.isRunning = false;

	}


	void OnTriggerEnter(Collider other)
	{

		if(other.tag.Equals ("Player") )
		{

			if(GetComponentInParent<Killer_Mover>().interest ())
			{

				if (this.isRunning) 
				{
					
					StopCoroutine ("lost_player_timer");
					
					this.isRunning = false;
				
				}

			}

			GetComponentInParent<Killer_Mover>().isInterested();
			
		}
		
	}


	void OnTriggerExit(Collider other)
	{

		if(other.tag.Equals("Player"))
		{

			StartCoroutine(lost_player_timer());

		}

	}


	IEnumerator lost_player_timer() 
	{

		this.isRunning = true;

		yield return new WaitForSeconds (this.timeUntilLost);

		GetComponentInParent<Killer_Mover> ().notInterested ();

		//yield return null;

	}

}
