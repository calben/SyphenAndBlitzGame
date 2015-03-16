using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class TutorialVacuum : MonoBehaviour {

	public List<GameObject> tutorials;
	int tutorialIndex;

	void Start () {
		tutorialIndex = 0;
		foreach (GameObject tutorial in tutorials) {
			tutorial.SetActive(false);
		}
	}

	public void NextTutorial() {
		//Disable current tutorial panel
		tutorials [tutorialIndex].SetActive (false);
		tutorialIndex++;
		if (tutorialIndex < tutorials.Count) {
			//Enable next tutorial panel
			tutorials [tutorialIndex].SetActive (true);
			//Activate button on that panel so that the xbox controller can access it
			try {
				GameObject button = tutorials [tutorialIndex].transform.FindChild ("Button").gameObject;
				GameObject.Find("EventSystem").GetComponent<EventSystem>().SetSelectedGameObject (button);
			} catch (System.NullReferenceException e) {
			}
		}
	}
}
