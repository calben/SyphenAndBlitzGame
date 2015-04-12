using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public class DeftLayerSyncStatistics : MonoBehaviour
{
  public string onsyncfilename;
  public string objecttrackingfilename;
  System.IO.StreamWriter onsyncfile;
  System.IO.StreamWriter objectTrackingFile;
  public List<GameObject> objectsToTrack;

  public float flushTimer = 1.0f;
  float acc;

  public void addHeaderOnSync()
  {
    onsyncfile.WriteLine("time,view_id,current_position_x,current_position_y,current_position_z,goal_position_x,goal_position_z,goal_position_z,positional_difference_square_magnitude");
  }

  public void addLineOnSync(float time, NetworkViewID viewId, Vector3 currentPosition, Vector3 goalPosition)
  {
    string tmp = Network.time.ToString() + "," + viewId.ToString() + "," + currentPosition.ToString() + "," + goalPosition.ToString() + "," + Vector3.SqrMagnitude(currentPosition - goalPosition);
    tmp = tmp.Replace("(", "");
    tmp = tmp.Replace(")", "");
    onsyncfile.WriteLine(tmp);
  }

  public void addHeaderFullState()
  {
    string tmp = "time";
    foreach (GameObject state in this.objectsToTrack)
    {
      foreach (string v in new string[] { "x", "y", "z" })
        tmp += "," + state.GetComponent<NetworkView>().viewID.GetHashCode() + "-" + v;
    }
    this.objectTrackingFile.Write(tmp + "\n");
  }

  public void addLineToFullState()
  {
    string tmp = Network.time.ToString();
    foreach (GameObject state in this.objectsToTrack)
    {
      tmp += "," + state.transform.position.x.ToString() + "," + state.transform.position.y.ToString() + "," + state.transform.position.z.ToString();
    }
    this.objectTrackingFile.WriteLine(tmp);
  }

  void Awake()
  {
    if (!this.gameObject.GetComponent<DeftLayerSyncManager>().statistics)
    {
      this.GetComponent<DeftLayerSyncStatistics>().enabled = false;
      return;
    }
    onsyncfilename = System.IO.Directory.GetCurrentDirectory() + "/sync-statistics.csv";
    objecttrackingfilename = System.IO.Directory.GetCurrentDirectory() + "/full-state-tracking.csv";
    Debug.Log("Creating statistics log at " + onsyncfilename);
    onsyncfile = new System.IO.StreamWriter(onsyncfilename);
    Debug.Log("Creating full state tracking log at " + this.objecttrackingfilename);
    this.objectTrackingFile = new System.IO.StreamWriter(this.objecttrackingfilename);
    addHeaderFullState();
  }

  void Update()
  {
    if (Network.isServer || Network.isClient)
    {
      acc += Time.deltaTime;
      this.addLineToFullState();
    }
    if (acc >= flushTimer)
    {
      onsyncfile.Flush();
      this.objectTrackingFile.Flush();
      acc = 0.0f;
    }
  }


}
