using UnityEngine;
using System.Collections;
using GamepadInput;

public class TutorialAbility : MonoBehaviour {

	public string inputName;
	public GameObject gameManager;
	private TutorialManager tutManager;

	void Start () {
		tutManager = gameManager.GetComponent<TutorialManager> ();
	}
	
	void Update () {
		if (inputName == "left") {
			if (GamePad.GetTrigger (GamePad.Trigger.LeftTrigger, GamePad.Index.One) > 0.20f) {
				tutManager.NextTutorial ();
			}
		} else if (inputName == "right") {
			if (GamePad.GetTrigger (GamePad.Trigger.RightTrigger, GamePad.Index.One) > 0.20f) {
				tutManager.NextTutorial ();
			}
		}
	}
}