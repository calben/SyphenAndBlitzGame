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
		GetComponent<AudioSource>().clip = _audioclips[0];
		GetComponent<AudioSource>().Play();
	}

	public void playLoop(){
		GetComponent<AudioSource>().clip = _audioclips[1];
		GetComponent<AudioSource>().Play();
	}

	public void playPowerDown(){
		GetComponent<AudioSource>().clip = _audioclips[2];
		GetComponent<AudioSource>().Play();
	}
}
