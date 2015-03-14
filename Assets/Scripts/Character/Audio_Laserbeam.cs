using UnityEngine;
using System.Collections;

public class Audio_Laserbeam : MonoBehaviour {
	public AudioClip[] _audioclips;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void playPowerUp(){
		audio.clip = _audioclips[0];
		audio.Play();
	}

	public void playLoop(){
		audio.clip = _audioclips[1];
		audio.Play();
	}

	public void playPowerDown(){
		audio.clip = _audioclips[2];
		audio.Play();
	}
}
