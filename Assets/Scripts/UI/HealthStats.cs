using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class HealthStats : MonoBehaviour {

	public GameObject gameManager;
	private GameManager gameManagerStats;
	private int playerID;
	private Text stats;

	// Use this for initialization
	void Start () {
		gameManagerStats = gameManager.GetComponent<GameManager> ();
		stats = this.GetComponent<Text> ();
	}
	public void StartStats(int playerID) {
		this.playerID = playerID;
	}
	// Update is called once per frame
	void Update () {
		if (playerID>=0) {
			stats.text = gameManagerStats.playerCurrentHealth.ToString() + "/" + gameManagerStats.playerTotalHealth.ToString();
		}
	}
}
