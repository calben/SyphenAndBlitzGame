using UnityEngine;
using System.Collections;

public class PlayAudio : MonoBehaviour {
	public GameObject _audioManager;
	public string _songName;
	
	// Use this for initialization
	void Start () {
		_audioManager.GetComponent<AudioManager>().Initialize();
		_audioManager.GetComponent<AudioManager>().Play(_songName, 0.0f, true);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
