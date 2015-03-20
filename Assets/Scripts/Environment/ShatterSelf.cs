using UnityEngine;
using System.Collections;

public class ShatterSelf : MonoBehaviour {
	public float _velocityThreshold;
	
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	
	void OnCollisionEnter(Collision col) {
		if (col.rigidbody) {
			float velocity = col.rigidbody.velocity.sqrMagnitude;
			if (velocity > _velocityThreshold)
			{
				PhysicsStatus ps = this.GetComponent<PhysicsStatus>();
				if (ps && (ps.pullable || ps.pushable))
				{
					this.gameObject.GetComponent<Rigidbody>().isKinematic = false;
				}
				else{
					this.gameObject.SetActive(false);
				}
			}
		}
	}
	
	//	public void switchToFractured(){
	//		this.gameObject.SetActive(false);
	//		_fractureSet.gameObject.SetActive(true);
	//	}
}
