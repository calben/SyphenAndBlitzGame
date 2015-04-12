using UnityEngine;
using System.Collections;

public class TestingThrusters : MonoBehaviour {

  public float thrusterPower;

  [RPC]
  public void Activate()
  {
    this.gameObject.GetComponent<Rigidbody>().AddForce(this.gameObject.transform.forward * thrusterPower, ForceMode.Impulse);
  }

}
