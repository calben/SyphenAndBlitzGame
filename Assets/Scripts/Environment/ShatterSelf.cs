using UnityEngine;
using System.Collections;

public class ShatterSelf : MonoBehaviour {
	public float _velocityThreshold;

	public void switchToFractured(){
		this.gameObject.GetComponent<Rigidbody>().isKinematic = false;
		//_fractureSet.gameObject.SetActive(true);
	}
	/*
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
	*/

	//	public void switchToFractured(){
	//		this.gameObject.SetActive(false);
	//		_fractureSet.gameObject.SetActive(true);
	//	}
}
