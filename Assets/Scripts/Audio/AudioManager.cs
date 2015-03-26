using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour {
	public AudioClip[] _clips;
	public float _volume = 1.0f;

	private AudioSource _audioSource;
	private bool _isStoppingSFX = false;
	private bool _isPlayingSFX = false;
	private float _fadeTime = 0.0f;
	private bool _switchOnFinish = false;
	private string _nextClip;
	private bool _nextClipIsLooping;

	// Use this for initialization
	void Start () {
		_audioSource = this.GetComponent<AudioSource>();
	}

	public void Initialize(){
		_audioSource = this.GetComponent<AudioSource>();
	}

	// Update is called once per frame
	void Update () {
		if(_isStoppingSFX){
			_audioSource.volume -= Time.deltaTime * _fadeTime;
			if(_audioSource.volume <= 0.0f){
				_audioSource.volume = 0.0f;
				_audioSource.Stop();
				_isStoppingSFX = false;
				_audioSource.volume = _volume;
			}
		}
		if(_switchOnFinish){
			_audioSource.loop = false;
			if(!_audioSource.isPlaying){
				Play(_nextClip, 0.0f, _nextClipIsLooping);
			}
		}
		/*
		if(_isPlayingSFX){
			_audioSource.volume += Time.deltaTime * _fadeTime;
			if(_audioSource.volume >= _sfxVolume){
				_audioSource.volume = _sfxVolume;
				_isPlayingSFX = false;
			}
		}
		*/
	}

	public void Play(string clipName, float fadeTime, bool _isLooping){
		for(int i = 0; i < _clips.Length; ++i){
			if(_clips[i].name.Equals(clipName)){
				_audioSource.volume = _volume;
				_isStoppingSFX = false;
				_audioSource.loop = _isLooping;
				_audioSource.clip = _clips[i];
				_audioSource.Play();
				Debug.Log("Playing: " + clipName);
			}
		}
	}

	public void Stop(string clipName, float fadeTime){
		for(int i = 0; i < _clips.Length; ++i){
			if(_clips[i].name.Equals(clipName)){
				_isStoppingSFX = true;
				_fadeTime = fadeTime;
			}
		}
	}

	public void QueueClip(string clipName, bool isLooping){
		_switchOnFinish = true;
		_nextClip = clipName;
		_nextClipIsLooping = isLooping;
	}

	public void SetVolume(float volume){
		_audioSource.volume = volume;
	}
}
