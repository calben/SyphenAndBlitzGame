using UnityEngine;
using System.Collections;

public class Feeder_Spawner : MonoBehaviour {

	public GameObject feeders;
	public int numFeeders;
	public float spawnDelay;


	void Start()
	{

		StartCoroutine (spawnStarter (spawnDelay));

	}


	void spawn()
	{

		int i = 0;
		GameObject tempObj;

		while(i < numFeeders)
		{

			tempObj = Instantiate(feeders, gameObject.transform.position, Quaternion.identity) as GameObject;

			i++;
		}


	}


	IEnumerator spawnStarter(float timeToWait)
	{

		yield return new WaitForSeconds (timeToWait);

		spawn ();


	}

}
