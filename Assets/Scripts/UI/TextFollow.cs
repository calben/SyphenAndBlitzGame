using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class TextFollow : MonoBehaviour {

	public GameObject followObject;
	public Vector3 offset = Vector3.up;
	public float viewDepth = 50f;//The max depth of object in scene before text is disabled
	private Camera camera;
	private Text text;

	// Use this for initialization
	void Start () {
		camera = Camera.main;
		text = gameObject.GetComponent<Text>();
		text.enabled = true;
	}
	
	// Update is called once per frame
	void Update () {
		if (followObject.name.Contains ("Depot")) {
			offset = new Vector3 (0f, followObject.transform.FindChild("pCube1").transform.position.y/2f, 0f);
		}
		Vector3 viewport = camera.WorldToViewportPoint (followObject.transform.position + offset);
		//If object is less than a certain selfDistance, show its text
		if (viewport.z < viewDepth && viewport.x >=0 && viewport.y>=0 && viewport.z>=0 && followObject.activeSelf == true) {
			transform.position = new Vector3 (viewport.x * Screen.width, viewport.y * Screen.height, 0f);
			text.enabled = true;
		} else {
			text.enabled = false;
		}
	}
}
