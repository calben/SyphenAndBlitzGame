using UnityEngine;
using System.Collections;
using GamepadInput;

public class ClosePanel : MonoBehaviour {

	public string inputName;

	void Start () {
	}
	
	void Update () {
		if (inputName == "left") {
			if (GamePad.GetTrigger (GamePad.Trigger.LeftTrigger, GamePad.Index.One) > 0.20f) {
				gameObject.SetActive (false);
			}
		} else if (inputName == "right") {
			if (GamePad.GetTrigger (GamePad.Trigger.RightTrigger, GamePad.Index.One) > 0.20f) {
				gameObject.SetActive (false);
			}
		} else if (inputName == "a") {
			if (GamePad.GetButtonDown(GamePad.Button.A, GamePad.Index.One)) {
				gameObject.SetActive (false);
			}
		}
	}
}
