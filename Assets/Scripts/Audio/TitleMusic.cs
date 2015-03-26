using UnityEngine;
using System.Collections;

public class TitleMusic : MonoBehaviour {
	public AudioManager _audioManager;

	// Use this for initialization
	void Start () {
		_audioManager = this.gameObject.GetComponent<AudioManager>();
		DontDestroyOnLoad(gameObject);
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnLevelWasLoaded(int level) {
		if (level != 0)
		{
			_audioManager.Stop("MainMenu1", 0.0f);
		}
	}
}
