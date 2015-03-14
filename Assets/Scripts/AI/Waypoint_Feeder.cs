using UnityEngine;
using System.Collections;

public class Waypoint_Feeder : MonoBehaviour {
	
	public Transform nextWaypoint;


	void OnTriggerEnter(Collider other)
	{

		if(other.tag.Equals ("Feeder") )
		{
			if(!other.gameObject.GetComponent<Feeder_Mover>().interest() )
			{
				other.gameObject.GetComponent<Feeder_Mover>().updateWaypoint(this.nextWaypoint);
			}
		}

	}


}
