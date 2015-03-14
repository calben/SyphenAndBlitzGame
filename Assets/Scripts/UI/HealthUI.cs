using UnityEngine;
using System.Collections;

public class HealthUI : MonoBehaviour {

	// Use this for initialization
	void Start () {
		RectTransform statsRect = this.GetComponent<RectTransform> ();
		statsRect.anchoredPosition = new Vector2 (Screen.width*0.08f, -Screen.height*0.095f);
	}

}
