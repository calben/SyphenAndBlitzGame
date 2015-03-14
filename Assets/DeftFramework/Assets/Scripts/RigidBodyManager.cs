using UnityEngine;
using System.Collections.Generic;

public enum SyncHandler { snap, simplesmoothing, firstorder, secondorder, adaptivehigherorder };

public struct BodyState
{
  public double timestamp;
  public Vector3 pos;
  public Vector3 velocity;
  public Quaternion rot;
  public Vector3 angularVelocity;
}

public class RigidBodyManager : MonoBehaviour
{

  public Dictionary<int, GameObject> objectDictionary;
  public Queue<GameObject> objectsToSync;
  public Dictionary<GameObject, BodyState> objecToState;
  public HashSet<GameObject> allTrackedObjects;

  // syncs in order of priority
  // priority suggests how many to sync per syncing set
  public Dictionary<int, Queue<GameObject>> objectsByPriority;

  public int trackingLayer;
  public bool isShowingDebug;
  public bool isShowingLatency;

  public int playerPriority; // between 1 and 10
  // indicates number of player prioritised items to sync
  // before queue is rebuilt

  public SyncHandler syncHandler;

  public bool prioritizeByPlayerDistance;
  public float playerPriorityProximity;
  public float playerDontSyncProximity;

  public float timeBetweenSyncHighPriority = 0.1f;
  public float timeBetweenSyncLowPriority = 1.0f;
  public float timeMaximumUnsynced = 5.0f;

  float timeLastHighPrioritySync;
  float timeLastLowPrioritySync;
  float timeLastAllSynced;

  public bool useVisualizer;


  void Start()
  {
    objectDictionary = new Dictionary<int, GameObject>();
    objectsToSync = new Queue<GameObject>();
    allTrackedObjects = new HashSet<GameObject>();
    objectsByPriority = new Dictionary<int, Queue<GameObject>>();
    for (int i = 1; i < 11; i++)
    {
      objectsByPriority[i] = new Queue<GameObject>();
    }
  }

  public void ResetTrackedObjects()
  {
    allTrackedObjects.Clear();
    for (int i = 1; i < 11; i++)
    {
      objectsByPriority[i] = new Queue<GameObject>();
    }
    foreach (GameObject obj in FindObjectsOfType<GameObject>())
    {
      if (obj.layer == trackingLayer)
      {
        allTrackedObjects.Add(obj);
        objectDictionary[obj.GetInstanceID()] = obj;
      }
    }
    if (isShowingDebug)
      Debug.Log("Tracking " + allTrackedObjects.Count + " objects for automated networking.");
  }

  [RPC]
  public void ShareTrackedObjectList()
  {
    if (Network.isServer)
    {
      this.ResetTrackedObjects();
      foreach (GameObject obj in this.objectsToSync)
      {
        // sync objects
      }
    }
    if (Network.isClient)
    {
      this.objectsToSync.Clear();

    }
  }

  // Use this for initialization
  void Awake()
  {
    foreach (NetworkView n in GetComponents<NetworkView>())
      n.observed = this;
  }

  /// <summary>
  /// These variables are class fields for efficiency purposes when serialising.
  /// </summary>
  Vector3 pos;
  Quaternion rot;
  Vector3 velocity;
  Vector3 angular_velocity;
  int id;

  void SyncObject(GameObject obj)
  {
    if (isShowingDebug)
    {
      Debug.Log("Syncing object: " + obj.GetInstanceID());
    }
    switch (syncHandler)
    {
      case SyncHandler.snap:
        SnapSync(obj);
        break;
      case SyncHandler.simplesmoothing:
        FirstOrderSync(obj);
        break;
      case SyncHandler.firstorder:
        FirstOrderSync(obj);
        break;
      case SyncHandler.secondorder:
        //SecondOrderSync(obj);
        break;
    }
  }

  void SnapSync(GameObject obj)
  {
    obj.transform.position = pos;
    obj.rigidbody.velocity = velocity;
    obj.rigidbody.rotation = rot;
    obj.rigidbody.angularVelocity = angular_velocity;
  }

