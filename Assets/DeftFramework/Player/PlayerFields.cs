using UnityEngine;
using System.Collections;

public class PlayerFields : MonoBehaviour
{

  public float health = 100.0f;
  public bool debug = true;


  void OnCollisionEnter(Collision collision)
  {
    if (debug)
    {
      Debug.Log("Player hit collider with magnitude of " + collision.relativeVelocity.magnitude);
    }
    this.health -= collision.relativeVelocity.magnitude;
  }
}
