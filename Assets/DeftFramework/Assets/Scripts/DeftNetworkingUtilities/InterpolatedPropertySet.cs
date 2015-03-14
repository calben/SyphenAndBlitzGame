using UnityEngine;
using System.Collections;

public class InterpolatedPropertySet : MonoBehaviour
{

  float positionChangeThreshold = 0.5f;
  float velocityChangeThreshold = 0.5f;
  float rotationChangeThreshold = 0.5f;
  float angularVelocityChangeThreshold = 0.5f;
  public static int bufferSize = 10;
  public static double interpolationBackTime = 0.1;
  public static double extrapolationLimit = 0.5;
  DeftState lastRegistered;
  DeftState[] buffer;
  DeftState[] predictedStates;
  DeftState knownTargetState;

  bool smoothCorrections;

  public bool hasMovedBeyondThreshold(DeftState a, DeftState b)
  {
    if (Vector3.Distance(a.position, b.position) > positionChangeThreshold)
      return true;
    return false;
  }

  public bool hasMovedBeyondThreshold()
  {
    return hasMovedBeyondThreshold(buffer[0], buffer[1]);
  }

  public void addToBuffer(DeftState state)
  {
    for (int i = this.buffer.Length - 1; i > 0; i--)
    {
      this.buffer[i] = this.buffer[i - 1];
    }
    this.buffer[0] = state;
  }

  void Start()
  {
    buffer = new DeftState[bufferSize];
  }

  void FixedUpdate()
  {
    double interpolationTime = Network.time - interpolationBackTime;
    if (this.buffer[0] != null)
    {
      if (this.buffer[0].time > interpolationTime)
      {
        for (int i = 0; i < buffer.Length; i++)
        {
          if (buffer[i] != null)
          {
            DeftState rhs = buffer[Mathf.Max(i - 1, 0)];
            DeftState lhs = buffer[i];
            float t = 0.0f;
            if (rhs.time - lhs.time > 0.001f)
              t = (float)((interpolationTime - lhs.time) / (rhs.time - lhs.time));
            this.transform.localPosition = Vector3.Lerp(lhs.position, rhs.position, t);
            this.transform.localRotation = Quaternion.Slerp(lhs.rotation, rhs.rotation, t);
          }
        }
      }
      else
      {
        float extrapolationLength = (float)(interpolationTime - buffer[0].time);
        if (extrapolationLength < extrapolationLimit)
        {
          float axisLength = extrapolationLength * buffer[0].angularVelocity.magnitude * Mathf.Rad2Deg;
          Quaternion angularRotation = Quaternion.AngleAxis(axisLength, buffer[0].angularVelocity);

          rigidbody.position = buffer[0].position + buffer[0].velocity * extrapolationLength;
          rigidbody.rotation = angularRotation * buffer[0].rotation;
          rigidbody.velocity = buffer[0].velocity;
          rigidbody.angularVelocity = buffer[0].angularVelocity;
        }
      }
    }
  }

}
