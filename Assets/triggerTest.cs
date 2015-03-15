using UnityEngine;
using System.Collections;

public class triggerTest : MonoBehaviour {
	public GameObject _audioManager;

	// Use this for initialization
	void Start () {
		_audioManager.GetComponent<AudioManager>().Initialize();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void OnTriggerEnter(Collider col){
		_audioManager.GetComponent<AudioManager>().PlaySFX("Post_Break", 0.0f, false);
		Destroy(this.gameObject);
	}
}
