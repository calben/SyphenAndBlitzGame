using UnityEngine;
using System.Collections;

public class depotAbsorb : MonoBehaviour {

	public int depotNumber;
	public string supplyItemTag;

//	public int capacity = 5000;
//	public int resourceVal = 100;
//	public int currentStock = 0;
	public float animGrowRate = 0.1f;
	//Link to game manager to update stats
	public GameObject gameManager;

	private GameManager gameStats;
	private bool depotFull;
	private int capacity;
	private int resourceVal;
	private int currentStock;
	private Animator myAnim;

	void Start(){
		myAnim = this.GetComponent<Animator>();
		myAnim.SetBool("isStopped", true);
		myAnim.SetBool("isPlaying", false);
		myAnim.speed = 0.0f;
		gameStats = gameManager.GetComponent<GameManager> ();
		depotFull = false;
		capacity = gameStats.depotCapacity [depotNumber];
		resourceVal = gameStats.depotResourceValue [depotNumber];
		currentStock = gameStats.depotCurrentStock [depotNumber];
	}

	public int getCurrentSize() {
		return currentStock;
	}
	public int getSize() {
		return capacity;
	}

	void OnTriggerEnter(Collider other){
		if(other.tag == supplyItemTag && !depotFull){
			if ((gameStats.depotCurrentStock [depotNumber]) <= capacity) {
				Debug.Log("Growing");
				gameStats.increaseResourceCount(depotNumber);
				//Destroy resource
				Destroy( other.gameObject );
				myAnim.SetBool("isPlaying", true);
				myAnim.SetBool("isStopped", false);
				myAnim.Play("depotGrow", 0, myAnim.GetCurrentAnimatorStateInfo(0).length * gameStats.depotCurrentStock [depotNumber]/gameStats.depotCapacity [depotNumber] * animGrowRate);
			} else if (!depotFull) {
				Debug.Log ("Resource Depot FuLL");
				depotFull = true;
			}
		}

	}

}
