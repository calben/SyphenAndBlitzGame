using UnityEngine;
using System.Collections;

public class SupplyDepotBehaviour : MonoBehaviour {
	
	public int maxSize=10;
	public int currentSize=0;
	public GameObject gameManager;
	private GameManager gameStats;
	private bool depotFull;
	
	// Use this for initialization
	void Start () {
		gameStats = gameManager.GetComponent<GameManager> ();
		depotFull = false;
	}
	
	
	
	public SupplyDepotBehaviour getInstance() {
		
		return this;

	}
	
	public int getSize(){
		
		return this.maxSize;
		
	}
	public int getCurrentSize() {
		return this.currentSize;
	}
	
	public void updateSize(int item){
		
		if ((this.currentSize + item) <= maxSize) {
			this.currentSize += item;
		}else if (!depotFull && this.currentSize>=maxSize){
			Debug.Log ("Resource Depot FuLL");
			gameStats.depotFull();
			depotFull = true;
		}
		
		
	}
	
	
	// Update is called once per frame
	void Update () {
		
		//		Debug.Log (currentSize);
		
	}
}
