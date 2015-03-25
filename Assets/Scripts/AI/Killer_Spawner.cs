using UnityEngine;
using System.Collections;

public class Killer_Spawner : MonoBehaviour {

	public GameObject killers;
	public float spawnDelay;
	public Transform goHere;

	int numKillers;
	float numKilled;
	
	
	void Start()
	{
		numKillers = 2;
		numKilled = 0;
	
	}

	void Update()
	{

		if(numKilled == 2)
		{

			spawn ();

			numKilled = 0;

		}

	}
	
	
	void spawn()
	{
		int i = 0;
		GameObject tempObj;
		
		while(i < numKillers)
		{
			
			tempObj = Instantiate(killers, gameObject.transform.position, Quaternion.identity) as GameObject;

			tempObj.GetComponent<Killer_Mover>().updateWaypoint (goHere);

			i++;
		}
		
		
	}

	public void somethingDied()
	{

		numKilled++;

	}

}
