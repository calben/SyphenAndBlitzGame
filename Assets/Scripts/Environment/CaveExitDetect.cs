using UnityEngine;
using System.Collections;

public class CaveExitDetect : MonoBehaviour {
	public GameObject _audioManager;
	bool triggeredTutorial;//Makes sure that the GUI window appears only once

	// Use this for initialization
	void Start () {
		triggeredTutorial = false;
		_audioManager.GetComponent<AudioManager>().Initialize();
		_audioManager.GetComponent<AudioManager>().Play("Pre_Break_1", 0.0f, false);
		_audioManager.GetComponent<AudioManager>().QueueClip("Pre_Break_2", true);
	}

	void OnTriggerEnter(Collider other) {
		if (!triggeredTutorial && other.gameObject.tag.Equals("Player")) {
			GameObject.Find ("GameManager").GetComponent<GameManager>().activateNewObjective();
			triggeredTutorial = true;
			_audioManager.GetComponent<AudioManager>().QueueClip("Post_Break", true);
		}
	}
}
