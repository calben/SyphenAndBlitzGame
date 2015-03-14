using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class DepotStats : MonoBehaviour {

	public int depotID;
	public GameObject gameManager;
	private GameManager gameManagerStats;
	private Text stats;

	// Use this for initialization
	void Start () {
		gameManagerStats = gameManager.GetComponent<GameManager> ();
		stats = this.GetComponent<Text> ();
	}
	
	// Update is called once per frame
	void Update () {
		stats.text = gameManagerStats.depotCurrentStock[depotID].ToString() + "/" + gameManagerStats.depotCapacity[depotID].ToString();
	}
}
