using UnityEngine;
using System.Collections;

// DEPRECATED, DO NOT USE

public enum ForceNodeType { Push, Pull, Lift }
public enum DistanceRelationship { None, Linear }
public enum NodeColliderType { Radial, Box, Custom };

public class ForceNodeBehaviour : MonoBehaviour
{

  public float magnitudeBirth;
  public float magnitudeDeath;
  public float magnitudeRatio;
  public float radiusBirth;
  public float radiusDeath;
  public float radiusRatio;
  public ForceNodeType gravityType;
  public NodeColliderType colliderType;
  public DistanceRelationship distanceType;

  public bool timed;
  public bool affectEverything;
  public float duration;

  public float currentMagnitude;


  void Awake()
  {
    switch (colliderType)
    {
      case NodeColliderType.Radial:
        if (this.GetComponent<SphereCollider>() == null)
          this.gameObject.AddComponent<SphereCollider>();
        this.GetComponent<SphereCollider>().radius = this.radiusBirth;
        this.GetComponent<SphereCollider>().isTrigger = true;
        break;
      case NodeColliderType.Box:
        this.gameObject.AddComponent<BoxCollider>();
        break;
      case NodeColliderType.Custom:
        break;
    }
    currentMagnitude = magnitudeBirth;
  }

  /// <summary>
  /// FOR NOW ONLY SPHERE COLLIDER WORKS, THIS IS PROBLEMATIC
  /// </summary>
  void FixedUpdate()
  {
    if (timed)
    {
      this.gameObject.GetComponent<SphereCollider>().radius = Mathf.Lerp(magnitudeBirth, magnitudeDeath, radiusRatio * Time.deltaTime);
      currentMagnitude = Mathf.Lerp(radiusBirth, radiusDeath, radiusRatio * Time.deltaTime);
      duration -= Time.deltaTime;
    }
  }

  void OnTriggerStay(Collider other)
  {
    if (other.rigidbody.useGravity)
    {
      PhysicsStatus status = other.gameObject.GetComponent<PhysicsStatus>();
      Vector3 direction = Vector3.Normalize(other.transform.position - this.transform.position);
      switch (gravityType)
      {
        case ForceNodeType.Lift:
          if (status == null || status.liftable || affectEverything)
          {
            switch (distanceType)
            {
              case DistanceRelationship.Linear:
                other.rigidbody.AddForce(Vector3.up * (currentMagnitude / Vector3.Magnitude(other.transform.position - this.transform.position)), ForceMode.Impulse);
                break;
              case DistanceRelationship.None:
                other.rigidbody.AddForce(Vector3.up * (currentMagnitude), ForceMode.Impulse);
                break;
            }
          }
          break;
        case ForceNodeType.Pull:
          if (status == null || status.pullable || affectEverything)
          {
            switch (distanceType)
            {
              case DistanceRelationship.Linear:
                other.rigidbody.AddForce(direction * (-1 * currentMagnitude / Vector3.Magnitude(other.transform.position - this.transform.position)), ForceMode.Impulse);
                break;
              case DistanceRelationship.None:
                other.rigidbody.AddForce(direction * (-1 * currentMagnitude), ForceMode.Impulse);
                break;
            }
          }
          break;
        case ForceNodeType.Push:
          if (status == null || status.pushable || affectEverything)
          {
            switch (distanceType)
            {
              case DistanceRelationship.Linear:
                other.rigidbody.AddForce(direction * (currentMagnitude / Vector3.Magnitude(other.transform.position - this.transform.position)), ForceMode.Impulse);
                break;
              case DistanceRelationship.None:
                other.rigidbody.AddForce(direction * (currentMagnitude), ForceMode.Impulse);
                break;
            }
          }
          break;
      }
    }
  }
}
