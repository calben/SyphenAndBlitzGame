using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class TutorialManager : MonoBehaviour {

	public GameObject eventSystemObject;
	public GameObject tutorialStartButton;
	public List<GameObject> syphenTutorials;
	public List<GameObject> blitzTutorials;
	private EventSystem eventSystem;
	private List<GameObject> tutorials;
	public AudioManager _audioManager;

	// Use this for initialization
	void Start () {
		foreach (GameObject tut in syphenTutorials) {
			tut.SetActive(false);
		}
		foreach (GameObject tut in blitzTutorials) {
			tut.SetActive(false);
		}
		eventSystem = eventSystemObject.GetComponent<EventSystem> ();

//		StartTutorial ("Syphen");
	}
	public void StartTutorial(string playerName) {
		GameObject[] players = GameObject.FindGameObjectsWithTag ("Player");
		foreach (GameObject p in players) {
			p.GetComponent<RigidbodyNetworkedPlayerController>().enabled = false;
		}

		//Start first tutorial panel
		if (playerName.Contains("Syphen")) {
			tutorials = syphenTutorials;
		} else {
			tutorials = blitzTutorials;
		}
		tutorials[0].SetActive (true);
		eventSystem.SetSelectedGameObject(tutorialStartButton);
		_audioManager.Play ("tutorial_click", 1.0f, false);
	}
	public void NextTutorial() {
		//Disable current menu and remove it from list
		tutorials [0].SetActive (false);
		tutorials.RemoveAt (0);
		//Check if there exists next tutorial
		if (tutorials.Count > 0) {
			tutorials [0].SetActive (true);
			//Activate button on that panel so that the xbox controller can access it
			try {
				GameObject button = (tutorials [0].transform.FindChild ("Panel")).FindChild ("YesButton").gameObject;
				eventSystem.SetSelectedGameObject (button);
			} catch (System.NullReferenceException e) {

			}
		} else {
			ExitTutorial();
		}
	}
	private void ExitTutorial() {
		_audioManager.Play ("tutorial_click", 1.0f, false);
		GameObject[] players = GameObject.FindGameObjectsWithTag ("Player");
		foreach (GameObject p in players) {
			p.GetComponent<RigidbodyNetworkedPlayerController>().enabled = true;
		}
	}
}
