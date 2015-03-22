using UnityEngine;
using System.Collections;

public class ShatterSelf : MonoBehaviour {
//	public float _velocityThreshold;
	public bool _immovable;

	public void switchToFractured(){
		PhysicsStatus ps = this.GetComponent<PhysicsStatus>();
		if (_immovable) {
			this.gameObject.SetActive (false);
		}
		else if (ps && ps.pushable)
		{
			this.gameObject.GetComponent<Rigidbody>().isKinematic = false;
			this.GetComponent<PhysicsStatus>.pullable = true;
		}
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
