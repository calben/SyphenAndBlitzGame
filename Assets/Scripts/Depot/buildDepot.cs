using UnityEngine;
using System.Collections;

public class buildDepot : MonoBehaviour {
	public Object depot;
	GameObject player;

	Vector3 playerPos;
	Vector3 playerDirection;
	Vector3 spawnPos;
	Quaternion playerRotation;

	float lastDeployed;
	bool isDeploying;
	bool firstButtonPressed = false;
	bool secondButtonPressed = false;

	public bool froze;

	float spawnDistance = 1.0f;

	public GameObject button1, button2, button3;

	// Use this for initialization
	void Start () {
		//depot = Resources.Load("Prefabs/Cube");
		player = this.gameObject;
		lastDeployed = 0.0f;
		isDeploying = false;

		/*
		Component[] comps = gameObject.GetComponents(typeof(Component));
		foreach( Component c in comps){
			Debug.Log (c.ToString());
		}
		*/
	}
	
	// Update is called once per frame
	void Update () {
		lastDeployed += Time.deltaTime;

		var vert = Input.GetAxis("Xbox360ControllerDPadY");

		// get the spawn position for the cube, which shoudl be in front of the player
		playerPos = player.transform.position;
		playerDirection = player.transform.forward;
		playerRotation = player.transform.rotation;
		spawnPos = playerPos + playerDirection*spawnDistance;
		spawnPos.y += 0.5f; // bring the cube up

		//UnitySampleAssets.Characters.ThirdPerson.ThirdPersonUserControl userCtrl = gameObject.GetComponent<UnitySampleAssets.Characters.ThirdPerson.ThirdPersonUserControl>();

		// if "up" on d-pad is pressed, spawn depot
		if(vert > 0 && lastDeployed > 1.0f){
			isDeploying = true;

			// character cannot move
			//userCtrl.enabled = false;
		}

		if (isDeploying) {
			// press a sequence of buttons to deploy
			if(Input.GetKeyDown ("joystick button 0")){
				firstButtonPressed = true;
			}
			if(Input.GetKeyDown ("joystick button 2") && firstButtonPressed){
				secondButtonPressed = true;
			}
			// after pressing last button, return to default state
			if(Input.GetKeyDown ("joystick button 3") && firstButtonPressed && secondButtonPressed){
				Instantiate(depot, spawnPos, playerRotation);
				lastDeployed = 0.0f;
				isDeploying = false;

				firstButtonPressed = false;
				secondButtonPressed = false;
				froze = false;
				//userCtrl.enabled = true;
			}

			// render button sprite
			//button1.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Sprites/Button_A");

			// option to cancel if "B"
			if(Input.GetKeyDown ("joystick button 1")){
				isDeploying = false;
				lastDeployed = 0.0f;
				firstButtonPressed = false;
				secondButtonPressed = false;
				froze = false;
				//userCtrl.enabled = true;
			}
		}
	}

	void OnGUI(){
		if(isDeploying)
			GUI.Label(new Rect(0,0,Screen.width,Screen.height),"To deploy: press A -> X -> Y\nTo cancel: press B");
	}
}
