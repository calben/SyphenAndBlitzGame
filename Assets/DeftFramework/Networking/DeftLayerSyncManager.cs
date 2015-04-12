using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Runtime.InteropServices;

public class DeftLayerSyncManager : MonoBehaviour
{

  public Dictionary<NetworkViewID, GameObject> objectsInLayer;
  public Queue<DeftBodyState> syncQueue;
  public List<DeftBodyState> lastSavedStates;
  DeftLayerSyncStatistics statisticsManager;
  public int layer;
  public float hardSyncThreshold = 0.0f;
  public float maxSyncRate = 0.0f;
  public float maxQueueBuildRate = 0.0f;
  public float distanceThreshold = 0.0f;
  public int updatesPerBundle = 5;

  public bool considerPlayer = true;
  public float tooCloseToPlayerSquaredDistance = 9.0f;
  public float tooFarFromPlayerSquaredDistance = 250.0f;

  public bool debug = false;
  public bool statistics = false;
  public int statisticsSyncsSavedByDistanceThreshhold;
  public int statisticsSyncsSavedByPlayerDistanceThreshholds;

  GameObject[] players;
  DeftBodyState lastSavedPlayerState;

  float resetLayerTimer = 10.0f;
  float resetLayerTimerTmp;

  [RPC]
  void SetLastSavedStateRPC()
  {
    Debug.Log("Saving!");
    this.lastSavedStates.Clear();
    foreach (KeyValuePair<NetworkViewID, GameObject> entry in this.objectsInLayer)
    {
      this.lastSavedStates.Add(DeftBodyStateUtil.BuildState(entry.Value));
    }
    foreach (GameObject p in players)
    {
      Debug.Log("Checking position for " + p.name);
      if (p.GetComponent<RigidbodyNetworkedPlayerController>().isThisMachinesPlayer)
      {
        this.lastSavedPlayerState = DeftBodyStateUtil.BuildState(p);
        Debug.Log(this.lastSavedPlayerState.position.ToString());
        this.lastSavedPlayerState.position = this.lastSavedPlayerState.position + new Vector3(0.0f, 5.0f, 0.0f);
        break;
      }
    }
  }

  public void SetLastSavedState()
  {
    if (Network.isClient || Network.isServer)
    {
      this.GetComponent<NetworkView>().RPC("SetLastSavedStateRPC", RPCMode.All);
    }
    else
    {
      this.SetLastSavedStateRPC();
    }
  }

  public void LoadLastSavedState()
  {
    Debug.Log("Loading!");
    if (Network.isClient || Network.isServer)
    {
      this.GetComponent<NetworkView>().RPC("LoadLastSavedStateRPC", RPCMode.All);
    }
    else
    {
      this.LoadLastSavedStateRPC();
    }
  }

  [RPC]
  void LoadLastSavedStateRPC()
  {
    Debug.Log("Loading saved state.");
    //foreach (DeftBodyState state in this.lastSavedStates)
    //{
    //  try
    //  {
    //    DeftBodyStateUtil.SetGameObjectToDeftBodyStateValues(this.objectsInLayer[state.id], state);
    //  }
    //  catch (MissingReferenceException e)
    //  {
    //    Debug.Log(e.Message);
    //  }
    //}
    foreach (GameObject p in players)
    {
      Debug.Log("Checking position for " + p.name);
      if (p.GetComponent<RigidbodyNetworkedPlayerController>().isThisMachinesPlayer)
      {
        Debug.Log("Setting player");
        DeftBodyStateUtil.SetGameObjectToDeftBodyStateValues(p, this.lastSavedPlayerState);
        GameObject.Find("GameManager").GetComponent<GameManager>().playerCurrentHealth = GameObject.Find("GameManager").GetComponent<GameManager>().playerTotalHealth;
      }
    }
  }

  [RPC]
  public void SetObjectsInLayer()
  {
    this.objectsInLayer.Clear();
    foreach (GameObject obj in FindObjectsOfType<GameObject>())
    {
      if (obj.layer == this.layer)
      {
        this.objectsInLayer[obj.GetComponent<NetworkView>().viewID] = obj;
      }
    }
    this.players = GameObject.FindGameObjectsWithTag("Player");
    Debug.Log(this.objectsInLayer.Count + " objects tracked in layer " + this.layer + "  with " + this.players.Length + " players observed.");
  }

  [RPC]
  public void UpdateMarshalledDeftBodyState(byte[] bytes)
  {
    DeftBodyState state = DeftBodyStateUtil.UnMarshalDeftBodyState(bytes);
    Debug.Log("Updating deft body state for " + state.id.ToString());
    this.objectsInLayer[state.id].GetComponent<DeftSyncWorker>().goalState = state;
    this.objectsInLayer[state.id].GetComponent<DeftSyncWorker>().StartSync();
  }


  [RPC]
  public void UpdateDeftBodyState(DeftBodyState state)
  {
    Debug.Log("Updating deft body state for " + state.id.ToString());
    this.objectsInLayer[state.id].GetComponent<DeftSyncWorker>().goalState = state;
    this.objectsInLayer[state.id].GetComponent<DeftSyncWorker>().StartSync();
  }

