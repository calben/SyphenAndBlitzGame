using UnityEngine;
using System.Collections;

public class CaveExitDetect : MonoBehaviour {

	bool triggeredTutorial;//Makes sure that the GUI window appears only once

	// Use this for initialization
	void Start () {
		triggeredTutorial = false;
	}
	
	void OnCollisionEnter(Collision other) {
		Debug.Log ("DETECTED YOUUUU");
		if (!triggeredTutorial && other.gameObject.tag.Equals("Player")) {
			Debug.Log ("EXITED CAVE");
			GameObject.Find ("GameManager").GetComponent<GameManager>().activateNewObjective();
			triggeredTutorial = true;
		}
	}
}
