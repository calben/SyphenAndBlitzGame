using UnityEngine;
using System.Collections;

public class StatusUpdate : MonoBehaviour {

	TextMesh status;
	Camera myCamera;

	public string normalText;
	public string chasingText;
	public string damageText;

	public float depthThreshold = 30;


	// Use this for initialization
	void Start ()
	{
	
		myCamera = Camera.main;

		status = gameObject.GetComponent<TextMesh> ();

		updateText (false);

	}

	void Update()
	{

		if (myCamera.WorldToViewportPoint (transform.position).z < depthThreshold) {
			status.GetComponent<Renderer>().enabled = true;
		} else {
			status.GetComponent<Renderer>().enabled = false;
		}

		gameObject.transform.LookAt (myCamera.transform.position);
		gameObject.transform.RotateAround (transform.position, transform.up, 180f);
		
	}

	public void updateText(bool playerClose)
	{

		if(playerClose)
		{
			status.color = Color.red;
			status.text = chasingText;
		
		}
		else
		{

			status.color = Color.yellow;
			status.text = normalText;

		}

	}

	public void hitText()
	{

		status.color = Color.red;
		status.text = damageText;

	}

}
