using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DepotBar : MonoBehaviour {

	public int depotID;
	public GameObject gameManager;
	private GameManager gameManagerStats;
	private Image bar;

	void Start() {
		//Set up depot bar
		bar = GetComponent<Image> ();
		RectTransform barRect = GetComponent<RectTransform> ();

		//Set up manager
		gameManagerStats = gameManager.GetComponent<GameManager> ();
	}
	// Update is called once per frame
	void Update () {
		float currentSize = (float)(gameManagerStats.depotCurrentStock[depotID]) / gameManagerStats.depotCapacity[depotID];
		if (currentSize!=bar.fillAmount) {
			bar.fillAmount = Mathf.MoveTowards (bar.fillAmount, currentSize, Time.deltaTime * 0.8f);
		}
	}
}
