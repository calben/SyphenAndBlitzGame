using UnityEngine;
using System.Collections;

public class triggerTest : MonoBehaviour {
	public GameObject _audioManager;

	// Use this for initialization
	void Start () {
		_audioManager.GetComponent<AudioManager>().Initialize();
		_audioManager.GetComponent<AudioManager>().Play("Pre_Break_1", 0.0f, false);
		_audioManager.GetComponent<AudioManager>().QueueClip("Pre_Break_2", true);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void OnTriggerEnter(Collider col){
		if(col.gameObject.tag == "Player"){
			_audioManager.GetComponent<AudioManager>().QueueClip("Post_Break", true);
			//_audioManager.GetComponent<AudioManager>().Play("Post_Break", 0.0f, false);
			Destroy(this.gameObject);
		}
	}
}
