using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ArcReactorDemo3GenerateMirrors : MonoBehaviour {

	public GameObject mirrorPrefab;
	public GameObject spherePrefab;
	public int mirrorCount;
	public float mirrorSphereRatio;
	public Vector3 center;
	public Vector3 size;

	private List<Transform> mirrors = new List<Transform>();

	const float minDistance = 5;

	public void GenerateMirrors(int count)
	{
		GameObject obj;
		Vector3 position;
		float dist = float.MaxValue;
		for (int i = 0; i < count; i++)
		{
			if (UnityEngine.Random.value > mirrorSphereRatio)
				obj = (GameObject)(GameObject.Instantiate(mirrorPrefab));
			else
				obj = (GameObject)(GameObject.Instantiate(spherePrefab));
			obj.transform.parent = transform;
			dist = float.MaxValue;
			int iteration = 0;
			do
			{
				iteration++;
				dist = float.MaxValue;
				position = new Vector3(UnityEngine.Random.Range(center.x-size.x,center.x+size.x),
				                       UnityEngine.Random.Range(center.y-size.y,center.y+size.y),
				                       UnityEngine.Random.Range(center.z-size.z,center.z+size.z));
				foreach (Transform tr in mirrors)
				{
					if (Vector3.Distance(tr.position,position) < dist)
						dist = Vector3.Distance(tr.position,position);
				}
				//Debug.Log(position.ToString() + ":" + dist);
			} while (dist < minDistance && iteration < 100);
			obj.transform.position = position;
			mirrors.Add (obj.transform);
		}
	}



	// Use this for initialization
	void Start () 
	{
		GenerateMirrors(mirrorCount);
	}

	void Update ()
	{
		if (Input.GetKeyUp(KeyCode.Q))
		{
			foreach(Transform tr in mirrors)
				Object.Destroy(tr.gameObject);
			
			mirrors = new List<Transform>();
			
			GenerateMirrors(mirrorCount);
		}
	}

}
