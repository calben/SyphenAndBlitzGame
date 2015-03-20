using UnityEngine;
using System.IO;
using System.Collections;

public class DeftLayerSyncStatistics : MonoBehaviour
{
  public string fileName = System.IO.Directory.GetCurrentDirectory() + "/syncStatistics.csv";
  System.IO.StreamWriter file;

  public float flushTimer = 1.0f;
  float acc;

  public void addHeader()
  {
    file.WriteLine("time,view_id,current_position_x,current_position_y,current_position_z,goal_position_x,goal_position_z,goal_position_z,positional_difference_square_magnitude");
  }

  public void addLine(float time, NetworkViewID viewId, Vector3 currentPosition, Vector3 goalPosition)
  {
    string tmp = time.ToString() + "," + viewId.ToString() + "," + currentPosition.ToString() + "," + goalPosition.ToString() + "," + Vector3.SqrMagnitude(currentPosition - goalPosition);
    tmp = tmp.Replace("(", "");
    tmp = tmp.Replace(")", "");
    file.WriteLine(tmp);
  }

  // Use this for initialization
  void Start()
  {
    if (!this.gameObject.GetComponent<DeftLayerSyncManager>().statistics)
    {
      this.GetComponent<DeftLayerSyncStatistics>().enabled = false;
      return;
    }
    Debug.Log("Creating statistics log at " + fileName);
    try
    {
      file = new System.IO.StreamWriter(fileName);
    }
    catch (IOException)
    {
      this.GetComponent<DeftLayerSyncStatistics>().enabled = false;
    }
    try
    {
      addHeader();
    }
    catch (IOException)
    {
      this.GetComponent<DeftLayerSyncStatistics>().enabled = false;
    }
  }

  // Update is called once per frame
  void FixedUpdate()
  {
    acc += Time.deltaTime;
    if (acc >= flushTimer)
    {
      file.Flush();
      acc = 0.0f;
    }
  }
}