  void FirstOrderSync(GameObject obj)
  {
    obj.transform.position = Vector3.Lerp(obj.transform.position, pos, 0.5f);
    obj.rigidbody.velocity = Vector3.Lerp(obj.rigidbody.velocity, velocity, 0.5f);
    obj.rigidbody.rotation = Quaternion.Slerp(obj.rigidbody.rotation, rot, 0.5f);
    obj.rigidbody.angularVelocity = Vector3.Lerp(obj.rigidbody.angularVelocity, angular_velocity, 0.5f);
  }

  void PHBRSync(BodyState[] states)
  {
    // assuming states in correct order
    // assuming timesteps approximately equal
    // assuming order with 0..n being most recent to least recent

    // direct from a paper
    // maybe not be best but don't mess with this please
    // -- calben
    while (states[0].timestamp < Time.time)
    {
      float d1 = (float)(states[0].timestamp - states[1].timestamp);
      float d2 = (float)(states[1].timestamp - states[2].timestamp);
      BodyState update = new BodyState();
      float tmp1 = 2 * d1 * d1 / d2 / (d1 + d2);
      float tmp2 = 2 * d1 / d2 + 1;
      float tmp3 = 2 * d1 / (d1 + d2) + 1;
      update.pos = tmp1 * states[3].pos - tmp2 * states[1].pos + tmp3 * states[0].pos;
      update.velocity = 1 / (2 * d1) * states[1].pos - 2 / d1 * states[0].pos + 3 / (2 * d1) * update.pos;
      update.timestamp = states[0].timestamp + d1;
      for (int i = states.Length - 1; i >= 1; i--)
      {
        states[i] = states[i - 1];
      }
    }
  }


  void SetPlayerPriorityObjects()
  {
    foreach (GameObject player in GameObject.FindGameObjectsWithTag("Player"))
    {
      Vector3 player_position = player.transform.position;
      foreach (GameObject tracked in objectsToSync)
      {
        double distance = Vector3.Distance(player_position, tracked.transform.position);
        // this strcture can reduce passes
        // preferably don't modify this
        if (distance < this.playerDontSyncProximity)
        {
          continue;
        }
        else if (distance < this.playerPriorityProximity)
        {
          objectsByPriority[this.playerPriority].Enqueue(tracked);
        }
        else
        {
          continue;
        }
      }
    }
  }

  void FixedUpdate()
  {
    if (prioritizeByPlayerDistance)
    {
      SetPlayerPriorityObjects();
    }
  }

  public void BuildSyncQueue()
  {
    for(int i = 1; i < 11; i++)
    {
      if (this.objectsByPriority[i].Count > 0)
      {
        int j = 0;
        while (j < i && this.objectsByPriority[i].Count > 0)
        {
          this.objectsToSync.Enqueue(this.objectsByPriority[i].Dequeue());
        }
      }
    }
  }

  void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info)
  {
    if (this.useVisualizer)
    {
      // note using the visualizer will probably hugely slow down your simulation
      this.gameObject.GetComponent<RigidBodyManagerVisualizer>().ShowVisualization();
    }
    if (isShowingDebug)
    {
      Debug.Log("Last low priority sync time: " + timeLastLowPrioritySync);
      Debug.Log("Current time: " + Time.time);
    }

    if (this.objectsToSync.Count > 0)
    {
      timeLastHighPrioritySync = Time.time;
      GameObject obj = this.objectsToSync.Dequeue();
      {
        if (stream.isWriting)
        {
          pos = obj.rigidbody.position;
          rot = obj.rigidbody.rotation;
          velocity = obj.rigidbody.velocity;
          angular_velocity = obj.rigidbody.angularVelocity;
          id = obj.GetInstanceID();
          stream.Serialize(ref pos);
          stream.Serialize(ref velocity);
          stream.Serialize(ref rot);
          stream.Serialize(ref angular_velocity);
          stream.Serialize(ref id);
        }
        else
        {
          stream.Serialize(ref pos);
          stream.Serialize(ref velocity);
          stream.Serialize(ref rot);
          stream.Serialize(ref angular_velocity);
          stream.Serialize(ref id);
          if(isShowingDebug)
          {
            Debug.Log("Received " + id);
          }
          SyncObject(objectDictionary[id]);
        }
      }
    }
  }
}
