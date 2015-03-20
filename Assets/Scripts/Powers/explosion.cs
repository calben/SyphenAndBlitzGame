using UnityEngine;
using System.Collections;

public class explosion : MonoBehaviour {
	public float radius;
	public float power;
	public float lift;
	public float delay;
	public GameObject effect;
	public AudioManager _audioManager;
	public float liftTime;
	public float liftMag;
	private int maxLift = 1000000;

	void Start(){
		StartCoroutine ("MyMethod");
	}

	void DealDamage(Collider other){
		int dmg = 50;
		if (other.gameObject.tag == "Feeder") {
			Feeder_Mover fm = other.GetComponent<Feeder_Mover>();
			fm.health -= dmg;
		}
		else if (other.gameObject.tag == "Killer") {
			Killer_Mover km = other.GetComponent<Killer_Mover>();
			km.health -= dmg;
		}
	}

	IEnumerator Lift(Collider[] colliders, float airTime){
		//Debug.Log("got into lift!");
		float curTime = 0;
		while (curTime < airTime) {
			//Debug.Log("lifting stuff");
			curTime += Time.deltaTime;
			foreach (Collider hit in colliders) {
				PhysicsStatus ps = (PhysicsStatus) hit.GetComponent<PhysicsStatus>();
				if( ps && (ps.liftable||ps.pushable) && hit.attachedRigidbody){ 
					hit.rigidbody.AddForce( Vector3.up * Mathf.Clamp(liftMag/Vector3.Magnitude(this.transform.position - hit.transform.position), 0, maxLift) , ForceMode.Impulse);
				}
			}
		}
		yield return null;
	}

	IEnumerator MyMethod() {
		_audioManager.Initialize();
		_audioManager.Play("grenade_beeping", 0.25f, false);
		yield return new WaitForSeconds(delay);
		_audioManager.Play("grenade_explosion", 0.0f, false);

		Vector3 explosionPos = transform.position;
		Collider[] colliders = Physics.OverlapSphere(explosionPos, radius);

		float airTime = liftTime;

		yield return StartCoroutine (Lift (colliders, airTime)); // should be waiting for coroutine to finish
		Debug.Log ("Returned from lift");

		Instantiate(effect, explosionPos, transform.rotation);
		//Vector3 explosionPos = transform.position;

		foreach (Collider hit in colliders) {
			// can add cases for non physics objects later
			DealDamage(hit); // call to method that hurts ai

			PhysicsStatus ps = (PhysicsStatus) hit.GetComponent<PhysicsStatus>(); // grab physics status of object
			if( ps && ps.pushable ){ 
				Shatterable shatterable = hit.gameObject.GetComponent<Shatterable>();
				if(shatterable){
					shatterable.switchToFractured(); // shatter shatterables
				} else if(hit.attachedRigidbody){
					hit.GetComponent<PhysicsStatus>().pullable = true; // switch objects to pullable
				}
			}
			if (hit && hit.rigidbody){
				//and then still apply explosion force to any rigidbodies
				if (hit.gameObject.tag!="Player"){ // except the players themselves
					hit.rigidbody.AddExplosionForce(power, explosionPos, radius, lift, ForceMode.Impulse); 
				}
			}
		}
		_audioManager.Play("grenade_explosion", 0.0f, false);
		Destroy (gameObject);
	}
}
