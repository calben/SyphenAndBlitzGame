using UnityEngine;
using System.Collections;

public class Killer_Spawner : MonoBehaviour {

	public GameObject killers;
	public int numKillers;
	public float spawnDelay;
	
	
	void Start()
	{
		
		StartCoroutine (spawnStarter (spawnDelay));
		
	}
	
	
	void spawn()
	{
		int i = 0;
		GameObject tempObj;
		
		while(i < numKillers)
		{
			
			tempObj = Instantiate(killers, gameObject.transform.position, Quaternion.identity) as GameObject;
			
			i++;
		}
		
		
	}
	
	
	public IEnumerator spawnStarter(float timeToWait)
	{
		
		yield return new WaitForSeconds (timeToWait);
		
		spawn ();
		
		
	}

}
