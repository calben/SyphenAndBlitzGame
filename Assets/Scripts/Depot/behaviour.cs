using UnityEngine;
using System.Collections;

public class DepotBehaviour : MonoBehaviour {

	public string supplyDepotTag;
	public string supplyItemTag;
	private SupplyDepotBehaviour sdB;
	//private Canvas canvas;

	void Start(){

		sdB = GameObject.FindWithTag (supplyDepotTag).GetComponent<SupplyDepotBehaviour>().getInstance ();
		//canvas = sdB.GetComponent<Canvas> ();
		//tempObj = sdB.getInstance ();
		
	}


	void OnTriggerEnter(Collider other){

		/*
		 * replace 10 with other.gameObject.value
		 * 
		 *
		 */
		if(other.tag == supplyItemTag){
		
			sdB.updateSize (1);
			this.transform.parent.transform.localScale += (Vector3.up * 0.1f);
			Destroy( other.gameObject );
		
		}

	}


}
