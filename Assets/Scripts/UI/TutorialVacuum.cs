using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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
		}
	}
}
