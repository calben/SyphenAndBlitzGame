using UnityEngine;
using System.Collections;

public class DepotCircleLocation : MonoBehaviour {

	public int depotNum = 1;
	private RectTransform depot;

	// Use this for initialization
	void Start () {
		depot = gameObject.GetComponent<RectTransform> ();
		depot.sizeDelta = new Vector2 (Screen.width*0.1f, Screen.width*0.1f);
		if (depotNum==1) {
			depot.anchoredPosition = new Vector2 (-Screen.width * 0.01f, -Screen.height * 0.01f);
		} else {
			depot.anchoredPosition = new Vector2 (-1.0f*(depot.sizeDelta.x+Screen.width * 0.015f), -Screen.height * 0.01f);
		}
	}
}
