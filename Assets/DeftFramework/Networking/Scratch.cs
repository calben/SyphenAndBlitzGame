using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

public struct CustomRigidBodyState
{
  // OPTIONAL
  public NetworkViewID id;
  public double timestamp;

  // NOT OPTIONAL
  public Vector3 position;
  public Vector3 velocity;
  public Quaternion rotation;
  public Vector3 angularVelocity;
}

public class Scratch : MonoBehaviour {

	// Use this for initialization
	void Start () {
    CustomRigidBodyState c = new CustomRigidBodyState();
    Debug.Log("ID: " + Marshal.SizeOf(c.id));
    Debug.Log("POS: " + Marshal.SizeOf(c.position));
    Debug.Log("ROT: " + Marshal.SizeOf(c.rotation));
    Debug.Log("RBS: " + Marshal.SizeOf(c));
    Debug.Log("RIGID: " + Marshal.SizeOf(this.gameObject.GetComponent<Rigidbody>()));
	}
	
}
