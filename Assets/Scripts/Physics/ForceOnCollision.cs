using UnityEngine;
using System.Collections;

public class ForceOnCollision: MonoBehaviour {
	public float _magnitude = 100;
	public float _radius = 100;
	public int _duration = 100;

	public enum ForceType {Push, Pull, Lift};
	public ForceType _forceType = ForceType.Push;
	
	private int _maxMagnitude = 1000000;

	public float _delayTime = 5.0f;

	public bool debug = true;


	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void FixedUpdate () {
		if( _duration > 0 ){
			_duration--;
		}
		else{
			Destroy(this.gameObject);
		}
	}

	/// <summary>
	/// Raises the draw gizmos selected event.
	/// </summary>
	void OnDrawGizmosSelected() {
		//Debugging
		if(debug){
			Gizmos.color = Color.yellow;
			Gizmos.DrawSphere(transform.position, _radius);
		}
	}

	void OnTriggerStay(Collider other){
		if(_delayTime <= 0){
			DealDamage(other); // call to method that hurts ai
			PhysicsStatus ps = (PhysicsStatus) other.GetComponent(typeof(PhysicsStatus));
			if( ps ){
				switch( _forceType ){
				case ForceType.Push:
					if( ps.pushable ){
						Shatterable shatterable = other.gameObject.GetComponent<Shatterable>();
						if(shatterable){
							shatterable.switchToFractured();
							other.GetComponent<PhysicsStatus>().pullable = true;
						}
						ShatterSelf SS = other.gameObject.GetComponent<ShatterSelf>();
						if(SS){
							SS.switchToFractured();
							other.GetComponent<PhysicsStatus>().pullable = true;
						}
						if(other.attachedRigidbody){
							Vector3 direction = Vector3.Normalize( other.transform.position - this.transform.position );
              				other.rigidbody.isKinematic = false;
							other.rigidbody.AddForce( direction * Mathf.Clamp(_magnitude/Vector3.Magnitude(  other.transform.position - this.transform.position ), 0, _maxMagnitude) , ForceMode.Impulse);
							other.GetComponent<PhysicsStatus>().pullable = true;
						}
					}				
					break;
				case ForceType.Pull:
					if( ps.pullable ){
						if(other.attachedRigidbody){
							Vector3 direction = Vector3.Normalize( this.transform.position - other.transform.position );
							other.rigidbody.AddForce( direction * Mathf.Clamp(_magnitude/Vector3.Magnitude(this.transform.position - other.transform.position), 0, _maxMagnitude) , ForceMode.Impulse);
						}
					}		
					break;
				case ForceType.Lift:
					if( ps.liftable ){
						if(other.attachedRigidbody){
							other.rigidbody.AddForce( Vector3.up * Mathf.Clamp(_magnitude/Vector3.Magnitude(this.transform.position - other.transform.position), 0, _maxMagnitude) , ForceMode.Impulse);
						}
					}
					break;
				}
			}
		}
		else{
			_delayTime -= Time.deltaTime;
		}
	}

	void DealDamage(Collider other){
		Killer_Mover km = other.GetComponent<Killer_Mover>(); // grab scripts
		Feeder_Mover fm = other.GetComponent<Feeder_Mover>();
		//int dmg = 0; // initialize dmg amount
		//if (_forceType==ForceType.Push) dmg = 50; // alter dmg amount according to ability
		//if (_forceType==ForceType.Pull) dmg = 10; // pull does much less
		if (km) km.damage (); // reduce health here
		if (fm) fm.damage ();
	}
}
