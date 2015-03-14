using UnityEngine;
using System.Collections;

public class CheckpointUI : MonoBehaviour {

	public float duration=5;
	public GameObject checkpointAnimatedObject;

	// Use this for initialization
	void Start () {
		checkpointAnimatedObject.SetActive(false);
	}
	public void TurnOnCheckpoint() {
		checkpointAnimatedObject.SetActive(true);
		Invoke ("TurnOffCheckpoint", duration);
	}
	void TurnOffCheckpoint() {
		checkpointAnimatedObject.SetActive(false);
	}
}
