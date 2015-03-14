using UnityEngine;
using System.Collections.Generic;

public class ObjectManager : MonoBehaviour
{

  public List<GameObject> trackedObjects;
  public int layer = 10;
  float timeBetweenSync = 0.1f;
  float timeBetweenAllSync = 0.3f;

  float lastSyncTime;
  float lastAllSyncTime;
  
  // Use this for initialization
  void Start()
  {
    foreach (NetworkView n in GetComponents<NetworkView>())
      n.observed = this;
    trackedObjects = new List<GameObject>();
    foreach (GameObject obj in FindObjectsOfType<GameObject>())
    {
      if (obj.layer == layer)
      {
        trackedObjects.Add(obj);
      }
    }
    Debug.Log("Tracking " + trackedObjects.Count + " objects for automated networking.");
    foreach (GameObject obj in trackedObjects)
    {
      obj.AddComponent<InterpolatedPropertySet>();
    }
  }

  void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info)
  {
    Debug.Log(lastSyncTime);
    Debug.Log(Time.time);
    if (Time.time - lastSyncTime > timeBetweenSync)
    {
      Debug.Log("Syncing set.");
      lastSyncTime = Time.time;
      foreach (GameObject obj in trackedObjects)
      {
        //if (obj.GetComponent<InterpolatedPropertySet>().hasMovedBeyondThreshold() || true)
        {
          Debug.Log("Syncing movement data for " + obj.name);
          if (stream.isWriting)
          {
            Vector3 pos = obj.rigidbody.position;
            Quaternion rot = obj.rigidbody.rotation;
            Vector3 velocity = obj.rigidbody.velocity;
            Vector3 angularVelocity = obj.rigidbody.angularVelocity;

            stream.Serialize(ref pos);
            stream.Serialize(ref velocity);
            stream.Serialize(ref rot);
            stream.Serialize(ref angularVelocity);
          }
          else
          {
            Vector3 pos = Vector3.zero;
            Vector3 velocity = Vector3.zero;
            Quaternion rot = Quaternion.identity;
            Vector3 angularVelocity = Vector3.zero;
            stream.Serialize(ref pos);
            stream.Serialize(ref velocity);
            stream.Serialize(ref rot);
            stream.Serialize(ref angularVelocity);
            // DeftState state = new DeftState();
            obj.transform.position = pos;
            obj.rigidbody.velocity = velocity;
            obj.rigidbody.rotation = rot;
            obj.rigidbody.angularVelocity = angularVelocity;
            // obj.time = Time.time;
            // obj.GetComponent<InterpolatedPropertySet>().addToBuffer(state);
          }
        }
      }
    }
  }
}
