using UnityEngine;
using System.Collections;

public class Waypoint : MonoBehaviour {
	
	public Transform nextWaypoint;


	void OnTriggerEnter(Collider other)
	{

		if(other.tag.Equals ("Killer") )
		{

			if( !other.gameObject.GetComponent<Killer_Mover>().interest() )
			{

				other.gameObject.GetComponent<Killer_Mover>().updateWaypoint(this.nextWaypoint);
			
			}

		}

	}


}
