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

  [RPC]
  public void SetLastSavedState()
  {
    this.lastSavedStates.Clear();
    foreach (KeyValuePair<NetworkViewID, GameObject> entry in this.objectsInLayer)
    {
      this.lastSavedStates.Add(DeftBodyStateUtil.BuildState(entry.Value));
    }
  }

  [RPC]
  public void LoadLastSavedState()
  {
    foreach (DeftBodyState state in this.lastSavedStates)
    {
      UpdateDeftBodyState(state);
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
        this.objectsInLayer[obj.networkView.viewID] = obj;
      }
    }
    this.players = GameObject.FindGameObjectsWithTag("Player");
    if (debug)
    {
      Debug.Log(this.objectsInLayer.Count + " objects tracked in layer " + this.layer + "  with " + this.players.Length + " players observed.");
    }
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
    DeftBodyState state = new DeftBodyState();
    state.position = position;
    state.rotation = rotation;
    state.timestamp = timestamp;
    state.velocity = velocity;
    state.angularVelocity = angularVelocity;
    state.id = id;
    this.objectsInLayer[state.id].GetComponent<DeftSyncWorker>().goalState = state;
    if (statistics)
    {
      this.statisticsManager.addLineOnSync(Time.time, id, this.objectsInLayer[state.id].gameObject.GetComponent<Rigidbody>().position, state.position);
    }
    this.objectsInLayer[state.id].GetComponent<DeftSyncWorker>().StartSync();
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
      DeftBodyState lastChecked = entry.Value.GetComponent<DeftSyncWorker>().lastCheckedState;
      float selfDistance = DeftBodyStateUtil.SquaredPositionalDifference(entry.Value, lastChecked);
      if (selfDistance > this.distanceThreshold || Time.time - lastChecked.timestamp > this.hardSyncThreshold)
      {
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
      else
      {
        this.statisticsSyncsSavedByDistanceThreshhold++;
        //if (debug)
        //{
        //    Debug.Log("No need to sync " + lastChecked.id.ToString() + "(selfDistance: " + selfDistance);
        //}
      }
    }
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
          this.networkView.RPC("UpdateDeftBodyStateRaw", RPCMode.OthersBuffered, state.position, state.rotation, (float)state.timestamp, state.velocity, state.angularVelocity, state.id);
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
