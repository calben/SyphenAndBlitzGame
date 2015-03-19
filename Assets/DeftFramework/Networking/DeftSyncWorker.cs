using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum SyncWorkerType { snap, firstorder, secondorder, adaptivehigherorder };

public class DeftSyncWorker : MonoBehaviour
{

  public DeftBodyState goalState;
  public DeftBodyState lastCheckedState;
  public LinkedList<DeftBodyState> stateHistory = new LinkedList<DeftBodyState>();
  public bool moveToState;
  public float duration = 1.0f;
  public float syncThreshhold = 1.0f;
  public bool debug = true;
  public SyncWorkerType workerType = SyncWorkerType.firstorder;
  float durationTmp;

  public void StartSync()
  {
    this.durationTmp = duration;
  }

  void FixedUpdate()
  {
    if (durationTmp > 0 && DeftBodyStateUtil.SquaredPositionalDifference(goalState, lastCheckedState) > syncThreshhold)
    {
      switch (this.workerType)
      {
        case SyncWorkerType.firstorder:
          FirstOrderSync(this.goalState);
          break;
        case SyncWorkerType.snap:
          SnapSync(this.goalState);
          break;
      }
    }
  }

  void FirstOrderSync(DeftBodyState state)
  {
    float lerpSpeed = this.duration / Time.fixedDeltaTime;
    this.gameObject.transform.position = Vector3.Lerp(this.gameObject.transform.position, state.position, lerpSpeed);
    this.gameObject.rigidbody.velocity = Vector3.Lerp(this.gameObject.rigidbody.velocity, state.velocity, lerpSpeed);
    this.gameObject.rigidbody.rotation = Quaternion.Slerp(this.gameObject.rigidbody.rotation, state.rotation, lerpSpeed);
    this.gameObject.rigidbody.angularVelocity = Vector3.Lerp(this.gameObject.rigidbody.angularVelocity, state.angularVelocity, lerpSpeed);
    if (debug)
    {
      Debug.Log(durationTmp + ": moving " + state.id + " to " + state.position.ToString());
    }
  }

  void SnapSync(DeftBodyState state)
  {
    this.gameObject.transform.position = state.position;
    this.gameObject.GetComponent<Rigidbody>().velocity = state.velocity;
    this.gameObject.GetComponent<Rigidbody>().rotation = state.rotation;
    this.gameObject.GetComponent<Rigidbody>().angularVelocity = state.angularVelocity;
    this.durationTmp = 0.0f;
    if (debug)
    {
      Debug.Log(durationTmp + ": moving " + state.id + " to " + state.position.ToString());
    }
  }

  void SecondOrderSync(BodyState[] states)
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

  bool detectBlockingCollision()
  {
    Vector3 currentPosition = this.GetComponent<Rigidbody>().position;
    Vector3 goalPosition = this.goalState.position;
    return false;
  }
}