  [RPC]
  public void UpdateDeftBodyStateRaw(Vector3 position, Quaternion rotation, float timestamp, Vector3 velocity, Vector3 angularVelocity, NetworkViewID id)
  {
    if (debug)
    {
      Debug.Log("Updating deft body state.");
    }
    DeftBodyState state = new DeftBodyState();
    state.position = position;
    state.rotation = rotation;
    state.timestamp = timestamp;
    state.velocity = velocity;
    state.angularVelocity = angularVelocity;
    state.id = id;
    DeftBodyStateUtil.SetGameObjectToDeftBodyStateValues(this.objectsInLayer[id], state);
  }

  void BuildSyncQueue()
  {
    DeftBodyState[] playerStates = new DeftBodyState[this.players.Length];
    for (int i = 0; i < this.players.Length; i++)
    {
      playerStates[i] = DeftBodyStateUtil.BuildState(this.players[i]);
    }
    foreach (KeyValuePair<NetworkViewID, GameObject> entry in this.objectsInLayer)
    {
      DeftBodyState lastChecked = DeftBodyStateUtil.BuildState(entry.Value);
      //DeftBodyState lastChecked = entry.Value.GetComponent<DeftSyncWorker>().lastCheckedState;
      //float selfDistance = DeftBodyStateUtil.SquaredPositionalDifference(entry.Value, lastChecked);
      //if (selfDistance > this.distanceThreshold || Time.time - lastChecked.timestamp > this.hardSyncThreshold)
      //{
      if (debug)
      {
        Debug.Log("Adding " + entry.Key + "to sync queue with selfDistance difference of " + DeftBodyStateUtil.SquaredPositionalDifference(entry.Value, lastChecked));
      }
      if (this.considerPlayer)
      {
        bool sync = true;
        foreach (DeftBodyState playerState in playerStates)
        {
          if (DeftBodyStateUtil.SquaredPositionalDifference(playerState, lastChecked) > this.tooFarFromPlayerSquaredDistance || DeftBodyStateUtil.SquaredPositionalDifference(playerState, lastChecked) < this.tooCloseToPlayerSquaredDistance)
          {
            sync = false;
          }
        }
        if (sync)
        {
          entry.Value.GetComponent<DeftSyncWorker>().lastCheckedState = DeftBodyStateUtil.BuildState(entry.Value);
          this.syncQueue.Enqueue(lastChecked);
        }
        else
        {
          this.statisticsSyncsSavedByPlayerDistanceThreshholds++;
        }
      }
      else
      {
        entry.Value.GetComponent<DeftSyncWorker>().lastCheckedState = DeftBodyStateUtil.BuildState(entry.Value);
        this.syncQueue.Enqueue(lastChecked);
      }
    }
    //else
    //{
    //  this.statisticsSyncsSavedByDistanceThreshhold++;
    //  //if (debug)
    //  //{
    //  //    Debug.Log("No need to sync " + lastChecked.id.ToString() + "(selfDistance: " + selfDistance);
    //  //}
    //}
    //}
    if (debug)
    {
      Debug.Log("Sync queue rebuilt with " + this.syncQueue.Count + " objects ready to sync.");
    }
  }

  void Start()
  {
    foreach (NetworkView netView in this.GetComponents<NetworkView>())
    {
      netView.observed = this;
    }
    this.objectsInLayer = new Dictionary<NetworkViewID, GameObject>();
    this.syncQueue = new Queue<DeftBodyState>();
    this.lastSavedStates = new List<DeftBodyState>();
    this.SetObjectsInLayer();
    if (statistics)
    {
      this.statisticsManager = this.gameObject.GetComponent<DeftLayerSyncStatistics>();
      this.statisticsManager.objectsToTrack = new List<GameObject>();
      foreach (GameObject obj in this.objectsInLayer.Values)
      {
        this.statisticsManager.objectsToTrack.Add(obj);
      }
      this.statisticsManager.addHeaderOnSync();
      this.statisticsManager.addHeaderFullState();
    }
  }

  public float maxSyncRateTmp;
  public float maxQueueBuildRateTmp;
  void FixedUpdate()
  {
    this.maxSyncRateTmp += Time.deltaTime;
    this.maxQueueBuildRateTmp += Time.deltaTime;
    if (Network.isServer)
    {
      int i = 0;
      while (i < this.updatesPerBundle && this.syncQueue.Count > 0)
      {
        if (maxSyncRateTmp > maxSyncRate)
        {
          //DeftBodyState state = DeftBodyStateUtil.BuildState(this.objectsInLayer[this.syncQueue.Dequeue().id]);
          DeftBodyState state = DeftBodyStateUtil.BuildState(this.objectsInLayer[this.syncQueue.Dequeue().id]);
          if (debug)
          {
            Debug.Log(Time.time + ": Sending " + state.id.ToString());
          }
          //this.networkView.RPC("UpdateDeftBodyState", RPCMode.AllBuffered, DeftBodyStateUtil.MarshallDeftBodyState(state));
          this.GetComponent<NetworkView>().RPC("UpdateDeftBodyStateRaw", RPCMode.OthersBuffered, state.position, state.rotation, (float)state.timestamp, state.velocity, state.angularVelocity, state.id);
          UpdateDeftBodyStateRaw(state.position, state.rotation, (float)state.timestamp, state.velocity, state.angularVelocity, state.id);
          this.maxSyncRateTmp = 0.0f;
        }
        i++;
      }
      if (this.syncQueue.Count == 0)
      {
        BuildSyncQueue();
      }
    }
  }

}
