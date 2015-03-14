using UnityEngine;
using System.Collections;

public class Sphere_Of_Influence_Feeder : MonoBehaviour {

	public float eatingRadius;

	void OnTriggerEnter(Collider other)
	{

		if(other.tag.Equals ("EnviroTile"))
		{

			GetComponentInParent<Feeder_Mover>().setTempTarget(other.gameObject.transform);
			GetComponentInParent<Feeder_Mover>().isInterested();

		}

		if(other.tag.Equals ("Player"))
		{
			//StopCoroutine("noMoreFood");

			GetComponentInParent<Feeder_Mover>().notInterested();

		}
		
	}


}
