using UnityEngine;
using System.Collections;

public class ArcReactorDemo_FollowCamera : MonoBehaviour {
	
	public Transform followTransform;
	
	new private Camera camera;

	// Use this for initialization
	void Start () 
	{
		camera = GetComponent<Camera>();
		camera.rect = new Rect(0.01f,0.01f,0.4f,0.4f);	
	}

	void LateUpdate () 
	{
		transform.LookAt(followTransform.position);
	}
}
